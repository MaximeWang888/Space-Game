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

        public UsersController(List<User> users)
        {
            this.users = users;
        }

        [SwaggerOperation(Summary = "Put a specific user")]
        [HttpPut("{id}")]
        public ActionResult<User> PutUserById(string id, [FromBody] User user)
        {
            if (user.Id.Length < 2)
                return BadRequest("Invalid user ID");

            if (user == null)
                return BadRequest("Request body is required");

            if (user.Id != id)
                return BadRequest("Inconsistent user ID");

            Unit unit = new Unit("1");

            user.unit = unit;

            users.Add(user);

            return user;
        }

        [SwaggerOperation(Summary = "Get a specific user")]
        [HttpGet("{id}")]
        public ActionResult<User> GetUserById(string id)
        {
            var user = users.FirstOrDefault(u => u.Id == id);

            if (user == null)
                return NotFound();

            return user;
        }

    }
}