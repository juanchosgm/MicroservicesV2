using System;
using System.Collections.Generic;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PlatformService.Data;
using PlatformService.Dtos;
using PlatformService.Models;

namespace PlatformService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlatformsController : ControllerBase
    {
        private readonly IPlatformRepo platformRepo;
        private readonly IMapper mapper;

        public PlatformsController(IPlatformRepo platformRepo, IMapper mapper)
        {
            this.mapper = mapper;
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
        public ActionResult CreatePlatform([FromBody] PlatformCreateDto platformCreate)
        {
            var platformModel = mapper.Map<Platform>(platformCreate);
            platformRepo.CreatePlatform(platformModel);
            platformRepo.SaveChanges();
            var platformRead = mapper.Map<PlatformReadDto>(platformModel);
            return CreatedAtRoute(nameof(GetPlatformById), new { id = platformRead.Id }, platformRead);
        }
    }
}