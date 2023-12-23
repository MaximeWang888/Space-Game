using Swashbuckle.AspNetCore.Annotations;

namespace Duncan.Model
{
    public class UnitBlueprint
    {
        [SwaggerSchema("Type of unit.")]
        public string? Type { get; set; }
    }
}
