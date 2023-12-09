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
        private readonly PlanetRepo _planetRepo;
        private readonly UnitsService _unitsService;
        // private readonly IClock _clock;

        public UnitsController(MapGeneratorWrapper mapGenerator, UsersRepo usersRepo, UnitsRepo unitsRepo, UnitsService unitsService, SystemsRepo systemsRepo, PlanetRepo planetRepo)
        {
            this._map = mapGenerator;
            this._unitsRepo = unitsRepo;
            this._usersRepo = usersRepo;
            this._systemsRepo = systemsRepo;
            this._planetRepo = planetRepo;
            this._unitsService = unitsService;
        }

        [SwaggerOperation(Summary = "Get unit of a specific user")]
        [HttpGet("users/{userId}/Units")]
        public ActionResult<List<Unit>> GetAllUnit(string userId)
        {
            User? user = _usersRepo.GetUserWithUnitsByUserId(userId);

            if (user == null) 
                return NotFound("Not Found user");

            List<Unit> units = user.Units ?? new List<Unit>();

            return units;
        }
        //public async Task DeleteUnitsAfterDelay(Unit unit, User user, int delayInSeconds)
        //{
        //    await _clock.Delay(delayInSeconds);

        //    _unitsRepo.DeleteUnit(unit, user);
        //}

        [SwaggerOperation(Summary = "Move Unit By Id")]
        [HttpPut("users/{userId}/units/{unitId}")]
        public async Task<ActionResult<Unit?>> MoveUnitByIdAsync(string userId, string unitId, [FromBody] Unit unit)
        {
            User? user = _usersRepo.GetUserWithUnitsByUserId(userId);
            if (user == null)
                return NotFound("Not Found userWithUnits");

            Unit? unitFound = _unitsRepo.GetUnitByUnitId(unitId, user);

            bool isAdmin = HelperAuth.isAdmin(Request);

            if (unitFound == null && !isAdmin)
                return Unauthorized("Unauthorized");
            else if (isAdmin)
            {
                unit.DestinationPlanet = unit.Planet;
                unit.DestinationSystem = unit.System;
                switch (unit.Type)
                {
                    case "bomber":
                        unit.Health = 50;
                        break;
                    case "fighter":
                        unit.Health = 80;
                        break;
                    case "cruiser":
                        unit.Health = 400;
                        break;
                }
                user.Units.Add(unit);
                //_unitsService.RunTaskOnUnit(unit, user);
                //_unitsService.RunTaskOnUnit(unit1, unit2);

                // await DeleteUnitsAfterDelay(unit, user, 60000);
                return unit;
            }

            unitFound.DestinationPlanet = unit.DestinationPlanet;
            unitFound.DestinationSystem = unit.DestinationSystem;
            unitFound.task = _unitsService.WaitingUnit(unit, unitFound);

            var building = user.Buildings.FirstOrDefault(b => b.BuilderId == unit.Id);

            if (building != null && unit.DestinationSystem == unit.System && unit.DestinationPlanet != unit.Planet || unit.DestinationSystem != unit.System && unit.DestinationPlanet == unit.Planet)
            {
                building.CancellationSource.Cancel();
                user.Buildings.Remove(user.Buildings.FirstOrDefault(b => b.BuilderId == unit.Id));
            }

            return unitFound;
        }

        [SwaggerOperation(Summary = "Return information about one single unit of a user")]
        [HttpGet("users/{userId}/Units/{unitId}")]
        public async Task<ActionResult<Unit>> GetUnitInformation(string userId, string unitId)
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

        [SwaggerOperation(Summary = "Get unit location by user ID and unit ID")]
        [HttpGet("users/{userId}/Units/{unitId}/location")]
        public ActionResult<UnitLocation> GetUnitLocation(string userId, string unitId)
        {
            User? user = _usersRepo.GetUserWithUnitsByUserId(userId);
            if (user == null)
                return NotFound("Not Found userWithUnits");

            Unit? unitFound = _unitsRepo.GetUnitByUnitId(unitId, user);
            if (unitFound == null)
                return NotFound("Not Found unitFound");

            UnitLocation unitInformation = new UnitLocation();
            unitInformation.System = unitFound.System;
            unitInformation.Planet = unitFound.Planet;

            SystemSpecification? system = _systemsRepo.GetSystemByName(unitFound.System, _map.Map.Systems);
            if (system == null)
                return NotFound("Not Found system");

            PlanetSpecification? planet = _planetRepo.GetPlanetByName(unitFound.Planet, system);
            if (planet == null)
                return NotFound("Not Found planet");

            if (unitFound.Type == "scout")
                unitInformation.ResourcesQuantity = planet.ResourceQuantity.ToDictionary(r => r.Key.ToString().ToLower(), r => r.Value);

            return unitInformation;
        }
    }
}
