using System.Text;
using Duncan.Model;
using Duncan.Repositories;
using Duncan.Services;
using Duncan.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Any;
using Shard.Shared.Core;
using Swashbuckle.AspNetCore.Annotations;

namespace Duncan.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BuildingsController : ControllerBase
    {
        private readonly UsersRepo _usersRepo;
        private readonly UnitsRepo _unitsRepo;
        private readonly BuildingsService _buildingsService;
        private readonly MapGeneratorWrapper _map;
        private readonly IClock _clock;

        public BuildingsController(UnitsRepo unitsRepo, UsersRepo usersRepo, IClock clock,BuildingsService buildingsService, MapGeneratorWrapper map)
        {
            _unitsRepo = unitsRepo;
            _usersRepo = usersRepo;
            _clock = clock;
            _buildingsService = buildingsService;
            _map = map;
        }

        [SwaggerOperation(Summary = "Create a building at a location")]
        [HttpPost("/users/{userId}/buildings")]
        public ActionResult<Building> BuildMine(string userId, [FromBody] BuildingBody building)
        {

            User? user = _usersRepo.GetUserWithUnitsByUserId(userId);

            if (user == null)
                return NotFound();

            Unit? unitFound = _unitsRepo.GetUnitWithType("builder", user);

            if (unitFound == null)
                return NotFound("Not Found unit");

            if (building == null)
                return BadRequest("Building is null");

            if (building.BuilderId == null)
                return BadRequest("Builder'id is null");

            if(building.Type != "mine" && building.Type != "starport")
                return BadRequest();

            if (unitFound.Id != building.BuilderId)
                return BadRequest("BuilderId is different than the unitfoundId");

            if (unitFound.DestinationPlanet != unitFound.Planet)
                return BadRequest("Builder is not over a planet");

            if (building.Type == "mine" && !_buildingsService.ValidateResourceCategory(building.ResourceCategory))
                return BadRequest();

            var buildingCreated = _buildingsService.CreateBuilding(building, unitFound);

            _buildingsService.RunTasksOnBuilding(buildingCreated, user);

            user?.Buildings?.Add(buildingCreated);

            return Created("", buildingCreated);
        }

        [SwaggerOperation(Summary = "Create a building at a location")]
        [HttpGet("/users/{userId}/buildings")]

        public ActionResult<List<Building>> GetBuildings(string userId)
        {

            User? user = _usersRepo.GetUserWithUnitsByUserId(userId);

            if (user == null)
                return NotFound();

            return Ok(user.Buildings);
        }
        [SwaggerOperation(Summary = "Create a building at a location")]
        [HttpGet("/users/{userId}/buildings/{buildingId}")]
        public async Task<ActionResult<Building>> GetSingleBuilding(string userId, string buildingId)
        {
            User? user = _usersRepo.GetUserWithUnitsByUserId(userId);

            if (user == null)
                return NotFound();

            var building = user?.Buildings?.FirstOrDefault(b => b.Id == buildingId);

            if (building == null)
                return NotFound();

            if (building.EstimatedBuildTime.HasValue)
            {
                var timeOfArrival = ((building.EstimatedBuildTime.Value - _clock.Now).TotalSeconds);

                if (timeOfArrival <= 2)
                {
                    try
                    {
                        await building.Task;
                    }
                    catch
                    {
                        return NotFound();
                    }
                }
                else
                {
                    building.IsBuilt = false;
                }
            }

            return Ok(building);
        }
        [HttpPost("/users/{userId}/buildings/{starportId}/queue")]
        public async Task<ActionResult<AnyType>> Queuing(string userId, string starportId, [FromBody] QueueBody? queueRequest)
        {
            User? user = _usersRepo.GetUserWithUnitsByUserId(userId);

            if (user == null)
                return NotFound();

            var building = user?.Buildings?.FirstOrDefault(b => b.Id == starportId);

            if (building == null)
                return NotFound();

            var system = _map.Map.Systems.First().Name;
            var planet = _map.Map.Systems.First().Planets.First().Name;

            var unitFound = _unitsRepo.CreateUnitWithType(queueRequest.Type, system, planet);

            switch (queueRequest.Type)
            {
                case "scout" when (user?.ResourcesQuantity?["iron"] < 5 || user?.ResourcesQuantity?["carbon"] < 5):
                    return BadRequest("Not enough resources");

                case "scout" when (building.Type == "mine" || building.IsBuilt == false):
                    return BadRequest();

                case "scout":
                    user.ResourcesQuantity["iron"] -= 5;
                    user.ResourcesQuantity["carbon"] -= 5;
                    break;

                case "builder":
                    user.ResourcesQuantity["iron"] -= 10;
                    user.ResourcesQuantity["carbon"] -= 5;
                    break;

                case "cargo":
                    user.ResourcesQuantity["carbon"] -= 10;
                    user.ResourcesQuantity["iron"] -= 10;
                    user.ResourcesQuantity["gold"] -= 5;
                    break;

                default:
                    return Ok(building);
            }

            return Ok(unitFound);
        }
    }
}