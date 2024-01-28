using Duncan.Model;
using Duncan.Repositories;
using Duncan.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Shard.Shared.Core;
using System.Net.Http.Headers;
using System.Text;


namespace Duncan.Services
{
    public class UnitsService
    {
        private readonly IClock _clock;
        private readonly UsersRepo _usersRepo;
        private readonly UnitsRepo _unitsRepo;
        private readonly BuildingsService _buildingsService;
        private Dictionary<string, Wormholes> _wormholes;
        private readonly IHttpClientFactory _httpClientFactory;

        public UnitsService(IClock clock, UsersRepo usersRepo, UnitsRepo unitsRepo, IOptions<Dictionary<string, Wormholes>> wormholes, BuildingsService buildingsService, IHttpClientFactory httpClientFactory)
        {
            _clock = clock;
            _usersRepo = usersRepo;
            _unitsRepo = unitsRepo;
            _wormholes = wormholes.Value;
            _httpClientFactory = httpClientFactory;
            _buildingsService = buildingsService;
        }

        public async Task WaitingUnit(Unit currentUnit, Unit unitWithNewDestination)
        {
            currentUnit.DestinationPlanet = unitWithNewDestination.DestinationPlanet;
            currentUnit.DestinationSystem = unitWithNewDestination.DestinationSystem;

            var travelTime = currentUnit.DestinationSystem == currentUnit.System ? 0 : 60;
            travelTime += currentUnit.DestinationPlanet == currentUnit.Planet ? 0 : 15;

            if (travelTime == 0) {
                return; 
            }

            currentUnit.Planet = null;

            await Waiting(currentUnit, travelTime);

            currentUnit.System = currentUnit.DestinationSystem;
            currentUnit.Planet = currentUnit.DestinationPlanet;
        }


        private async Task Waiting(Unit currentUnit, int time)
        {
            currentUnit.EstimatedTimeOfArrival = _clock.Now.AddSeconds(time);
            await _clock.Delay(time * 1000);
        }


        public async Task VerifyTimeDifference(Unit unit)
        {
            if (unit.EstimatedTimeOfArrival.HasValue)
            {
                var timeOfArrival = (unit.EstimatedTimeOfArrival.Value - _clock.Now).TotalSeconds;

                if (timeOfArrival <= 2)
                {
                    if (unit.Task != null)
                        await unit.Task;
                }
                else
                    unit.Planet = null;
            }
        }

        public void LoadAndUnloadResources(User user ,Unit unitFound, Unit unitBody)
        {
            foreach (var resource in unitBody.ResourcesQuantity.Keys.ToList())
            {
                int bodyQuantity = unitBody.ResourcesQuantity[resource];
                int cargoQuantity = unitFound.ResourcesQuantity[resource];
                int diff = bodyQuantity - cargoQuantity;

                if (diff > 0)
                {
                    unitFound.ResourcesQuantity[resource] += diff;
                    user.ResourcesQuantity[resource] -= diff;
                }
                else if (diff < 0)
                {
                    user.ResourcesQuantity[resource] -= diff;
                    unitFound.ResourcesQuantity[resource] += diff;
                }
            }
        }
        public void LaunchAllUnitsFight()
        {
            foreach (Unit unit in GetAllUnits())
            {
                if (IsCombatUnit(unit.Type))
                {
                    ProcessAttack(unit);
                }
            };

            foreach (var unit in GetAllUnits().Where(unit => unit.Health <= 0))
            {
                if(unit.Type != "cargo") _usersRepo?.GetUserWithUnitId(unit.Id)?.Units?.Remove(unit);
            }
        }

        private void ProcessAttack(Unit unitThatAttack)
        { 
            User? userOwner = _usersRepo.GetUserWithUnitId(unitThatAttack.Id);
            List<Unit> enemies = userOwner is not null ? GetEnemies(userOwner) : new List<Unit>() ;
            List<Unit> enemiesAtSameLocation = GetEnemiesAtSameLocation(enemies, userOwner);
            List<Unit>? enemiesOrderedByPriorityThenByHealth = GetPriorityByType(unitThatAttack.Type)
                ?.SelectMany(priorityList => enemiesAtSameLocation.Where(unit => priorityList.Contains(unit.Type)))
                .ToList();
            Unit? targetUnit = enemiesOrderedByPriorityThenByHealth?.FirstOrDefault();

            if (targetUnit is null) return;

            targetUnit.Health -= GetUnitDamage(unitThatAttack.Type, targetUnit.Type);
        }
        private static List<Unit> GetEnemiesAtSameLocation(List<Unit> enemies, User userOwner)
        {
            return enemies
            .Where(unitEnemie =>
                userOwner.Units.Any(unit =>
                    (unit.Planet.Equals(unitEnemie.Planet) && unit.System.Equals(unitEnemie.System)) ||
                    (!unit.Planet.Equals(unitEnemie.Planet) && unit.System.Equals(unitEnemie.System))
                )
            )
            .ToList();
        }
        private List<Unit> GetEnemies(User userOwner)
        {
            return GetAllUnits()
                .Where(unit => userOwner.Units is not null && !userOwner.Units.Any(userUnit => userUnit.Equals(unit)))
                .Where(unit => unit.Type != "scout" && unit.Type != "builder")
                .ToList();
        }
        private List<Unit> GetAllUnits()
        {
            List<User>? users = _usersRepo.GetUsers();

            return (users ?? new List<User>())
                .Where(u => u.Units != null) 
                .SelectMany(u => u.Units ?? Enumerable.Empty<Unit>())
                .ToList();
        }
        private static List<string>? GetPriorityByType(string type) => type switch
        {
            "bomber" => new List<string> { "cruiser", "bomber", "fighter" },
            "fighter" => new List<string> { "bomber", "fighter", "cruiser" },
            "cruiser" => new List<string> { "fighter", "cruiser", "bomber" },
            _ => null
        };
        private static bool IsCombatUnit(string unitType)
        {
            return unitType == "cruiser" || unitType == "bomber" || unitType == "fighter";
        }
        private int? GetUnitDamage(string? unitTypeThatAttack, string? targetUnitType)
        {
            if (unitTypeThatAttack == "bomber" && _clock.Now.Second % 60 == 0)
            {
                return 400;
            }

            return (unitTypeThatAttack, targetUnitType) switch
            {
                ("bomber", "cruiser") => 0,
                ("bomber", "fighter") => 0,
                ("bomber", "bomber") => 0,
                ("fighter", _) => 10,
                ("cruiser", "bomber") => 4,
                ("cruiser", _) => 40,
                _ => null
            };
        }

