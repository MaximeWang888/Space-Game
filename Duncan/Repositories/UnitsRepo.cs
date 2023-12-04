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
        //public void DeleteUnit(Unit unit, User user)
        //{
        //    user.Units?.Remove(unit);
        //}

    }
}
