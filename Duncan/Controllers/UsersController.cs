using Duncan.Model;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Duncan.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {

        private readonly List<UserUnit> users = new();


        [SwaggerOperation(Summary = "Get a specific user")]
        [HttpGet("{id}")]
        public ActionResult<UserUnit> GetUserById(string id)
        {
            var user = users.FirstOrDefault(u => u.Id == id);

            if (user == null)
                return NotFound();

            return user;
        }

        [SwaggerOperation(Summary = "Put a specific user")]
        [HttpPut("{id}")]
        public ActionResult<UserUnit> PutUserById(string id, [FromBody] User user)
        {
            if (user.Id.Length < 2)
                return BadRequest("Invalid user ID");

            if (user == null)
                return BadRequest("Request body is required");

            if (user.Id != id)
                return BadRequest("Inconsistent user ID");

            users.Add(new UserUnit());

            return users.First();
        }

        private Unit initUnit()
        {
            Unit unit = new Unit();
            unit.Type = "scout";
            unit.System = "system";
            unit.Planet = "planet";

            return unit;
        }

    }
}