        public bool NeedToLoadOrUnloadResources(Unit? unitFound, Unit unitBody)
        {
            var first = unitFound?.ResourcesQuantity;
            var second = unitBody?.ResourcesQuantity;
            if (first is null && second is null) return false;
            if (first is not null && second is null) return true;
            if (first is null && second is not null) return true;
            return !first.OrderBy(kv => kv.Key).SequenceEqual(second.OrderBy(kv => kv.Key));
        }


        // functions usefull for path => [HttpPut("users/{userId}/Units/{unitId}")]
        public async Task HandleExistingUnit(Unit? unitFound, Unit unitBody)
        {
            if (unitFound != null)
            {
                unitFound.Task = WaitingUnit(unitFound, unitBody);
            }
        }

        public ActionResult<Unit?> HandleNonExistingUnit(User user, bool isAdmin, bool isFakeRemoteUser, Unit unitBody)
        {
            if (!isAdmin && !isFakeRemoteUser && unitBody.Type == "cargo")
            {
                user?.Units?.Add(unitBody);
                return unitBody;
            }
            else if (!isAdmin && !isFakeRemoteUser)
                return new UnauthorizedObjectResult("Unauthorized");
            else if (isAdmin)
                return CreateAdminUnit(user, unitBody);
            else if (isFakeRemoteUser)
                return CreateFakeRemoteUnit(user, unitBody);

            return new BadRequestObjectResult("Invalid request");
        }

        public async Task<ActionResult<Unit?>> ProcessUnitChange(User user, bool isAdmin, bool isFakeRemoteUser, Unit unitBody, Unit unitFound)
        {
            var resourceLoadingResult = await HandleResourceLoading(user, unitFound, unitBody);
            if (resourceLoadingResult != null)
                return resourceLoadingResult;

            if (_usersRepo.CheckIfUserHaveNotEnoughResources(user))
                return new BadRequestObjectResult("User has not enough resources");

            var building = user?.Buildings?.FirstOrDefault(b => b.BuilderId == unitBody.Id);
            if (building != null && _unitsRepo.CheckIfThereIsAFakeMoveOfUnit(unitBody))
                _buildingsService.CancelBuildingTask(user, building, unitBody);

            if (unitBody.DestinationShard != null)
                return await MoveUnitToAnotherShard(user, unitBody, unitFound);

            return unitFound;
        }

        private ActionResult<Unit?> CreateAdminUnit(User user, Unit unitBody)
        {
            user?.Units?.Add(unitBody);
            unitBody.DestinationPlanet = unitBody.Planet;
            unitBody.DestinationSystem = unitBody.System;
            unitBody.Health = GetInitialHealth(unitBody);
            return unitBody;
        }

        private ActionResult<Unit?> CreateFakeRemoteUnit(User user, Unit unitBody)
        {
            unitBody.System = "80ad7191-ef3c-14f0-7be8-e875dad4cfa6";
            user?.Units?.Add(unitBody);
            return unitBody;
        }

        private int? GetInitialHealth(Unit unit)
        {
            return unit.Type switch
            {
                "bomber" => 50,
                "fighter" => 80,
                "cruiser" => 400,
                _ => unit.Health
            };
        }

        private async Task<ActionResult<Unit?>?> HandleResourceLoading(User user, Unit unitFound, Unit unitBody)
        {
            if (NeedToLoadOrUnloadResources(unitFound, unitBody))
            {
                if (unitFound.Type == "cargo")
                {
                    if (!_unitsRepo.CheckIfThereIsStarportOnPlanet(user, unitFound))
                        return new BadRequestObjectResult("There is no starport on the planet");

                    LoadAndUnloadResources(user, unitFound, unitBody);
                }
                else return new BadRequestObjectResult("Unit type is not equal to cargo");
            }

            return null;
        }

        private async Task<ActionResult<Unit?>> MoveUnitToAnotherShard(User user, Unit unitBody, Unit unitFound)
        {
            var warmhole = _wormholes[unitBody.DestinationShard];
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(warmhole.BaseUri.ToString());
            client.DefaultRequestHeaders.Authorization = CreateShardAuthorizationHeader(warmhole.User, warmhole.SharedPassword);

            await client.PutAsJsonAsync($"users/{user.Id}", user);
            await client.PutAsJsonAsync($"users/{user.Id}/units/{unitBody.Id}", unitFound);

            user?.Units?.Remove(unitFound);

            return new RedirectResult(warmhole.BaseUri.ToString() + $"users/{user.Id}/units/{unitBody.Id}", true, true);
        }

        private AuthenticationHeaderValue CreateShardAuthorizationHeader(string shardName, string sharedKey)
           => new("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"shard-{shardName}:{sharedKey}")));

    }
}
