using Duncan.Model;
using Duncan.Repositories;
using Shard.Shared.Core;

namespace Duncan.Services
{
    public class BuildingsService
    {
        private readonly MapGeneratorWrapper _map;
        private readonly IClock _clock;
        private readonly SystemsRepo _systemsRepo;
        private readonly PlanetRepo _planetRepo;
        public BuildingsService(MapGeneratorWrapper mapGenerator, IClock clock, SystemsRepo systemsRepo, PlanetRepo planetRepo)
        {
            this._map = mapGenerator; 
            this._systemsRepo = systemsRepo;
            this._planetRepo = planetRepo;
            this._clock = clock;
        }
        public async Task ProcessBuild(Building building)
        {
            building.CancellationSource = new CancellationTokenSource();
            await _clock.Delay(300000, building.CancellationSource.Token); //5 minutes
            building.IsBuilt = true;
            building.EstimatedBuildTime = null;
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
    }
}

