using Duncan.Model;
using Duncan.Repositories;
using Microsoft.AspNetCore.Http.HttpResults;
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

            var ok = planet.ResourceQuantity.ToDictionary(r => r.Key.ToString().ToLower(), r => r.Value);

            switch (ressourceCategory)
            {
                case "solid":

                    await _clock.Delay(360000); //6 minutes
                    resourcesQuantity["aluminium"] += 64;

                     while (ok["carbon"] > ok["iron"])
                     {
                            user.ResourcesQuantity["carbon"] += 1;
                            ok["carbon"] -= 1;
                            await _clock.Delay(60000);
                     }
                    user.ResourcesQuantity["iron"] += 1;

                    await _clock.Delay(60000);

                    user.ResourcesQuantity["carbon"] += 1;
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

