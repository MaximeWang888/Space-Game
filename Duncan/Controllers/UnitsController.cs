using Duncan.Helper;
using Duncan.Model;
using Duncan.Repositories;
using Duncan.Services;
using Duncan.Utils;
using Microsoft.AspNetCore.Mvc;
using Shard.Shared.Core;
using Swashbuckle.AspNetCore.Annotations;
using System.Runtime.InteropServices;

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

        public UnitsController(MapGeneratorWrapper mapGenerator, UsersRepo usersRepo, UnitsRepo unitsRepo, UnitsService unitsService, SystemsRepo systemsRepo, PlanetRepo planetRepo)
        {
            _map = mapGenerator;
            _unitsRepo = unitsRepo;
            _usersRepo = usersRepo;
            _systemsRepo = systemsRepo;
            _planetRepo = planetRepo;
            _unitsService = unitsService;
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

        [SwaggerOperation(Summary = "Move Unit By Id")]
        [HttpPut("users/{userId}/units/{unitId}")]
        public async Task<ActionResult<Unit?>> MoveUnitByIdAsync(string userId, string unitId, [FromBody] Unit unit)
        {
            User? user = _usersRepo.GetUserWithUnitsByUserId(userId);
            if (user == null)
                return NotFound("Not Found userWithUnits");

            Unit? unitFound = _unitsRepo.GetUnitByUnitId(unitId, user);

            bool isAdmin = HelperAuth.isAdmin(Request);
            bool isFakeRemoteUser = HelperAuth.isFakeRemoteUser(Request);

            if (unitFound == null && !isAdmin && !isFakeRemoteUser && unit.Type == "cargo") 
            {
                user.Units.Add(unit);
                return unit; 
            }
            else if (unitFound == null && !isAdmin && !isFakeRemoteUser)
                return Unauthorized("Unauthorized");
            else if (isAdmin)
            {
                user.Units.Add(unit);
                unit.DestinationPlanet = unit.Planet;
                unit.DestinationSystem = unit.System;
                unit.Health = unit.Type switch
                {
                    "bomber" => 50,
                    "fighter" => 80,
                    "cruiser" => 400,
                    _ => unit.Health // Default case, keep the existing health if the unit type is not recognized
                };
                return unit;
            }
            else if (isFakeRemoteUser)
            {
                unit.System = "80ad7191-ef3c-14f0-7be8-e875dad4cfa6";
                user.Units.Add(unit);
                return unit;
            }

            unitFound.DestinationPlanet = unit.DestinationPlanet;
            unitFound.DestinationSystem = unit.DestinationSystem;
            unitFound.Task = _unitsService.WaitingUnit(unit, unitFound);

            var building = user.Buildings.FirstOrDefault(b => b.BuilderId == unit.Id);

            if (building != null &&
                ((unit.DestinationSystem == unit.System && unit.DestinationPlanet != unit.Planet) ||
                 (unit.DestinationSystem != unit.System && unit.DestinationPlanet == unit.Planet)))
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
