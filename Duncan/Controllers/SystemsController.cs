using Duncan.Model;
using Duncan.Repositories;
using Duncan.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Duncan.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SystemsController : ControllerBase
    {

        private readonly MapGeneratorWrapper _map;
        private readonly SystemsService _systemsService;
        private readonly SystemsRepo _systemsRepo;
        private readonly PlanetsRepo _planetsRepo;

        public SystemsController(MapGeneratorWrapper mapGenerator, SystemsService systemsService, SystemsRepo systemsRepo, PlanetsRepo planetsRepo)
        {
            _map = mapGenerator;    
            _systemsService = systemsService; 
            _systemsRepo = systemsRepo;
            _planetsRepo = planetsRepo;
        }

        [SwaggerOperation(Summary = "Fetches all systems, and their planet")]
        [HttpGet("")]
        public IList<CustomSystem> GetAllSystems()
        {
            IList<CustomSystem> CustomSystems = new List<CustomSystem>();

            _systemsService.SystemsTransformation(CustomSystems, _map);

            return CustomSystems;
        }

        [SwaggerOperation(Summary = "Fetches a single system, and all its planets")]
        [HttpGet("{systemName}")]
        public CustomSystem GetSystem([FromRoute] string systemName)
        {
            IList<CustomSystem> systems = GetAllSystems();

            return _systemsRepo.GetSystemByName(systemName, systems);
        }

        [SwaggerOperation(Summary = "Fetches all planets of a single system")]
        [HttpGet("{systemName}/planets")]
        public IList<Planet> GetAllPlanetsOfSystem([FromRoute] string systemName)
        {
            CustomSystem systemSelected = GetSystem(systemName);

            return systemSelected.Planets;
        }

        [SwaggerOperation(Summary = "Fetches a single planet of a system")]
        [HttpGet("{systemName}/planets/{planetName}")]
        public Planet? GetPlanet([FromRoute] string systemName, [FromRoute] string planetName)
        {
            IList<Planet> planetsSelected = GetAllPlanetsOfSystem(systemName);

            return _planetsRepo.GetPlanetByName(planetName, planetsSelected);
        }
    }
}