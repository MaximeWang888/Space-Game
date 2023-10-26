using Duncan.Model;
using Microsoft.AspNetCore.Mvc;
using Shard.Shared.Core;
using Swashbuckle.AspNetCore.Annotations;

namespace Duncan.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {

        private readonly UserDB? userDB;
        private readonly MapGeneratorWrapper map;

        public UsersController(UserDB userDB, MapGeneratorWrapper mapGenerator)
        {
            this.userDB = userDB;
            this.map = mapGenerator;
        }

        [SwaggerOperation(Summary = "Put a specific user")]
        [HttpPut("{id}")]
        public ActionResult<User> PutUserById(string id, [FromBody] User user)
        {
            if (user == null)
                return BadRequest("Request body is required");

            if (user.Id?.Length < 2)
                return BadRequest("Invalid user ID");

            if (user.Id != id)
                return BadRequest("Inconsistent user ID");

            Unit unit = new Unit();
            unit.Planet = map.Map.Systems.First().Planets.First().Name;
            unit.System = map.Map.Systems.First().Name;
            unit.Type = "scout";
            UserWithUnits userWithUnits = new UserWithUnits();
            userWithUnits.Id = user.Id;
            userWithUnits.Pseudo = user.Pseudo;
            userWithUnits.Units?.Add(unit);

            userDB?.users.Add(userWithUnits);

            return user;
        }

        [SwaggerOperation(Summary = "Get a specific user")]
        [HttpGet("{id}")]
        public ActionResult<User> GetUserById(string id)
        {
            if (userDB == null)
                return NotFound("Not Found User DB");

            UserWithUnits? user = userDB.users.FirstOrDefault(u => u.Id == id);

            if (user == null)
                return NotFound();

            User userF = new User();
            userF.Id = user.Id;
            userF.Pseudo = user.Pseudo;

            return userF;
        }

    }
}