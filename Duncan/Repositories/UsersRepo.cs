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
    }
}

