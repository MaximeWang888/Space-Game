using Duncan.Model;

namespace Duncan.Services
{
    public class UsersService
    {
        private readonly MapGeneratorWrapper _map;

        public UsersService(MapGeneratorWrapper mapGenerator)
        {
            _map = mapGenerator;
        }

        public bool ValidateUserRequest(string id, User user, out string validationError)
        {
            validationError = null;

            if (user == null)
            {
                validationError = "Request body is required";
                return false;
            }

            if (user.Id?.Length < 2)
            {
                validationError = "Invalid user ID";
                return false;
            }

            if (user.Id != id)
            {
                validationError = "Inconsistent user ID";
                return false;
            }

            return true;
        }

        public bool UpdateExistingUser(User existingUser, User newUser, bool isAdmin)
        {
            if (existingUser != null && isAdmin)
            {
                existingUser.ResourcesQuantity = newUser.ResourcesQuantity;
                return true;
            }

            return false;
        }

        public void InitializeUser(User user, bool isAdmin, bool isFakeRemoteUser)
        {
            user.ResourcesQuantity = isAdmin || isFakeRemoteUser ? GetEmptyResources() : GetInitalResources();

            user.Id = user.Id;
            user.Pseudo = user.Pseudo;

            if (!isFakeRemoteUser)
            {
                List<Unit> initialUnits = GetInitialUnits();
                user.DateOfCreation = new DateTime();
                user.Units?.AddRange(initialUnits);
            }
        }

        private List<Unit> GetInitialUnits()
        {
            var randomPlanetName = _map.Map.Systems.OrderBy(x => new Random().Next()).First().Planets.First().Name;

            var unit1 = new Unit
            {
                Planet = randomPlanetName,
                System = _map.Map.Systems.First().Name,
                DestinationSystem = _map.Map.Systems.First().Name,
                Type = "scout",
                Health = 50
            };

            var unit2 = new Unit
            {
                Planet = randomPlanetName,
                System = _map.Map.Systems.First().Name,
                DestinationSystem = _map.Map.Systems.First().Name,
                Type = "builder",
                Health = 50
            };

            return new List<Unit> { unit1, unit2 };
        }

        private Dictionary<string, int> GetInitalResources()
        {
            return new Dictionary<string, int>
                    {
                        {"titanium", 0},
                        {"gold", 0},
                        {"aluminium", 0},
                        {"iron", 10},
                        {"carbon", 20},
                        {"oxygen", 50},
                        {"water", 50}
                    };
        }

        private Dictionary<string, int> GetEmptyResources()
        {
            return new Dictionary<string, int>
                    {
                        {"titanium", 0},
                        {"gold", 0},
                        {"aluminium", 0},
                        {"iron", 0},
                        {"carbon", 0},
                        {"oxygen", 0},
                        {"water", 0}
                    };
        }

    }
}
