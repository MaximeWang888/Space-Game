using Duncan.Model;

namespace Duncan.Utils
{
    public class UnitsRepo
    {
        public Unit? GetUnitByUnitId(string unitId, User userWithUnits)
        {
            return userWithUnits.Units?.FirstOrDefault(u => u.Id == unitId);
        }
        public Unit? GetUnitWithType(string type, User userWithUnits)
        {
            return userWithUnits.Units?.FirstOrDefault(u => u.Type == type);
        }
    }
}
