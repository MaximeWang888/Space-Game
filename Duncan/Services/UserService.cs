//using Duncan.Model;

//namespace Duncan.Services
//{
//    public interface IUserService
//    {
//        Task<User> Authenticate(string username, string password);
//        Task<IEnumerable<User>> GetAll();
//    }
//    public class UserService : IUserService
//    {
//        // users hardcoded for simplicity, store in a db with hashed passwords in production applications
//        private List<User> _users = new List<User>
//        {
//            //new User { Id = 1, FirstName = "Test", LastName = "User", Username = "test", Password = "test" }
//            new User { Id = "1", FirstName = "Test", LastName = "User", Pseudo = "User_Pseudo", DateOfCreation = new DateTime(), ResourcesQuantity = new Dictionary<string, int>()}
//        };

//        public async Task<User> Authenticate(string pseudo, string password)
//        {
//            var user = await Task.Run(() => _users.SingleOrDefault(x => x.Pseudo == pseudo && x.Password == password));

//            // return null if user not found
//            if (user == null)
//                return null;

//            // authentication successful so return user details without password
//            return user.WithoutPassword();
//        }

//        public async Task<IEnumerable<User>> GetAll()
//        {
//            return await Task.Run(() => _users.WithoutPasswords());
//        }
//    }
//}
