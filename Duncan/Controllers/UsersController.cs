using Duncan.Model;
using Duncan.Repositories;
using Duncan.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Duncan.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {

        private readonly UserDB? _userDB;
        private readonly UsersRepo _usersRepo;
        private readonly UsersService _usersService;

        public UsersController(UserDB userDB, UsersRepo usersRepo, UsersService usersService)
        {
            _userDB = userDB;
            _usersRepo = usersRepo;
            _usersService = usersService;
        }

        [SwaggerOperation(Summary = "Create a new user")]
        [HttpPut("{id}")]
        public ActionResult<User> CreateNewUser([FromRoute] string id, [FromBody] User user)
        {
            bool isAdmin = User.IsInRole("admin");
            bool isFakeRemoteUser = User.IsInRole("shard");

            if (!_usersService.ValidateUserRequest(id, user, out var validationError))
                return BadRequest(validationError);

            var existingUser = _userDB?.users.FirstOrDefault(u => u.Id == id);

            if (_usersService.UpdateExistingUser(existingUser, user, isAdmin))
                return existingUser;

            _usersService.InitializeUser(user, isAdmin, isFakeRemoteUser);

            _userDB?.users.Add(user);

            return user;
        }

        [SwaggerOperation(Summary = "Returns details of an existing user")]
        [HttpGet("{id}")]
        public ActionResult<User> GetUserById([FromRoute] string id)
        {
            User? user = _usersRepo.GetUserWithUnitsByUserId(id);

            if (user == null)
                return NotFound("User not found");

            return user;
        }

    }
}