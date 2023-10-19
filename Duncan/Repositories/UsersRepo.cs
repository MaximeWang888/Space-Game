using Duncan.Model;

namespace Duncan.Repositories
{
    public class UsersRepo
    {
        private readonly UserDB? _userDB;

        public UsersRepo(UserDB userDB)
        {
            this._userDB = userDB;
        }
        public UserWithUnits? GetUserWithUnitsByUserId(string userId)
        {
            return _userDB?.users.FirstOrDefault(u => u.Id == userId);
        }
    }
}

