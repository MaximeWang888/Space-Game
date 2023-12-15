using Duncan.Helper;
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

        private readonly UserDB? _userDB;
        private readonly MapGeneratorWrapper _map;
        private readonly UsersRepo _usersRepo;

        public UsersController(UserDB userDB, MapGeneratorWrapper mapGenerator, UsersRepo usersRepo)
        {
            _userDB = userDB;
            _map = mapGenerator;
            _usersRepo = usersRepo;
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

            var existingUser = _userDB?.users.FirstOrDefault(u => u.Id == id);

            bool isAdmin = HelperAuth.isAdmin(Request);

            if (existingUser != null && isAdmin)
            {
                existingUser.ResourcesQuantity = user.ResourcesQuantity;

                return Ok(existingUser); 
            }

            Unit unit_1 = new Unit
            {
                Planet = _map.Map.Systems.First().Planets.First().Name,
                System = _map.Map.Systems.First().Name,
                DestinationSystem = _map.Map.Systems.First().Name,
                Type = "scout",
                Health = 50
            };

            Unit unit_2 = new Unit
            {
                Planet = _map.Map.Systems.First().Planets.First().Name,
                System = _map.Map.Systems.First().Name,
                DestinationSystem = _map.Map.Systems.First().Name,
                Type = "builder",
                Health = 50
            };

            var ResourcesQuantity = new Dictionary<string, int>
               {
                {"titanium", 0},
                {"gold", 0},
                {"aluminium", 0},
                {"iron", 10},
                {"carbon", 20},
                {"oxygen", 50},
                {"water", 50}
            };

            if (HelperAuth.isAdmin(Request))
            {
                foreach (var (key, value) in user.ResourcesQuantity)
                {
                    ResourcesQuantity[key] = value;
                }
            }

            user.Id = user.Id;
            user.Pseudo = user.Pseudo;

            if (HelperAuth.isFakeRemoteUser(Request))
            {
                user.DateOfCreation = user.DateOfCreation;
                user.ResourcesQuantity = ResourcesQuantity.ToDictionary(kv => kv.Key, kv => 0);
            }
            else
            {
                user.DateOfCreation = new DateTime();
                user.ResourcesQuantity = ResourcesQuantity;
                user.Units?.Add(unit_1);
                user.Units?.Add(unit_2);
            }

            _userDB?.users.Add(user);

            return user;
        }

        [SwaggerOperation(Summary = "Get a specific user")]
        [HttpGet("{id}")]
        public ActionResult<User> GetUserById(string id)
        {
            User? userUnit = _usersRepo.GetUserWithUnitsByUserId(id);

            if (userUnit == null)
                return NotFound();

            User user = new User();
            user.Id = userUnit.Id;
            user.Pseudo = userUnit.Pseudo;
            user.ResourcesQuantity = userUnit.ResourcesQuantity;
            user.DateOfCreation = userUnit.DateOfCreation;

            return user;
        }

    }
}