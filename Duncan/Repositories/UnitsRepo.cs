using Duncan.Model;

namespace Duncan.Utils
{
    public class UnitsRepo
    {
        public Unit? GetUnitByUnitId(string unitId, UserWithUnits userWithUnits)
        {
            return userWithUnits.Units?.FirstOrDefault(u => u.Id == unitId);
        }
        public Unit? GetUnitWithType(string type, UserWithUnits userWithUnits)
        {
            return userWithUnits.Units?.FirstOrDefault(u => u.Type == type);
        }
    }
}
