using Duncan.Model;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Duncan.Controllers
{
    public class UnitsController : ControllerBase
    {

        private readonly List<UserUnit> users = new();

        [SwaggerOperation(Summary = "Get unit of a specific user")]
        [HttpGet("users/{Id}/units")]
        public ActionResult<List<Unit>> GetAllUnit(string Id)
        {
            UserUnit user = users.FirstOrDefault(u => u.Id == Id);

            if (user == null)
                return NotFound();

            return user.Units;

        }

        [SwaggerOperation(Summary = "Get unit of a specific user")]
        [HttpGet("users/{Id}/units/{unitId}")]
        public ActionResult<Unit> GetScoutStatusById(string Id, string unitId)
        {
            UserUnit user = users.FirstOrDefault(u => u.Id == Id);

            Unit scoutUnit = user.Units.FirstOrDefault(u => u.Id == unitId);

            if (scoutUnit == null) return NotFound();

            return scoutUnit;
        }

        [SwaggerOperation(Summary = "Get unit of a specific user")]
        [HttpGet("users/{Id}/units/{unitId}/loaction")]
        public ActionResult<Unit> GetScoutLocation(string type, string unitId)
        {
            return Ok();
        }

        [SwaggerOperation(Summary = "Put a specific user")]
        [HttpPut("users/{Id}/units/{UnitId}")]
        public ActionResult<Unit> PutUnitById(string id, [FromBody] Unit unit)
        {
            if (unit.Id.Length < 2)
                return BadRequest("Invalid user ID");

            if (unit == null)
                return BadRequest("Request body is required");

            if (unit.Id != id)
                return BadRequest("Inconsistent unit ID");

            var user = users.Find(u => u.Id == id);

            return user.Units[0];
        }
    }
}