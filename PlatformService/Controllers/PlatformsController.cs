using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PlatformService.Data;
using PlatformService.Dtos;
using PlatformService.Models;
using PlatformService.SyncDataServices.Http;

namespace PlatformService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlatformsController : ControllerBase
    {
        private readonly IPlatformRepo platformRepo;
        private readonly IMapper mapper;
        private readonly ICommandDataClient commandDataClient;

        public PlatformsController(IPlatformRepo platformRepo,
            IMapper mapper,
            ICommandDataClient commandDataClient)
        {
            this.mapper = mapper;
            this.commandDataClient = commandDataClient;
            this.platformRepo = platformRepo;
        }

        [HttpGet]
        public ActionResult<IEnumerable<PlatformReadDto>> GetPlatforms()
        {
            Console.WriteLine("--> Getting Platforms...");
            var platformItems = platformRepo.GetAllPlatforms();
            return Ok(mapper.Map<IEnumerable<PlatformReadDto>>(platformItems));
        }

        [HttpGet("{id}", Name = "GetPlatformById")]
        public ActionResult<PlatformReadDto> GetPlatformById([FromRoute] int id)
        {
            var platformItem = platformRepo.GetPlatformById(id);
            if (platformItem != null)
            {
                return Ok(mapper.Map<PlatformReadDto>(platformItem));
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<ActionResult> CreatePlatform([FromBody] PlatformCreateDto platformCreate)
        {
            var platformModel = mapper.Map<Platform>(platformCreate);
            platformRepo.CreatePlatform(platformModel);
            platformRepo.SaveChanges();
            var platformRead = mapper.Map<PlatformReadDto>(platformModel);
            try
            {
                await commandDataClient.SendPlatformToCommand(platformRead);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Could not send synchronously: {ex.Message}");
                throw;
            }
            return CreatedAtRoute(nameof(GetPlatformById), new { id = platformRead.Id }, platformRead);
        }
    }
}