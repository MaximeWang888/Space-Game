using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json.Serialization;

namespace Duncan.Model
{
    public class User
    {
        [SwaggerSchema("Id of the user")]
        public string? Id { get; set; }

        [SwaggerSchema("Name displayed")]
        public string? Pseudo { get; set; }

        [SwaggerSchema("Date at which the user was created originally, with time zone")]
        public DateTime? DateOfCreation { get; set; }

        public IDictionary<string, int>? ResourcesQuantity { get; set; }

        [JsonIgnore]
        public List<Unit>? Units { get; } = new List<Unit>();
        public List<Building>? Buildings { get; set; } = new List<Building>();
    }
}
