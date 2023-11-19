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
        public async Task processBuild(UserWithUnits user,Building building, IDictionary<string, int> resourcesQuantity,string builderId)
        {
            var timeOfArrival = (building.EstimatedBuildTime.Value - _clock.Now).TotalSeconds;

            var unit = user.Units.FirstOrDefault((u) => u.Id == builderId);

            if (timeOfArrival == 60)
            {
                building.IsBuilt = false;
                building.EstimatedBuildTime = building.EstimatedBuildTime.Value;
            }

            if (building.IsBuilt == false)
            {
                await _clock.Delay(300000); //5 minutes
                building.IsBuilt = true;
                building.EstimatedBuildTime = null;
            }
        }
        public async Task processExtract(Building building, UserWithUnits user, String ressourceCategory, IDictionary<string, int> resourcesQuantity)
        {
            SystemSpecification? system = _systemsRepo.GetSystemByName(building.System, _map.Map.Systems);

            PlanetSpecification? planet = _planetRepo.GetPlanetByName(building.Planet, system);

            var planetResources = planet.ResourceQuantity.ToDictionary(r => r.Key.ToString().ToLower(), r => r.Value);

            switch (ressourceCategory)
            {
                case "solid":
                    await _clock.Delay(360000); //6 minutes

                    var solidResources = planetResources
                         .Where(kv => !kv.Key.Equals("water", StringComparison.OrdinalIgnoreCase) &&
                         !kv.Key.Equals("oxygen", StringComparison.OrdinalIgnoreCase))
                        .ToDictionary(kv => kv.Key, kv => kv.Value);

                    string maxKey = solidResources.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;
                    string minKey = solidResources.Aggregate((x, y) => x.Value < y.Value ? x : y).Key;

                    var copy = user.ResourcesQuantity[maxKey];

                    var value = planetResources[maxKey];

                    if (maxKey == minKey)
                    {
                        while (user.ResourcesQuantity[maxKey] < value + copy)
                        {
                            user.ResourcesQuantity[maxKey] += 1;
                            planetResources[maxKey] -= 1;
                            await _clock.Delay(60000);
                        }
                    }
                    else
                    {
                        // planetResources.carbon 63
                        // planetResources.iron 28

                        // planetResources.carbon 28
                        // exit - boucle

                        // Mandatory testing equality
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
                    break;

                case "liquid":
                    await _clock.Delay(360000); //6 minutes
                    resourcesQuantity["water"] += 15;
                    break;

                case "gaseous":
                    await _clock.Delay(360000); //6 minutes
                    resourcesQuantity["oxygen"] += 113;
                    break;
            }
        }
    }
}

