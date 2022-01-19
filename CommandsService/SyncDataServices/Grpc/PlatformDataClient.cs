using AutoMapper;
using CommandsService.Models;
using Grpc.Net.Client;

namespace CommandsService.SyncDataServices.Grpc
{
    public class PlatformDataClient : IPlatformDataClient
    {
        private readonly IConfiguration configuration;
        private readonly IMapper mapper;
        private readonly ILogger<PlatformDataClient> logger;

        public PlatformDataClient(IConfiguration configuration, IMapper mapper,
            ILogger<PlatformDataClient> logger)
        {
            this.configuration = configuration;
            this.mapper = mapper;
            this.logger = logger;
        }

        public IEnumerable<Platform> ReturnAllPlatforms()
        {
            logger.LogInformation($"--> Calling GRPC service {configuration["GrpcPlatform"]}");
            using var channel = GrpcChannel.ForAddress(configuration["GrpcPlatform"]);
            var client = new GrpcPlatform.GrpcPlatformClient(channel);
            var request = new GetAllRequest();
            try
            {
                 var response = client.GetAllPlatforms(request);
                 return mapper.Map<IEnumerable<Platform>>(response.Platforms);
            }
            catch (Exception ex)
            {
                logger.LogError($"--> Could not call GRPC Server {ex.Message}");
                return default!;
            }
        }
    }
}