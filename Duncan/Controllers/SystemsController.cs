using Microsoft.AspNetCore.Mvc;
using Shard.Shared.Core;
using Swashbuckle.AspNetCore.Annotations;

namespace Duncan.Controllers
{
    public class SystemsController : ControllerBase
    {
        [SwaggerOperation(Summary = "Get all systems")]
        [HttpGet("/systems")]
        public IReadOnlyList<SystemSpecification> GetAllSystems()
        {
            MapGeneratorOptions mapOptions = new MapGeneratorOptions() { Seed = "MohaMax" }; 
            MapGenerator mapGenerator = new MapGenerator(mapOptions);

            return mapGenerator.Generate().Systems;
        }

        [SwaggerOperation(Summary = "Get a specific system by its name")]
        [HttpGet("/systems/{systemName}")]
        public SystemSpecification GetSystem(string systemName)
        {
            IReadOnlyList<SystemSpecification> systems = GetAllSystems();
            IEnumerable<SystemSpecification> filteredSystem = systems.Where(system => system.Name == systemName);
            
            return filteredSystem.First();
        }

        [SwaggerOperation(Summary = "Get all planets of a specific system")]
        [HttpGet("/systems/{systemName}/planets")]
        public IReadOnlyList<PlanetSpecification> GetAllPlanetsOfSystem(string systemName)
        {
            SystemSpecification systemSelected = GetSystem(systemName);

            return systemSelected.Planets;
        }

        [SwaggerOperation(Summary = "Get a specific planet of a system")]
        [HttpGet("/systems/{systemName}/planets/{planetName}")]

        public PlanetSpecification GetPlanet(string systemName,string planetName)
        {
            IReadOnlyList<PlanetSpecification> planetsSelected = GetAllPlanetsOfSystem(systemName);
            IEnumerable<PlanetSpecification> filteredPlanets = planetsSelected.Where(planet => planet.Name == planetName);

            return filteredPlanets.First();
        }
    }
}