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
        private readonly PlanetRepo _planetRepo;

        public SystemsController(MapGeneratorWrapper mapGenerator, SystemsService systemsService, SystemsRepo systemsRepo, PlanetRepo planetRepo)
        {
            _map = mapGenerator;    
            _systemsService = systemsService; 
            _systemsRepo = systemsRepo;
            _planetRepo = planetRepo;
        }

        [SwaggerOperation(Summary = "Get all systems")]
        [HttpGet("")]
        public IList<CustomSystem> GetAllSystems()
        {
            IList<CustomSystem> CustomSystems = new List<CustomSystem>();

            _systemsService.SystemsTransformation(CustomSystems, _map);

            return CustomSystems;
        }

        [SwaggerOperation(Summary = "Get a specific system by its name")]
        [HttpGet("{systemName}")]
        public CustomSystem GetSystem(string systemName)
        {
            IList<CustomSystem> systems = GetAllSystems();

            return _systemsRepo.GetSystemByName(systemName, systems);
        }

        [SwaggerOperation(Summary = "Get all planets of a specific system")]
        [HttpGet("{systemName}/planets")]
        public IList<Planet> GetAllPlanetsOfSystem(string systemName)
        {
            CustomSystem systemSelected = GetSystem(systemName);

            return systemSelected.Planets;
        }

        [SwaggerOperation(Summary = "Get a specific planet of a system")]
        [HttpGet("{systemName}/planets/{planetName}")]
        public Planet? GetPlanet(string systemName, string planetName)
        {
            IList<Planet> planetsSelected = GetAllPlanetsOfSystem(systemName);

            return _planetRepo.GetPlanetByName(planetName, planetsSelected);
        }
    }
}