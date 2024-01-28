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

        [JsonIgnore]
        public Task? Task { get; set; }
        public Unit()
        {
            Id = Guid.NewGuid().ToString();
        }
    }
}
