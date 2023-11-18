using Duncan.Model;
using Shard.Shared.Core;

namespace Duncan.Services
{
    public class BuildingsService
    {
        private readonly IClock _clock;
        public BuildingsService(IClock clock)
        {
            this._clock = clock;
        }
        public async Task processBuild(Building building, IDictionary<string, int> resourcesQuantity)
        {
            if (building.IsBuilt == false)
            {
                await _clock.Delay(300000); //5 minutes
                building.IsBuilt = true;
                building.EstimatedBuildTime = null;
                await _clock.Delay(60000); //6 minutes
                resourcesQuantity["carbon"] += 1;
            }
        }
        public async Task processExtract(String ressourceCategory, IDictionary<string, int> resourcesQuantity)
        {
          switch(ressourceCategory)
            {
                case "solid":
                    await _clock.Delay(360000); //6 minutes
                    resourcesQuantity["aluminium"] += 64;
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

