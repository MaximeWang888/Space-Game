using Duncan.Model;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Duncan.Controllers
{
    public class UnitsController : ControllerBase
    {
        private readonly UsersController usersController;

        public UnitsController(UsersController usersController)
        {
            this.usersController = usersController;
        }

        [SwaggerOperation(Summary = "Get unit of a specific user")]
        [HttpGet("{userPath}/units")]
        public List<Unit> GetAllUnitByUserId(string userId)
        {
            // var userIdValue = userId.Split('/')[1];

            // List<UserWithUnits> users = usersController.GetAllUsers();
            // UserWithUnits userWithUnits = users.FirstOrDefault(u => u.Id == userIdValue);

            // return userWithUnits.Units;
            return new List<Unit>();
        }

        [SwaggerOperation(Summary = "Put a specific user")]
        [HttpPut("users/{id}/units/{unitId}")]
        public ActionResult<List<Unit>> PutUnitById(string id, [FromBody] Unit unit)
        {
            if (unit == null)
                return BadRequest("Request body is required");

            if (unit.Id.Length < 2)
                return BadRequest("Invalid user ID");

            if (unit.Id != id)
                return BadRequest("Inconsistent unit ID");

            List<UserWithUnits> users = usersController.GetAllUsers();
            UserWithUnits userWithUnits = users.FirstOrDefault(u => u.Id == id);

            // var userWithUnits = users.Find(u => u.Id == id);

            return userWithUnits.Units;
        }

        [SwaggerOperation(Summary = "Get a specific user")]
        [HttpGet("users/{id}/units/{unitId}/location")]
        public ActionResult<string> GetUnitLocation(string userPath, Unit unit)
        {
            return unit.Planet;
        }
    }
}