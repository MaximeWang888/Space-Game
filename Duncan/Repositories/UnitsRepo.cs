using Duncan.Model;

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

        public Unit? CreateUnitWithType(string type, string system, string planet)
        {
            Unit unit = new Unit
            {
                Planet = planet,
                System = system,
                DestinationSystem = system,
                Type = type,
                Health = GetHealthByType(type)
            };
            return unit;
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
                default:
                    return 0;
            }
        }
    }
}
