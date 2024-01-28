using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json.Serialization;

namespace Duncan.Model
{
    [SwaggerSchema("Unit. Currently, only spaceships of class \"scout\" exists, thought this may change in future versions.")]

    public class Unit
    {
        [SwaggerSchema("Id of the unit")]
        public string Id { get; set; }

        [SwaggerSchema("Type of unit. Right now, only \"scout\" is valid")]
        public string? Type { get; set; }

        [SwaggerSchema("Name of the system the unit currently is.")]
        public string? System { get; set; }

        [SwaggerSchema("Name of the planet the unit currently is orbiting around or landed on.")]
        public string? Planet { get; set; }

        [SwaggerSchema("Name of the system the unit is moving toward.")]
        public string? DestinationSystem { get; set; }

        [SwaggerSchema("Name of the planet the unit is moving toward.")]
        public string? DestinationPlanet { get; set; }

        [SwaggerSchema("Destination shard. Never returned, only set. If set, the unit immediately jumps to the provided Shard, and the server returns the URI of the unit in the Location header.")]
        public string DestinationShard { get; set; }

        [SwaggerSchema("Date at which the unit is expected to arrive.")]
        public DateTime? EstimatedTimeOfArrival { get; set; }
        public int? Health { get; set; }

        [SwaggerSchema("Amount of health points the unit has.")]
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
