using Microsoft.AspNetCore.Mvc;
using Shard.Shared.Core;
using Swashbuckle.AspNetCore.Annotations;

namespace Duncan.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SystemsController : ControllerBase
    {

        private readonly MapGeneratorWrapper map;

        public SystemsController(MapGeneratorWrapper mapGenerator)
        {
            this.map = mapGenerator;
        }

        [SwaggerOperation(Summary = "Get all systems")]
        [HttpGet("")]
        public IReadOnlyList<SystemSpecification> GetAllSystems()
        {
            return map.Map.Systems;
        }

        [SwaggerOperation(Summary = "Get a specific system by its name")]
        [HttpGet("{systemName}")]
        public SystemSpecification GetSystem(string systemName)
        {
            IReadOnlyList<SystemSpecification> systems = GetAllSystems();
            IEnumerable<SystemSpecification> filteredSystem = systems.Where(system => system.Name == systemName);

            return filteredSystem.First();
        }

        [SwaggerOperation(Summary = "Get all planets of a specific system")]
        [HttpGet("{systemName}/planets")]
        public IReadOnlyList<PlanetSpecification> GetAllPlanetsOfSystem(string systemName)
        {
            SystemSpecification systemSelected = GetSystem(systemName);

            return systemSelected.Planets;
        }

        [SwaggerOperation(Summary = "Get a specific planet of a system")]
        [HttpGet("{systemName}/planets/{planetName}")]

        public PlanetSpecification GetPlanet(string systemName, string planetName)
        {
            IReadOnlyList<PlanetSpecification> planetsSelected = GetAllPlanetsOfSystem(systemName);
            IEnumerable<PlanetSpecification> filteredPlanets = planetsSelected.Where(planet => planet.Name == planetName);

            return filteredPlanets.First();
        }
    }
}