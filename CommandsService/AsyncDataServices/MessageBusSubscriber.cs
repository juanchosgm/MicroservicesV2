using System.Text;
using CommandsService.EventProcessing;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CommandsService.AsyncDataServices
{
    public class MessageBusSubscriber : BackgroundService
    {
        private readonly IConfiguration configuration;
        private readonly IEventProcessor eventProcessor;
        private readonly ILogger<MessageBusSubscriber> logger;
        private IConnection? connection;
        private IModel? channel;
        private string? queueName;

        public MessageBusSubscriber(IConfiguration configuration, IEventProcessor eventProcessor,
            ILogger<MessageBusSubscriber> logger)
        {
            this.configuration = configuration;
            this.eventProcessor = eventProcessor;
            this.logger = logger;
            InitializeRabbitMQ();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (sender, e) => 
            {
                logger.LogInformation("--> Event Received!");
                var body =  Encoding.UTF8.GetString(e.Body.ToArray());
                eventProcessor.ProcessEvent(body);
            };
            channel.BasicConsume(queueName, true, consumer: consumer);
            return Task.CompletedTask;
        }

        private void InitializeRabbitMQ()
        {
             var factory = new ConnectionFactory
             {
                 HostName = configuration["RabbitMQHost"],
                 Port = int.Parse(configuration["RabbitMQPort"])
             };
             connection = factory.CreateConnection();
             channel = connection.CreateModel();
             channel.ExchangeDeclare("trigger", ExchangeType.Fanout);
             queueName = channel.QueueDeclare().QueueName;
             channel.QueueBind(queueName, "trigger", string.Empty);
             logger.LogInformation("--> Listening on the Message Bus...");
             connection.ConnectionShutdown += RabbitMQConnectionShutdown;
        }

        private void RabbitMQConnectionShutdown(object? sender, ShutdownEventArgs e)
        {
            logger.LogInformation("--> RabbitMQ Connection Shutdown");
        }

        public override void Dispose()
        {
            logger.LogInformation($"--> MessageBus Disposed");
            if (channel!.IsOpen)
            {
                channel.Close();
                connection!.Close();
            }
            base.Dispose();
        }
    }
}