using Duncan.Model;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Duncan.Utils
{
    public class UnitsRepo
    {
        public Unit? GetUnitByUnitId(string unitId, User user)
        {
            return user.Units?.FirstOrDefault(u => u.Id == unitId);
        }
        public Unit? GetUnitWithType(string type, User user)
        {
            return user.Units?.FirstOrDefault(u => u.Type == type);
        }

        public Unit CreateUnitWithType(string type, string system, string planet)
        {
            Unit unit = new Unit
            {
                Planet = planet,
                System = system,
                DestinationPlanet = planet,
                DestinationSystem = system,
                Type = type,
                Health = GetHealthByType(type)
            };
            return unit;
        }

        public bool CheckIfThereIsStarportOnPlanet(User user, Unit unitFound)
        {
            var buildingOnPlanet = user.Buildings?.Where(b => b.Planet == unitFound.Planet);
           
            return buildingOnPlanet.Any(b => b.Type == "starport");
        }

        public bool CheckIfThereIsAFakeMoveOfUnit( Unit unitBody)
        {
            return ((unitBody.DestinationSystem == unitBody.System && unitBody.DestinationPlanet != unitBody.Planet) ||
                 (unitBody.DestinationSystem != unitBody.System && unitBody.DestinationPlanet == unitBody.Planet));
        }

        private static int GetHealthByType(string type)
        {
            switch (type)
            {
                case "bomber":
                    return 50;
                case "fighter":
                    return 80;
                case "cruiser":
                    return 400;
                case "cargo":
                    return 100;
                default:
                    return 0;
            }
        }
    }
}
