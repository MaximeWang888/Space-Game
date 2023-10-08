using Duncan.Model;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Duncan.Controllers
{
    public class UnitsController : ControllerBase
    {
        private readonly UserDB? userDB;

        public UnitsController(UserDB userDB)
        {
            this.userDB = userDB;
        }

        [SwaggerOperation(Summary = "Get unit of a specific user")]
        [HttpGet("users/{userId}/Units")]
        public List<Unit> GetAllUnit(string userId)
        {
            UserWithUnits user = userDB.users.FirstOrDefault(u => u.Id == userId);

            return user.Units;
        }

        [SwaggerOperation(Summary = "Put a specific user")]
        [HttpPut("users/{userId}/units/{unitId}")]
        public ActionResult<List<Unit>> PutUnitById(string userId, string unitId, [FromBody] Unit unit)
        {
            if (unit == null)
                return BadRequest("Request body is required");

            if (unit.Id.Length < 2)
                return BadRequest("Invalid user ID");

            if (unit.Id != userId)
                return BadRequest("Inconsistent unit ID");

            UserWithUnits userWithUnits = userDB.users.FirstOrDefault(u => u.Id == userId);

            return userWithUnits.Units;
        }

        [SwaggerOperation(Summary = "Get a specific user")]
        [HttpGet("users/{userId}/units/{unitId}/location")]
        public ActionResult<string> GetUnitLocation(string userId, string unitId, Unit unit)
        {
            return unit.Planet;
        }
    }
}