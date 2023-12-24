using Duncan.Model;
using Duncan.Repositories;
using Microsoft.AspNetCore.Mvc;
using Shard.Shared.Core;

namespace Duncan.Services
{
    public class BuildingsService
    {
        private readonly MapGeneratorWrapper _map;
        private readonly IClock _clock;
        private readonly SystemsRepo _systemsRepo;
        private readonly PlanetsRepo _planetRepo;

        public BuildingsService(MapGeneratorWrapper mapGenerator, IClock clock, SystemsRepo systemsRepo, PlanetsRepo planetRepo)
        {
            _map = mapGenerator; 
            _systemsRepo = systemsRepo;
            _planetRepo = planetRepo;
            _clock = clock;
        }
        public bool ValidateResourceCategory(string resourceCategory)
        {
            IList<string> resourceKinds = new List<string> { "solid", "liquid", "gaseous" };
            return resourceKinds.Contains(resourceCategory);
        }

        public void RunTasksOnBuilding(Building building, User user)
        {
            building.Task = ProcessBuild(building);
            building.TaskTwo = ProcessExtract(building, user, building.ResourceCategory);
        }

        public Building CreateBuilding(BuildingBody building, Unit unitFound)
        {
            return new Building
            {
                Id = building.Id,
                BuilderId = building.BuilderId,
                Type = building.Type,
                System = unitFound.System,
                Planet = unitFound.Planet,
                IsBuilt = false,
                EstimatedBuildTime = _clock.Now.AddMinutes(5),
                ResourceCategory = building.ResourceCategory,
            };
        }

        public ActionResult<Building> ValidateBuilding(BuildingBody building, Unit builderUnit)
        {
            if (building == null)
                return new BadRequestObjectResult("Building is missing");

            if (building.BuilderId == null)
                return new BadRequestObjectResult("Builder id is missing");

            if (building.Type != "mine" && building.Type != "starport")
                return new BadRequestObjectResult("Builder type is different than mine and starport");

            if (builderUnit.Id != building.BuilderId)
                return new BadRequestObjectResult("BuilderId is different than the unitfoundId");

            if (builderUnit.DestinationPlanet != builderUnit.Planet)
                return new BadRequestObjectResult("Unit is not over the planet");

            if (building.Type == "mine" && !ValidateResourceCategory(building.ResourceCategory))
                return new BadRequestObjectResult("Builder specified a type other than mine");

            return null;
        }

        public bool IsBuildingUnderConstruction(Building building)
        {
            var timeOfArrival = (building.EstimatedBuildTime.Value - _clock.Now).TotalSeconds;

            if (timeOfArrival <= 2)
            {
                return true;
            }
            else
            {
                building.IsBuilt = false;
                return false;
            }
        }

        public bool DeductResources(User user, Building building, string unitType)
        {
            if (IsInvalidUnitCase(unitType, building, user))
            {
                return false;
            }

            switch (unitType)
            {
                case "scout":
                    user.ResourcesQuantity["iron"] -= 5;
                    user.ResourcesQuantity["carbon"] -= 5;
                    break;

                case "builder":
                    user.ResourcesQuantity["iron"] -= 10;
                    user.ResourcesQuantity["carbon"] -= 5;
                    break;

                case "cargo":
                    user.ResourcesQuantity["iron"] -= 10;
                    user.ResourcesQuantity["carbon"] -= 10;
                    user.ResourcesQuantity["gold"] -= 5;
                    break;

                default:
                    return true;
            }

            return true;
        }

        public async Task ProcessBuild(Building building)
        {
            building.CancellationSource = new CancellationTokenSource();
            await _clock.Delay(300_000, building.CancellationSource.Token); //5 minutes
            building.IsBuilt = true;
            building.EstimatedBuildTime = null;
        }

        public void CancelBuildingTask(User? user, Building? building, Unit? unitBody) {
            building?.CancellationSource?.Cancel();
            user?.Buildings?.Remove(user?.Buildings?.FirstOrDefault(b => b.BuilderId == unitBody.Id));
        }

