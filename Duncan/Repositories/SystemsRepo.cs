using Duncan.Model;
using Shard.Shared.Core;

namespace Duncan.Repositories
{
    public class SystemsRepo
    {
        public StarSystem? FindSystemByName(string systemName, IList<StarSystem> systems)
        {
            return systems.FirstOrDefault(system => system.Name == systemName);
        }

        public SystemSpecification? GetSystemByName(string? systemName, IReadOnlyList<SystemSpecification> systems)
        {
            return systems.FirstOrDefault(system => system.Name == systemName);
        }
    }
}
