using Duncan.Model;
using Shard.Shared.Core;

namespace Duncan.Services
{
    public class SystemsService
    {
        public void SystemsTransformation(IList<CustomSystem> CustomSystems, MapGeneratorWrapper map)
        {
            foreach (SystemSpecification system in map.Map.Systems)
            {
                IList<Planet> planets = new List<Planet>();
                CustomSystems.Add(new CustomSystem(system.Name, planets));
                foreach (PlanetSpecification planet in system.Planets)
                {
                    planets.Add(new Planet(planet.Name, planet.Size));
                }
            }

        }
    }
}
