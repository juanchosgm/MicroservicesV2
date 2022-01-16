using System;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PlatformService.Dtos;
using RabbitMQ.Client;

namespace PlatformService.AsyncDataServices
{
    public class MessageBusClient : IMessageBusClient, IDisposable
    {
        private readonly IConfiguration configuration;
        private readonly ILogger<MessageBusClient> logger;
        private readonly IConnection connection;
        private readonly IModel channel;

        public MessageBusClient(IConfiguration configuration, ILogger<MessageBusClient> logger)
        {
            this.configuration = configuration;
            this.logger = logger;
            var factory = new ConnectionFactory
            {
                HostName = configuration["RabbitMQHost"],
                Port = int.Parse(configuration["RabbitMQPort"])
            };
            try
            {
                connection = factory.CreateConnection();
                channel = connection.CreateModel();
                channel.ExchangeDeclare("trigger", ExchangeType.Fanout);
                connection.ConnectionShutdown += RabbitMQConnectionShutdown;
                logger.LogInformation("--> Connected to MessageBus");
            }
            catch (Exception ex)
            {
                logger.LogError($"--> Could not connect to Message Bus: {ex.Message}");
            }
        }

        public void PublishNewPlatform(PlatformPublishedDto platformPublished)
        {
            var message = JsonSerializer.Serialize(platformPublished);
            if (connection.IsOpen)
            {
                logger.LogInformation("--> RabbitMQ Connection Open, sending message...");
                SendMessage(message);
            }
            else
            {
                logger.LogInformation("--> RabbitMQ Connection closed, not sending");
            }
        }

        private void SendMessage(string message)
        {
            var body = Encoding.UTF8.GetBytes(message);
            channel.BasicPublish("trigger", string.Empty, body: body);
            logger.LogInformation($"--> We have sent {message}");
        }

        public void Dispose()
        {
            logger.LogInformation($"--> MessageBus Disposed");
            if (channel.IsOpen)
            {
                channel.Close();
                connection.Close();
            }
        }

        private void RabbitMQConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            logger.LogInformation("--> RabbitMQ Connection Shutdown");
        }
    }
}