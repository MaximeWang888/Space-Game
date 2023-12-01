using Duncan.Model;
using Duncan.Repositories;
using Duncan.Services;
using Duncan.Utils;
using Microsoft.AspNetCore.Mvc;
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
            this._unitsRepo = unitsRepo;
            this._usersRepo = usersRepo;
            this._clock = clock;
            this._buildingsService = buildingsService;
        }

        [SwaggerOperation(Summary = "Create a building at a location")]
        [HttpPost("/users/{userId}/buildings")]
        public ActionResult<Building> BuildMine(string userId, [FromBody] BuildingBody building)
        {

            UserWithUnits? userWithUnits = _usersRepo.GetUserWithUnitsByUserId(userId);

            if (userWithUnits == null)
                return NotFound();

            Unit? unitFound = _unitsRepo.GetUnitWithType("builder", userWithUnits);

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

            _buildingsService.RunTasksOnBuilding(buildingCreated, userWithUnits);

            userWithUnits?.Buildings?.Add(buildingCreated);

            return Created("", buildingCreated);
        }

        [SwaggerOperation(Summary = "Create a building at a location")]
        [HttpGet("/users/{userId}/buildings")]

        public ActionResult<List<Building>> GetBuildings(string userId)
        {

            UserWithUnits? userWithUnits = _usersRepo.GetUserWithUnitsByUserId(userId);

            if (userWithUnits == null)
                return NotFound();

            return Ok(userWithUnits.Buildings);
        }
        [SwaggerOperation(Summary = "Create a building at a location")]
        [HttpGet("/users/{userId}/buildings/{buildingId}")]
        public async Task<ActionResult<Building>> GetSingleBuilding(string userId, string buildingId)
        {
            UserWithUnits? userWithUnits = _usersRepo.GetUserWithUnitsByUserId(userId);

            if (userWithUnits == null)
                return NotFound();

            var building = userWithUnits?.Buildings?.FirstOrDefault(b => b.Id == buildingId);

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
    }
}