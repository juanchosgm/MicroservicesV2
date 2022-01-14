using AutoMapper;
using CommandsService.Data;
using CommandsService.Dtos;
using CommandsService.Models;
using Microsoft.AspNetCore.Mvc;

namespace CommandsService.Controllers
{
    [Route("api/c/platforms/{platformId}/[controller]")]
    [ApiController]
    public class CommandsController : ControllerBase
    {
        private readonly ICommandRepo commandRepo;
        private readonly IMapper mapper;
        private readonly ILogger<CommandsController> logger;

        public CommandsController(ICommandRepo commandRepo, IMapper mapper, ILogger<CommandsController> logger)
        {
            this.commandRepo = commandRepo;
            this.mapper = mapper;
            this.logger = logger;
        }

        [HttpGet]
        public ActionResult<IEnumerable<CommandReadDto>> GetCommandsForPlatform(int platformId)
        {
            logger.LogInformation($"--> Hit GetCommandsForPlatform: {platformId}");
            if (!commandRepo.PlatformExits(platformId))
            {
                return NotFound();
            }
            var commands = commandRepo.GetCommandsForPlatform(platformId);
            return Ok(mapper.Map<IEnumerable<CommandReadDto>>(commands));
        }

        [HttpGet("{commandId}", Name = "GetCommandForPlatform")]
        public ActionResult<CommandReadDto> GetCommandForPlatform(int platformId, int commandId)
        {
            logger.LogInformation($"--> Hit GetCommandForPlatform: {platformId} / {commandId}");
            if (!commandRepo.PlatformExits(platformId))
            {
                return NotFound();
            }
            var command = commandRepo.GetCommand(platformId, commandId);
            if (command == null)
            {
                return NotFound();
            }
            return Ok(mapper.Map<CommandReadDto>(command));
        }

        [HttpPost]
        public ActionResult<CommandReadDto> CreateCommandForPlatform(int platformId, CommandCreateDto commandCreate)
        {
            logger.LogInformation($"--> Hit CreateCommandForPlatform: {platformId}");
            if (!commandRepo.PlatformExits(platformId))
            {
                return NotFound();
            }
            var command = mapper.Map<Command>(commandCreate);
            commandRepo.CreateCommand(platformId, command);
            commandRepo.SaveChanges();
            var commandRead = mapper.Map<CommandReadDto>(command);
            return CreatedAtRoute(nameof(GetCommandForPlatform), new
            {
                platformId,
                commandId = commandRead.Id
            }, commandRead);
        }
    }
}