        public async Task ProcessExtract(Building building, User user, string resourceCategory)
        {
            SystemSpecification? system = _systemsRepo.GetSystemByName(building.System, _map.Map.Systems);
            PlanetSpecification? planet = _planetRepo.GetPlanetByName(building.Planet, system);

            await building.Task;
            await _clock.Delay(60000);

            if (building.IsBuilt == true && user != null && user.ResourcesQuantity != null)
            {
                var planetResources = planet.ResourceQuantity.ToDictionary(r => r.Key.ToString().ToLower(), r => r.Value);

                switch (resourceCategory)
                {
                    case "solid":
                        await ProcessSolidResources(building, user, planetResources);
                        break;

                    case "liquid":
                        await ProcessLiquidResources(user, planetResources);
                        break;

                    case "gaseous":
                        await ProcessGaseousResources(user, planetResources);                   
                        break;
                }
            }
        }

        private async Task ProcessSolidResources(Building building, User user, Dictionary<string, int> planetResources)
        {
            var solidResources = planetResources
                .Where(kv => !kv.Key.Equals("water", StringComparison.OrdinalIgnoreCase) &&
                             !kv.Key.Equals("oxygen", StringComparison.OrdinalIgnoreCase))
                .ToDictionary(kv => kv.Key, kv => kv.Value);

            string maxKey = solidResources.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;
            string minKey = solidResources.Aggregate((x, y) => x.Value < y.Value ? x : y).Key;

            var copy = user?.ResourcesQuantity?[maxKey];
            var value = planetResources[maxKey];

            if (maxKey == minKey)
            {
                await ProcessEqualSolidResources(user, planetResources, maxKey, value, copy);
            }
            else
            {
                await ProcessDifferentSolidResources(user, planetResources, solidResources, maxKey, minKey);
            }
        }
        private async Task ProcessEqualSolidResources(User user, Dictionary<string, int> planetResources, string maxKey, int value, int? copy)
        {
            while (user?.ResourcesQuantity?[maxKey] < value + copy)
            {
                user.ResourcesQuantity[maxKey] += 1;
                planetResources[maxKey] -= 1;
                await _clock.Delay(60000);
            }
        }

        private async Task ProcessDifferentSolidResources(User user, Dictionary<string, int> planetResources, Dictionary<string, int> solidResources, string maxKey, string minKey)
        {
            while (planetResources[maxKey] > planetResources[minKey])
            {
                user.ResourcesQuantity[maxKey] += 1;
                planetResources[maxKey] -= 1;
                maxKey = solidResources.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;
                minKey = solidResources.Aggregate((x, y) => x.Value < y.Value ? x : y).Key;
                await _clock.Delay(60000);
            }

            while (planetResources[maxKey] > 0 && planetResources[minKey] > 0)
            {
                user.ResourcesQuantity[minKey] += 1;
                planetResources[minKey] -= 1;
                await _clock.Delay(60000);
                user.ResourcesQuantity[maxKey] += 1;
                planetResources[maxKey] -= 1;
                await _clock.Delay(60000);
            }
        }

        private async Task ProcessLiquidResources(User user, Dictionary<string, int> planetResources)
        {
            while (planetResources["water"] > 0)
            {
                user.ResourcesQuantity["water"] += 1;
                planetResources["water"] -= 1;
                await _clock.Delay(60000);
            }
        }
        private async Task ProcessGaseousResources(User user, Dictionary<string, int> planetResources)
        {
            while (planetResources["oxygen"] > 0)
            {
                user.ResourcesQuantity["oxygen"] += 1;
                planetResources["oxygen"] -= 1;
                await _clock.Delay(60000);
            }
        }

        private bool IsInvalidUnitCase([FromRoute] string unitType, Building building, User user)
        {
            return unitType switch
            {
                "scout" when (building.Type == "mine" || building.IsBuilt == false) => true,
                "scout" when (user.ResourcesQuantity?["iron"] < 5 || user.ResourcesQuantity?["carbon"] < 5) => true,
                "builder" when (user.ResourcesQuantity?["iron"] < 10 || user.ResourcesQuantity?["carbon"] < 5) => true,
                "cargo" when (user.ResourcesQuantity?["iron"] < 10 || user.ResourcesQuantity?["carbon"] < 10 || user.ResourcesQuantity?["gold"] < 5) => true,
                _ => false
            };
        }
    }
}

