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
        private readonly MapGeneratorWrapper _map;
        private readonly IClock _clock;

        public BuildingsController(UnitsRepo unitsRepo, UsersRepo usersRepo, IClock clock, BuildingsService buildingsService)
        {
            _unitsRepo = unitsRepo;
            _usersRepo = usersRepo;
            _clock = clock;
            _buildingsService = buildingsService;
        }

        [SwaggerOperation(Summary = "Creates a building at a location.")]
        [HttpPost("/users/{userId}/Buildings")]
        public ActionResult<Building> CreateBuildingAtLocation([FromRoute] string userId, [FromBody] BuildingBody building)
        {
            User? user = _usersRepo.GetUserWithUnitsByUserId(userId);

            if (user == null)
                return NotFound("No user with such id=" + userId);

            Unit? builderUnit = _unitsRepo.GetUnitWithType("builder", user);

            if (builderUnit == null)
                return NotFound("Builder unit not found");

            ActionResult<Building> validationError = _buildingsService.ValidateBuilding(building, builderUnit);
            if (validationError != null)
                return validationError;

            var buildingCreated = _buildingsService.CreateBuilding(building, builderUnit);
            _buildingsService.RunTasksOnBuilding(buildingCreated, user);
            user?.Buildings?.Add(buildingCreated);

            return Created("", buildingCreated);
        }

        [SwaggerOperation(Summary = "Return all buildings of a user.")]
        [HttpGet("/users/{userId}/Buildings")]
        public ActionResult<List<Building>> GetAllBuildingsForUser([FromRoute] string userId)
        {

            User? user = _usersRepo.GetUserWithUnitsByUserId(userId);

            if (user == null)
                return NotFound("No user with such id=" + userId);

            return user.Buildings;
        }

        [SwaggerOperation(Summary = "Return information about one single building of a user.")]
        [HttpGet("/users/{userId}/Buildings/{buildingId}")]
        public async Task<ActionResult<Building>> GetInformationAboutBuilding([FromRoute] string userId, [FromRoute] string buildingId)
        {
            User? user = _usersRepo.GetUserWithUnitsByUserId(userId);

            if (user == null)
                return NotFound("No user with such id=" + userId);

            var building = user?.Buildings?.FirstOrDefault(b => b.Id == buildingId);

            if (building == null)
                return NotFound("No building with such id=" + buildingId);

            if (building.EstimatedBuildTime.HasValue && _buildingsService.IsBuildingUnderConstruction(building))
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

            return building;
        }

        [SwaggerOperation(Summary = "Add a unit to the build queue of the starport. Currently immediatly returns the unit.")]
        [HttpPost("/users/{userId}/Buildings/{starportId}/queue")]
        public ActionResult<Unit> AddUnitToBuildQueueAtStarport([FromRoute] string userId, [FromRoute] string starportId, [FromBody] UnitBlueprint? queueRequest)
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

            bool hasEnoughResources = _buildingsService.DeductResources(user, building, queueRequest.Type);

            if (!hasEnoughResources)
                return BadRequest("Not enough resources");

            return unitFound;
        }
    }
}