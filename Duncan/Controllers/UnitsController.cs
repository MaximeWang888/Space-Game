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
        public List<Unit> GetAllUnit(string userId)
        {
            UserWithUnits user = userDB.users.FirstOrDefault(u => u.Id == userId);

            return user.Units;
        }

        [SwaggerOperation(Summary = "Move Unit By Id")]
        [HttpPut("users/{userId}/Units/{unitId}")]
        public ActionResult<Unit> MoveUnitById(string userId, string unitId, [FromBody] Unit unit)
        {
            UserWithUnits userWithUnits = userDB.users.FirstOrDefault(u => u.Id == userId);

            Unit unitFound = userWithUnits.Units.FirstOrDefault(u => u.Id == unitId);
            unitFound.Planet = unit.Planet;
            unitFound.System = unit.System;

            return userWithUnits.Units.First();
        }

        [SwaggerOperation(Summary = "Return information about one single unit of a user")]
        [HttpGet("users/{userId}/Units/{unitId}")]
        public ActionResult<Unit> GetUnitInformation(string userId, string unitId)
        {
            UserWithUnits userWithUnits = userDB.users.FirstOrDefault(u => u.Id == userId);

            if (userWithUnits == null)
                return NotFound();

            Unit unitFound = userWithUnits.Units.FirstOrDefault(u => u.Id == unitId);

            if (unitFound == null)
                return NotFound();

            return unitFound;
        }

        [SwaggerOperation(Summary = "Get a specific user")]
        [HttpGet("users/{userId}/Units/{unitId}/location")]
        public ActionResult<UnitInformation> GetUnitLocation(string userId, string unitId)
        {
            UserWithUnits userWithUnits = userDB.users.FirstOrDefault(u => u.Id == userId);

            Unit unitFound = userWithUnits.Units.FirstOrDefault(u => u.Id == unitId);

            UnitInformation unitInformation = new UnitInformation();
            unitInformation.System = unitFound.System;
            unitInformation.Planet = unitFound.Planet;

            SystemSpecification system = map.Map.Systems.FirstOrDefault(s => s.Name == unitFound.System);
            if (system == null)
                return NotFound();
            PlanetSpecification planet = system.Planets.FirstOrDefault(p => p.Name == unitFound.Planet);


            unitInformation.ResourcesQuantity = planet.ResourceQuantity.ToDictionary(r => r.Key.ToString().ToLower(), r => r.Value);

            return unitInformation;
        }
    }
}