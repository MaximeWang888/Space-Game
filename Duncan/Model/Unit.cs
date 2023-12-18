using System.Text.Json.Serialization;

namespace Duncan.Model
{
    public class Unit
    {
        public string Id { get; set; }
        public string? Type { get; set; }
        public string? System { get; set; }
        public string? Planet { get; set; }
        public string? DestinationSystem { get; set; }
        public string? DestinationPlanet { get; set; }
        public DateTime? EstimatedTimeOfArrival { get; set; }
        public int? Health { get; set; }
        public IDictionary<string, int>? ResourcesQuantity { get; set; }

        [JsonIgnore]
        public Task? Task { get; set; }
        public Unit()
        {
            Id = Guid.NewGuid().ToString();
            ResourcesQuantity = new Dictionary<string, int>
        {
            { "carbon", 0 },
            { "iron", 0 },
            { "gold", 0 },
            { "aluminium", 0 },
            { "titanium", 0 },
            { "water", 0 },
            { "oxygen", 0 }
        };
        }
    }
}
