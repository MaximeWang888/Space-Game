using Duncan.Model;

namespace Duncan.Repositories
{
    public class UsersRepo
    {
        private readonly UserDB? _userDB;

        public UsersRepo(UserDB userDB)
        {
            _userDB = userDB;
        }
        public User? GetUserWithUnitsByUserId(string userId)
        {
            return _userDB?.users.FirstOrDefault(u => u.Id == userId);
        }

        internal List<User>? GetUsers()
        {
            return _userDB?.users; 
        }
        internal User? GetUserWithUnitId(string unitId)
        {
            return _userDB?.users.FirstOrDefault(user => user.Units.Any(unit => unit.Id == unitId));
        }

        internal bool CheckIfUserHaveNotEnoughResources(User user) 
        { 
            return user.ResourcesQuantity.Any(kv => kv.Value < 0);
        }
    }
}

