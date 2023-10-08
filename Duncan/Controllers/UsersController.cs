using Duncan.Model;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Duncan.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {

        private readonly List<UserWithUnits> users = new();

        public UsersController(List<UserWithUnits> users)
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

            Unit unit = new Unit();
            UserWithUnits userWithUnits = new UserWithUnits();
            userWithUnits.Id = user.Id;
            userWithUnits.Pseudo = user.Pseudo;
            userWithUnits.Units.Add(unit);

            users.Add(userWithUnits);

            return user;
        }

        [SwaggerOperation(Summary = "Get a specific user")]
        [HttpGet("{id}")]
        public ActionResult<User> GetUserById(string id)
        {
            UserWithUnits user = users.FirstOrDefault(u => u.Id == id);

            if (user == null)
                return NotFound();

            User userF = new User();
            userF.Id = user.Id;
            userF.Pseudo = user.Pseudo;

            return userF;
        }

        public List<UserWithUnits> GetAllUsers()
        {
            return users;
        }
    }
}