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

        [SwaggerOperation(Summary = "Create a new user")]
        [HttpPut("{id}")]
        public ActionResult<User> CreateNewUser(string id, [FromBody] User user)
        {
            if (user == null)
                return BadRequest("Request body is required");

            if (user.Id?.Length < 2)
                return BadRequest("Invalid user ID");

            if (user.Id != id)
                return BadRequest("Inconsistent user ID");

            var existingUser = _userDB?.users.FirstOrDefault(u => u.Id == id);

            bool isAdmin = User.IsInRole("admin");

            bool isFakeRemoteUser = User.IsInRole("shard");

            if (existingUser != null && isAdmin)
            {
                existingUser.ResourcesQuantity = user.ResourcesQuantity;

                return Ok(existingUser); 
            }

            var systems = _map.Map.Systems;
            var random = new Random();
            var shuffledSystems = systems.OrderBy(x => random.Next()).ToList();
            var randomPlanetName = shuffledSystems.First().Planets.First().Name;

            Unit unit_1 = new Unit
            {
                Planet = randomPlanetName,
                System = _map.Map.Systems.First().Name,
                DestinationSystem = _map.Map.Systems.First().Name,
                Type = "scout",
                Health = 50
            };

            Unit unit_2 = new Unit
            {
                Planet = randomPlanetName,
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


            if (isAdmin)
            {
                foreach (var (key, value) in user.ResourcesQuantity)
                {
                    ResourcesQuantity[key] = value;
                }
            }

            user.Id = user.Id;
            user.Pseudo = user.Pseudo;

            if (isFakeRemoteUser)
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

        [SwaggerOperation(Summary = "Returns details of an existing user")]
        [HttpGet("{id}")]
        public ActionResult<User> GetUserById(string id)
        {
            User? user = _usersRepo.GetUserWithUnitsByUserId(id);

            if (user == null)
                return NotFound("User not found");

            return user;
        }

    }
}