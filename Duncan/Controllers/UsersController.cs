using Duncan.Model;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Duncan.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {

        private readonly List<User> users = new();
        private List<Unit> units = new List<Unit> { new Unit { Type = "scout", Id = "1" } };

        public UsersController(List<User> users)
        {
            this.users = users;
        }

        [SwaggerOperation(Summary = "Put a specific user")]
        [HttpPut("{Id}")]
        public ActionResult<User> PutUserById(string id, [FromBody] User user)
        {
            if (user.Id.Length < 2)
                return BadRequest("Invalid user ID");

            if (user == null)
                return BadRequest("Request body is required");

            if (user.Id != id)
                return BadRequest("Inconsistent user ID");

            users.Add(user);
            return user;
        }

        [SwaggerOperation(Summary = "Put a specific user")]
        [HttpPut("{Id}/units/{UnitId}")]
        public ActionResult<Unit> PutUnitById(string id, [FromBody] Unit unit)
        {
            if (unit.Id.Length < 2)
                return BadRequest("Invalid user ID");

            if (unit == null)
                return BadRequest("Request body is required");

            if (unit.Id != id)
                return BadRequest("Inconsistent unit ID");

            units.Add(unit);
            return unit;
        }

        [SwaggerOperation(Summary = "Get a specific user")]
        [HttpGet("{Id}")]
        public ActionResult<User> GetUserById(string id)
        {
            var user = users.FirstOrDefault(u => u.Id == id);
            if (user == null)
                return NotFound();

            return user;
        }

        [SwaggerOperation(Summary = "Get unit of a specific user")]
        [HttpGet("{Id}/units")]
        public List<Unit> GetAllUnit()
        {
            return units;
        }

        [SwaggerOperation(Summary = "Get unit of a specific user")]
        [HttpGet("{Id}/units/{unitId}")]
        public ActionResult<Unit> GetScoutStatusById(string type, string unitId)
        {
            Unit scoutUnit = units.FirstOrDefault(u => u.Type == type && u.Id == unitId);


            return scoutUnit;
        }


    }
}