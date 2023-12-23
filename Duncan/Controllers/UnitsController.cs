using Duncan.Model;
using Duncan.Repositories;
using Duncan.Services;
using Duncan.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Shard.Shared.Core;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Http.Headers;
using System.Text;

namespace Duncan.Controllers
{
    public class UnitsController : ControllerBase
    {
        private readonly MapGeneratorWrapper _map;
        private readonly UsersRepo _usersRepo;
        private readonly UnitsRepo _unitsRepo;
        private readonly SystemsRepo _systemsRepo;
        private readonly PlanetsRepo _planetRepo;
        private readonly UnitsService _unitsService;
        private readonly BuildingsService _buildingsService;
        private Dictionary<string, Wormholes> _wormholes;
        private readonly IHttpClientFactory _httpClientFactory;

        public UnitsController(IHttpClientFactory httpClientFactory, MapGeneratorWrapper mapGenerator, UsersRepo usersRepo, UnitsRepo unitsRepo, UnitsService unitsService, SystemsRepo systemsRepo, PlanetsRepo planetRepo, IOptions<Dictionary<string, Wormholes>> wormholes, BuildingsService buildingsService)
        {
            _map = mapGenerator;
            _unitsRepo = unitsRepo;
            _usersRepo = usersRepo;
            _systemsRepo = systemsRepo;
            _planetRepo = planetRepo;
            _unitsService = unitsService;
            _wormholes = wormholes.Value;
            _httpClientFactory = httpClientFactory;
            _buildingsService = buildingsService;
        }

        [SwaggerOperation(Summary = "Return all units of a user.")]
        [HttpGet("users/{userId}/Units")]
        public ActionResult<List<Unit>> GetAllUnits([FromRoute] string userId)
        {
            User? user = _usersRepo.GetUserWithUnitsByUserId(userId);

            if (user == null) 
                return NotFound("Not Found user");

            List<Unit> units = user.Units ?? new List<Unit>();

            return units;
        }

        [SwaggerOperation(Summary = "Return information about one single unit of a user.")]
        [HttpGet("users/{userId}/Units/{unitId}")]
        public async Task<ActionResult<Unit>> GetUnitInformation([FromRoute] string userId, [FromRoute] string unitId)
        {
            User? user = _usersRepo.GetUserWithUnitsByUserId(userId);

            if (user == null)
                return NotFound("Not Found userWithUnits");

            Unit? unitFound = _unitsRepo.GetUnitByUnitId(unitId, user);

            if (unitFound == null)
                return NotFound("Not Found unitFound");

            await _unitsService.VerifyTimeDifference(unitFound);

            return unitFound;
        }

        [SwaggerOperation(Summary = "Change the status of a unit of a user. Right now, only its position (system and planet) can be changed - which is akin to moving it. " +
            "If the unit does not exist and the authenticated user is administrator, creates the unit")]
        [HttpPut("users/{userId}/Units/{unitId}")]
        public async Task<ActionResult<Unit?>> ChangeStatusOfUnit([FromRoute] string userId, [FromRoute] string unitId, [FromBody] Unit unitBody)
        {
            User? user = _usersRepo.GetUserWithUnitsByUserId(userId);

            if (user == null)
                return NotFound("Not Found userWithUnits");

            Unit? unitFound = _unitsRepo.GetUnitByUnitId(unitId, user);

            bool isAdmin = User.IsInRole("admin");

            bool isFakeRemoteUser = User.IsInRole("shard");

            var building = user?.Buildings?.FirstOrDefault(b => b.BuilderId == unitBody.Id);

            if(unitFound != null) {
                unitFound.Task = _unitsService.WaitingUnit(unitFound, unitBody);
            }

            if (unitFound == null && !isAdmin && !isFakeRemoteUser && unitBody.Type == "cargo") 
            {
                user?.Units?.Add(unitBody);
                return unitBody; 
            }

            else if (unitFound == null && !isAdmin && !isFakeRemoteUser)
                return Unauthorized("Unauthorized");

            else if (isAdmin)
            {
                user?.Units?.Add(unitBody);
                unitBody.DestinationPlanet = unitBody.Planet;
                unitBody.DestinationSystem = unitBody.System;
                unitBody.Health = unitBody.Type switch
                {
                    "bomber" => 50,
                    "fighter" => 80,
                    "cruiser" => 400,
                    _ => unitBody.Health
                };
                return unitBody;
            }
            else if (isFakeRemoteUser)
            {
                unitBody.System = "80ad7191-ef3c-14f0-7be8-e875dad4cfa6";
                user?.Units?.Add(unitBody);
                return unitBody;
            }

            if(_unitsService.NeedToLoadOrUnloadResources(unitFound, unitBody))
            {
                if (unitFound.Type == "cargo")
                {
                    if (!_unitsRepo.CheckIfThereIsStarportOnPlanet(user, unitFound))
                        return BadRequest("There is no starport on the planet");

                    _unitsService.LoadAndUnloadResources(user, unitFound, unitBody);
                }
                else return BadRequest("Unit type is not equal to cargo");
            }

            if (_usersRepo.CheckIfUserHaveNotEnoughResources(user))
            {
                return BadRequest("User has not enough resources");
            }

            if (building != null && _unitsRepo.CheckIfThereIsAFakeMoveOfUnit(unitBody))
            {
                _buildingsService.CancelBuildingTask(user, building, unitBody);
            }

            if (unitBody.DestinationShard != null)
            {
                var warmhole = _wormholes[unitBody.DestinationShard];
                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri(warmhole.BaseUri.ToString());
                client.DefaultRequestHeaders.Authorization = CreateShardAuthorizationHeader(warmhole.User, warmhole.SharedPassword);

                var response = await client.PutAsJsonAsync($"users/{userId}", user);
                response = await client.PutAsJsonAsync($"users/{userId}/units/{unitId}", unitFound);

                user?.Units?.Remove(unitFound);
             
                return new RedirectResult(warmhole.BaseUri.ToString() + $"users/{userId}/units/{unitId}", true, true);
            }

            return unitFound;
        }

        [SwaggerOperation(Summary = "Returned more detailled information about the location a unit of user currently is about.")]
        [HttpGet("users/{userId}/Units/{unitId}/location")]
        public ActionResult<UnitLocation> GetUnitLocation([FromRoute] string userId, [FromRoute] string unitId)
        {
            User? user = _usersRepo.GetUserWithUnitsByUserId(userId);
            if (user == null)
                return NotFound("User not found");

            Unit? unitFound = _unitsRepo.GetUnitByUnitId(unitId, user);
            if (unitFound == null)
                return NotFound("Unit not found");

            UnitLocation unitInformation = new UnitLocation();
            unitInformation.System = unitFound.System;
            unitInformation.Planet = unitFound.Planet;

            SystemSpecification? system = _systemsRepo.GetSystemByName(unitFound.System, _map.Map.Systems);
            if (system == null)
                return NotFound("System not found");

            PlanetSpecification? planet = _planetRepo.GetPlanetByName(unitFound.Planet, system);
            if (planet == null)
                return NotFound("Planet not found");

            if (unitFound.Type == "scout")
                unitInformation.ResourcesQuantity = planet.ResourceQuantity.ToDictionary(r => r.Key.ToString().ToLower(), r => r.Value);

            return unitInformation;
        }

        private static AuthenticationHeaderValue CreateShardAuthorizationHeader(string shardName, string sharedKey)
                   => new("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"shard-{shardName}:{sharedKey}")));
    }
}
