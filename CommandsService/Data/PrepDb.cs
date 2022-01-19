using CommandsService.Models;
using CommandsService.SyncDataServices.Grpc;

namespace CommandsService.Data
{
    public static class PrepDb
    {
        public static void PrepPopulation(IApplicationBuilder builder)
        {
            using var scope = builder.ApplicationServices.CreateScope();
            var platformDataClient = scope.ServiceProvider.GetService<IPlatformDataClient>();
            var platforms = platformDataClient!.ReturnAllPlatforms();
            var commandRepo = scope.ServiceProvider.GetService<ICommandRepo>();
            SeedData(commandRepo!, platforms);
        }

        private static void SeedData(ICommandRepo repo, IEnumerable<Platform> platforms)
        {
            Console.WriteLine("Seeding new platforms...");
            foreach (var platform in platforms)
            {
                if (!repo.ExternalPlatformExists(platform.ExternalID))
                {
                    repo.CreatePlatform(platform);
                    repo.SaveChanges();
                }
            }
        }
    }
}