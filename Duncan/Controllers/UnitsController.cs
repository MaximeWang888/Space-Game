using Duncan.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Extensions;
using Shard.Shared.Core;
using Swashbuckle.AspNetCore.Annotations;

namespace Duncan.Controllers
{
    public class UnitsController : ControllerBase
    {
        private readonly UserDB? userDB;
        private readonly MapGeneratorWrapper map;

        public UnitsController(UserDB userDB, MapGeneratorWrapper mapGenerator)
        {
            this.userDB = userDB;
            this.map = mapGenerator;
        }

        [SwaggerOperation(Summary = "Get unit of a specific user")]
        [HttpGet("users/{userId}/Units")]
        public ActionResult<List<Unit>> GetAllUnit(string userId)
        {
            if (userDB == null)
                return NotFound("Not Found User DB");

            UserWithUnits? user = GetUserWithUnitsByUserId(userId);

            if (user == null) return NotFound("Not Found user");

            List<Unit> units = user.Units ?? new List<Unit>();

            return units;
        }

        private UserWithUnits? GetUserWithUnitsByUserId(string userId)
        {
            return userDB.users.FirstOrDefault(u => u.Id == userId);
        }

        [SwaggerOperation(Summary = "Move Unit By Id")]
        [HttpPut("users/{userId}/Units/{unitId}")]
        public ActionResult<Unit> MoveUnitById(string userId, string unitId, [FromBody] Unit unit)
        {
            if (userDB == null)
                return NotFound("Not Found User DB");

            UserWithUnits? userWithUnits = GetUserWithUnitsByUserId(userId);

            if (userWithUnits == null)
                return NotFound("Not Found userWithUnits");

            Unit? unitFound = userWithUnits.Units?.FirstOrDefault(u => u.Id == unitId);

            if (unitFound == null)
                return NotFound("Not Found unitFound");

            unitFound.Planet = unit.Planet;
            unitFound.System = unit.System;

            List<Unit>? units = userWithUnits.Units;
            if (units == null || units.Count == 0)
            {
                return NotFound("User has no units");
            }
            return units.First();
        }

        [SwaggerOperation(Summary = "Return information about one single unit of a user")]
        [HttpGet("users/{userId}/Units/{unitId}")]
        public ActionResult<Unit> GetUnitInformation(string userId, string unitId)
        {
            if (userDB == null)
                return NotFound("Not Found User DB");

            UserWithUnits? userWithUnits = GetUserWithUnitsByUserId(userId);

            if (userWithUnits == null)
                return NotFound("Not Found userWithUnits");

            Unit? unitFound = userWithUnits.Units?.FirstOrDefault(u => u.Id == unitId);

            if (unitFound == null)
                return NotFound("Not Found unitFound");

            return unitFound;
        }

        [SwaggerOperation(Summary = "Get a specific user")]
        [HttpGet("users/{userId}/Units/{unitId}/location")]
        public ActionResult<UnitLocation> GetUnitLocation(string userId, string unitId)
        {
            if (userDB == null)
                return NotFound("Not Found User DB");

            UserWithUnits? userWithUnits = GetUserWithUnitsByUserId(userId);

            if (userWithUnits == null)
                return NotFound("Not Found userWithUnits");

            Unit? unitFound = userWithUnits.Units?.FirstOrDefault(u => u.Id == unitId);

            if (unitFound == null)
                return NotFound("Not Found unitFound");

            UnitLocation unitInformation = new UnitLocation();
            unitInformation.System = unitFound.System;
            unitInformation.Planet = unitFound.Planet;

            SystemSpecification? system = map.Map.Systems.FirstOrDefault(s => s.Name == unitFound.System);

            if (system == null)
                return NotFound("Not Found system");
            
            PlanetSpecification? planet = system.Planets.FirstOrDefault(p => p.Name == unitFound.Planet);

            if (planet == null)
                return NotFound("Not Found planet");

            unitInformation.ResourcesQuantity = planet.ResourceQuantity.ToDictionary(r => r.Key.ToString().ToLower(), r => r.Value);

            return unitInformation;
        }
    }
}