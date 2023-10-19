using Duncan.Model;
using Duncan.Repositories;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Duncan.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {

        private readonly UserDB? userDB;
        private readonly MapGeneratorWrapper map;
        private readonly UsersRepo _usersRepo;

        public UsersController(UserDB userDB, MapGeneratorWrapper mapGenerator, UsersRepo usersRepo)
        {
            this.userDB = userDB;
            this.map = mapGenerator;
            this._usersRepo = usersRepo;
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

            Unit unit_1 = new Unit
            {
                Planet = map.Map.Systems.First().Planets.First().Name,
                System = map.Map.Systems.First().Name,
                DestinationSystem = map.Map.Systems.First().Name,
                Type = "scout"
            };

            Unit unit_2 = new Unit
            {
                Planet = map.Map.Systems.First().Planets.First().Name,
                System = map.Map.Systems.First().Name,
                DestinationSystem = map.Map.Systems.First().Name,
                Type = "builder",
            };

            UserWithUnits userWithUnits = new UserWithUnits();
            userWithUnits.Id = user.Id;
            userWithUnits.Pseudo = user.Pseudo;

            userWithUnits.Units?.Add(unit_1);
            userWithUnits.Units?.Add(unit_2);

            userDB?.users.Add(userWithUnits);

            return user;
        }

        [SwaggerOperation(Summary = "Get a specific user")]
        [HttpGet("{id}")]
        public ActionResult<User> GetUserById(string id)
        {
            UserWithUnits? userUnit = _usersRepo.GetUserWithUnitsByUserId(id);
            if (userUnit == null)
                return NotFound();

            User user = new User();
            user.Id = userUnit.Id;
            user.Pseudo = userUnit.Pseudo;

            return user;
        }

    }
}