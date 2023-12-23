using Swashbuckle.AspNetCore.Annotations;

namespace Duncan.Model
{
    public class UnitLocation
    {
        [SwaggerSchema("Name of the system the unit currently is")]
        public string? System { get; set; }

        [SwaggerSchema("Name of the planet the unit currently is")]
        public string? Planet { get; set; }
        public IReadOnlyDictionary<string, int>? ResourcesQuantity { get; set; }
    }
}
