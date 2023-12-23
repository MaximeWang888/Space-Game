using Swashbuckle.AspNetCore.Annotations;

namespace Duncan.Model
{
    public class StarSystem
    {
        [SwaggerSchema("Name of the system. Serves as identifier")]
        public string Name { get; }

        [SwaggerSchema("List of planets contained within the system")]
        public IList<Planet> Planets { get; }
        public StarSystem(string Name, IList<Planet> Planets) {
            this.Name = Name;
            this.Planets = Planets;
        } 
    }
}