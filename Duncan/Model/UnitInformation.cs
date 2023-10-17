using Shard.Shared.Core;

namespace Duncan.Model
{
    public class UnitInformation
    {
        public IReadOnlyDictionary<string, int> ResourcesQuantity { get; set; }
        public string? Planet { get; set; }
        public string? System { get; set; }
    }
}
