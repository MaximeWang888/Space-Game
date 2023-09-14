using System;
using Microsoft.AspNetCore.Mvc;
using Shard.Shared.Core;
using Swashbuckle.AspNetCore.Annotations;

namespace Duncan.Controllers
{
    public class SystemsController : ControllerBase
    {
        [SwaggerOperation(Summary = "Get all systems with their planets")]
        [HttpGet("/systems")]
        public IReadOnlyList<SystemSpecification> GetSystems()
        {
            MapGeneratorOptions mapOptions = new MapGeneratorOptions();
            mapOptions.Seed = "MohaMax";
            MapGenerator map = new MapGenerator(mapOptions);
            SectorSpecification systems = map.Generate();

            return systems.Systems;
        }

        [SwaggerOperation(Summary = "Get a single system, and all its planet")]
        [HttpGet("/systems/{systemName}")]

        public SystemSpecification GetOneSystem(string systemName)
        {
            IReadOnlyList<SystemSpecification> systems = GetSystems();
            IReadOnlyList<SystemSpecification> filteredSystem = systems.Where(system => system.Name == systemName).ToList();

            return filteredSystem[0];
        }

        [SwaggerOperation(Summary = "Get all planets of a single system")]
        [HttpGet("/systems/{systemName}/planets")]

        public IReadOnlyList<PlanetSpecification> GetPlanetsOfOneSystem(string systemName)
        {
            IReadOnlyList<SystemSpecification> systems = GetSystems();
            IReadOnlyList<SystemSpecification> filteredSystem = systems.Where(system => system.Name == systemName).ToArray();

            return filteredSystem[0].Planets;
        }

        [SwaggerOperation(Summary = "Get a single planet of a system")]
        [HttpGet("/systems/{systemName}/planets/{planetName}")]

        public PlanetSpecification GetOnePlanet(string systemName,string planetName)
        {
            IReadOnlyList<SystemSpecification> systems = GetSystems();
            IReadOnlyList<SystemSpecification> filteredSystem = systems.Where(system => system.Name == systemName).ToList();
            IReadOnlyList<PlanetSpecification> planets = GetPlanetsOfOneSystem(systemName);
            PlanetSpecification response = planets.Where(planet => planet.Name == planetName).First();

            return response;
        }
    }
}