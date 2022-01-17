using System.Text.Json;
using AutoMapper;
using CommandsService.Data;
using CommandsService.Dtos;
using CommandsService.Models;
using CommandsService.Models.Enums;

namespace CommandsService.EventProcessing
{
    public class EventProcessor : IEventProcessor
    {
        private readonly IServiceScopeFactory scopeFactory;
        private readonly IMapper mapper;
        private readonly ILogger<EventProcessor> logger;

        public EventProcessor(IServiceScopeFactory scopeFactory, IMapper mapper,
            ILogger<EventProcessor> logger)
        {
            this.scopeFactory = scopeFactory;
            this.mapper = mapper;
            this.logger = logger;
        }

        public void ProcessEvent(string message)
        {
            var eventType = DetermineEvent(message);
            logger.LogInformation($"--> EventType: {eventType}");
            switch (eventType)
            {
                case EventType.PlatformPublished:
                    AddPlatform(message);
                    break;
                default:
                    break;
            }
        }

        private EventType DetermineEvent(string notificationMessage)
        {
            logger.LogInformation("--> Determining Event");
            var genericEvent = JsonSerializer.Deserialize<GenericEventDto>(notificationMessage);
            return genericEvent!.Event switch
            {
                "Platform_Published" => EventType.PlatformPublished,
                _ => EventType.Undertermined
            };
        }

        private void AddPlatform(string platformPublishedMessage)
        {
            using var scope = scopeFactory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<ICommandRepo>();
            var platformPublished = JsonSerializer.Deserialize<PlatformPublishedDto>(platformPublishedMessage);
            try
            {
                 var platform = mapper.Map<Platform>(platformPublished);
                 if (!repo.ExternalPlatformExists(platform.ExternalID))
                 {
                     repo.CreatePlatform(platform);
                     repo.SaveChanges();
                     logger.LogInformation("--> Platform added!");
                 }
                 else
                 {
                     logger.LogInformation("--> Platform already exists...");
                 }
            }
            catch (Exception ex)
            {
                logger.LogError($"--> Could not add Platform to DB {ex.Message}");
            }
        }
    }
}