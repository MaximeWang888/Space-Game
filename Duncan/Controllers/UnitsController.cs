using Duncan.Model;
using Duncan.Repositories;
using Duncan.Services;
using Duncan.Utils;
using Microsoft.AspNetCore.Mvc;
using Shard.Shared.Core;
using Swashbuckle.AspNetCore.Annotations;

namespace Duncan.Controllers
{
    public class UnitsController : ControllerBase
    {
        private readonly MapGeneratorWrapper _map;
        private readonly UsersRepo _usersRepo;
        private readonly UnitsRepo _unitsRepo;
        private readonly SystemsRepo _systemsRepo;
        private readonly PlanetsRepo _planetRepo;
        private readonly UnitsService _unitsService;

        public UnitsController(MapGeneratorWrapper mapGenerator, UsersRepo usersRepo, UnitsRepo unitsRepo, UnitsService unitsService, SystemsRepo systemsRepo, PlanetsRepo planetRepo)
        {
            _map = mapGenerator;
            _unitsRepo = unitsRepo;
            _usersRepo = usersRepo;
            _systemsRepo = systemsRepo;
            _planetRepo = planetRepo;
            _unitsService = unitsService;
        }

        [SwaggerOperation(Summary = "Return all units of a user.")]
        [HttpGet("users/{userId}/Units")]
        public ActionResult<List<Unit>> GetAllUnits([FromRoute] string userId)
        {
            User? user = _usersRepo.GetUserWithUnitsByUserId(userId);

            if (user == null) 
                return NotFound("Not Found user");

            List<Unit> units = user.Units ?? new List<Unit>();

            return units;
        }

        [SwaggerOperation(Summary = "Return information about one single unit of a user.")]
        [HttpGet("users/{userId}/Units/{unitId}")]
        public async Task<ActionResult<Unit>> GetUnitInformation([FromRoute] string userId, [FromRoute] string unitId)
        {
            User? user = _usersRepo.GetUserWithUnitsByUserId(userId);

            if (user == null)
                return NotFound("Not Found userWithUnits");

            Unit? unitFound = _unitsRepo.GetUnitByUnitId(unitId, user);

            if (unitFound == null)
                return NotFound("Not Found unitFound");

            await _unitsService.VerifyTimeDifference(unitFound);

            return unitFound;
        }

        [SwaggerOperation(Summary = "Change the status of a unit of a user. Right now, only its position (system and planet) can be changed - which is akin to moving it. " +
            "If the unit does not exist and the authenticated user is administrator, creates the unit")]
        [HttpPut("users/{userId}/Units/{unitId}")]
        public async Task<ActionResult<Unit?>> ChangeStatusOfUnit([FromRoute] string userId, [FromRoute] string unitId, [FromBody] Unit unitBody)
        {
            User? user = _usersRepo.GetUserWithUnitsByUserId(userId);

            if (user == null)
                return NotFound("Not Found userWithUnits");

            Unit? unitFound = _unitsRepo.GetUnitByUnitId(unitId, user);

            bool isAdmin = User.IsInRole("admin");
            bool isFakeRemoteUser = User.IsInRole("shard");

            await _unitsService.HandleExistingUnit(unitFound, unitBody);

            if (unitFound == null)
                return _unitsService.HandleNonExistingUnit(user, isAdmin, isFakeRemoteUser, unitBody);

            return await _unitsService.ProcessUnitChange(user, isAdmin, isFakeRemoteUser, unitBody, unitFound);
        }


        [SwaggerOperation(Summary = "Returned more detailled information about the location a unit of user currently is about.")]
        [HttpGet("users/{userId}/Units/{unitId}/location")]
        public ActionResult<UnitLocation> GetUnitLocation([FromRoute] string userId, [FromRoute] string unitId)
        {
            User? user = _usersRepo.GetUserWithUnitsByUserId(userId);
            if (user == null)
                return NotFound("User not found");

            Unit? unitFound = _unitsRepo.GetUnitByUnitId(unitId, user);
            if (unitFound == null)
                return NotFound("Unit not found");

            UnitLocation unitInformation = new UnitLocation();
            unitInformation.System = unitFound.System;
            unitInformation.Planet = unitFound.Planet;

            SystemSpecification? system = _systemsRepo.GetSystemByName(unitFound.System, _map.Map.Systems);
            if (system == null)
                return NotFound("System not found");

            PlanetSpecification? planet = _planetRepo.GetPlanetByName(unitFound.Planet, system);
            if (planet == null)
                return NotFound("Planet not found");

            if (unitFound.Type == "scout")
                unitInformation.ResourcesQuantity = planet.ResourceQuantity.ToDictionary(r => r.Key.ToString().ToLower(), r => r.Value);

            return unitInformation;
        }

    }
}
