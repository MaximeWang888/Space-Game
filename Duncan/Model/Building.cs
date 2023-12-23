using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json.Serialization;

namespace Duncan.Model
{
    public class Building
    {
        [SwaggerSchema("Id of the building. Ignored when creating a new building.")]
        public string Id { get; set; }

        [SwaggerSchema("Type of building. Right now, only \"mine\" is valid")]
        public string? Type { get; set; }

        [SwaggerSchema("Kind of resource extracted. Valid only if the type is \"mine\"")]
        public string? ResourceCategory { get; set; }

        [SwaggerSchema("Id of the unit to use to build. Only sent to the server, never returned.")]
        public string? BuilderId { get; set; }

        [SwaggerSchema("Name of the system the building is located on")]
        public string? System { get; set; }

        [SwaggerSchema("Name of the planet the building is located on")]
        public string? Planet { get; set; }

        [SwaggerSchema("True if built; False if building is in progress")]
        public bool? IsBuilt { get; set; }

        [SwaggerSchema("Date at which the building is expected to be ready. Null if built")]
        public DateTime? EstimatedBuildTime { get; set; }

        [JsonIgnore]
        public Task? Task { get; set; }

        [JsonIgnore]
        public Task? TaskTwo { get; set; }

        [JsonIgnore]
        public CancellationTokenSource? CancellationSource { get; set; }
        public Building()
        {
            Id = Guid.NewGuid().ToString();
        }
    }
}
