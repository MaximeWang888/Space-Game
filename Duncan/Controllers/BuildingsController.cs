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
        private readonly IClock _clock;

        public BuildingsController(UnitsRepo unitsRepo, UsersRepo usersRepo, IClock clock,BuildingsService buildingsService)
        {
            _unitsRepo = unitsRepo;
            _usersRepo = usersRepo;
            _clock = clock;
            _buildingsService = buildingsService;
        }

        [SwaggerOperation(Summary = "Creates a building at a location.")]
        [HttpPost("/users/{userId}/Buildings")]
        public ActionResult<Building> CreateBuilding(string userId, [FromBody] BuildingBody building)
        {

            User? user = _usersRepo.GetUserWithUnitsByUserId(userId);

            if (user == null)
                return NotFound("No user with such id=" + userId);

            Unit? unitFound = _unitsRepo.GetUnitWithType("builder", user);

            if (unitFound == null)
                return NotFound("Not Found unit");

            if (building == null)
                return BadRequest("Building is missing");

            if (building.BuilderId == null)
                return BadRequest("Builder id is missing");

            if(building.Type != "mine" && building.Type != "starport")
                return BadRequest();

            if (unitFound.Id != building.BuilderId)
                return BadRequest("BuilderId is different than the unitfoundId");

            if (unitFound.DestinationPlanet != unitFound.Planet)
                return BadRequest("Unit is not over the planet");

            if (building.Type == "mine" && !_buildingsService.ValidateResourceCategory(building.ResourceCategory))
                return BadRequest("Builder specify a type other than mine");

            var buildingCreated = _buildingsService.CreateBuilding(building, unitFound);

            _buildingsService.RunTasksOnBuilding(buildingCreated, user);

            user?.Buildings?.Add(buildingCreated);

            return Created("", buildingCreated);
        }

        [SwaggerOperation(Summary = "Return all buildings of a user.")]
        [HttpGet("/users/{userId}/Buildings")]
        public ActionResult<List<Building>> GetAllBuildings(string userId)
        {

            User? user = _usersRepo.GetUserWithUnitsByUserId(userId);

            if (user == null)
                return NotFound("No user with such id=" + userId);

            return Ok(user.Buildings);
        }

        [SwaggerOperation(Summary = "Return information about one single building of a user.")]
        [HttpGet("/users/{userId}/Buildings/{buildingId}")]
        public async Task<ActionResult<Building>> GetSingleBuilding(string userId, string buildingId)
        {
            User? user = _usersRepo.GetUserWithUnitsByUserId(userId);

            if (user == null)
                return NotFound("No user with such id=" + userId);

            var building = user?.Buildings?.FirstOrDefault(b => b.Id == buildingId);

            if (building == null)
                return NotFound("No building with such id=" + buildingId);

            if (building.EstimatedBuildTime.HasValue && IsBuildingUnderConstruction(building))
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

            return Ok(building);
        }

        [SwaggerOperation(Summary = "Add a unit to the build queue of the starport. Currently immediatly returns the unit.")]
        [HttpPost("/users/{userId}/Buildings/{starportId}/queue")]
        public async Task<ActionResult<AnyType>> AddUnitToBuildQueue(string userId, string starportId, [FromBody] QueueBody? queueRequest)
        {
            User? user = _usersRepo.GetUserWithUnitsByUserId(userId);

            if (user == null)
                return NotFound("No user with such id=" + userId);

            var building = user?.Buildings?.FirstOrDefault(b => b.Id == starportId);

            if (building == null)
                return NotFound();

            var system = building.System;
            var planet = building.Planet;

            var unitFound = _unitsRepo.CreateUnitWithType(queueRequest.Type, system, planet);

            user.Units.Add(unitFound);

            bool hasEnoughResources = DeductResources(user, building, queueRequest.Type);

            if (!hasEnoughResources)
                return BadRequest("Not enough resources");

            return Ok(unitFound);
        }

        private bool IsBuildingUnderConstruction(Building building)
        {
            var timeOfArrival = (building.EstimatedBuildTime.Value - _clock.Now).TotalSeconds;

            if (timeOfArrival <= 2)
            {
                return true;
            }
            else
            {
                building.IsBuilt = false;
                return false;
            }
        }
        private bool DeductResources(User user, Building building, string unitType)
        {
            if (IsInvalidUnitCase(unitType, building, user))
            {
                return false;
            }

            switch (unitType)
            {
                case "scout":
                    user.ResourcesQuantity["iron"] -= 5;
                    user.ResourcesQuantity["carbon"] -= 5;
                    break;

                case "builder":
                    user.ResourcesQuantity["iron"] -= 10;
                    user.ResourcesQuantity["carbon"] -= 5;
                    break;

                case "cargo":
                    user.ResourcesQuantity["iron"] -= 10;
                    user.ResourcesQuantity["carbon"] -= 10;
                    user.ResourcesQuantity["gold"] -= 5;
                    break;

                default:
                    return true; 
            }

            return true;
        }

        private bool IsInvalidUnitCase(string unitType, Building building, User user)
        {
            return unitType switch
            {
                "scout" when (building.Type == "mine" || building.IsBuilt == false) => true,
                "scout" when (user.ResourcesQuantity?["iron"] < 5 || user.ResourcesQuantity?["carbon"] < 5) => true,
                "builder" when (user.ResourcesQuantity?["iron"] < 10 || user.ResourcesQuantity?["carbon"] < 5) => true,
                "cargo" when (user.ResourcesQuantity?["iron"] < 10 || user.ResourcesQuantity?["carbon"] < 10 || user.ResourcesQuantity?["gold"] < 5) => true,
                _ => false
            };
        }
    }
}