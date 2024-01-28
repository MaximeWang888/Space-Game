using Swashbuckle.AspNetCore.Annotations;

namespace Duncan.Model
{
    public class Planet
    {
        [SwaggerSchema("Name of a planet. Serves as identifier")]
        public string Name { get; }

        [SwaggerSchema("Size of the planet")]
        public int Size { get; }
        public Planet(string Name, int Size)
        {
            this.Name = Name;
            this.Size = Size;
        }
    }

}

