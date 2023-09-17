using Microsoft.AspNetCore.Mvc;
using Shard.Shared.Core;
using Swashbuckle.AspNetCore.Annotations;

namespace Duncan.Controllers
{
    public class SystemsController : ControllerBase
    {
        [SwaggerOperation(Summary = "Get all systems")]
        [HttpGet("/systems")]
        public IReadOnlyList<SystemSpecification> GetSystems()
        {
            MapGeneratorOptions mapOptions = new MapGeneratorOptions() { Seed = "MohaMax" }; 
            MapGenerator mapGenerator = new MapGenerator(mapOptions);

            return mapGenerator.Generate().Systems;
        }

        [SwaggerOperation(Summary = "Get a specific system by its name")]
        [HttpGet("/systems/{systemName}")]
        public SystemSpecification GetOneSystem(string systemName)
        {
            IReadOnlyList<SystemSpecification> systems = GetSystems();
            IEnumerable<SystemSpecification> filteredSystem = systems.Where(system => system.Name == systemName);
            
            return filteredSystem.First();
        }

        [SwaggerOperation(Summary = "Get all planets of a single system")]
        [HttpGet("/systems/{systemName}/planets")]
        public IReadOnlyList<PlanetSpecification> GetPlanetsOfOneSystem(string systemName)
        {
            IReadOnlyList<SystemSpecification> systems = GetSystems();
            SystemSpecification filteredSystem = systems.Where(system => system.Name == systemName).First();

            return filteredSystem.Planets;
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