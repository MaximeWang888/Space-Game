using Duncan.Model;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Duncan.Controllers
{
    public class UnitsController : ControllerBase
    {

        private readonly List<User> users = new();

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

            return user.unit;
        }
    }
}