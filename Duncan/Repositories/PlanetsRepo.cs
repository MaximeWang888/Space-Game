using Duncan.Model;
using Shard.Shared.Core;

namespace Duncan.Repositories
{
    public class PlanetsRepo
    {
        public Planet? FindPlanetByName(string planetName, IList<Planet> planetsSelected)
        {
            return planetsSelected.FirstOrDefault(planet => planet.Name == planetName);
        }

        public PlanetSpecification? GetPlanetByName(string? planetName, SystemSpecification system)
        {
            return system.Planets.FirstOrDefault(p => p.Name == planetName);
        }
    }
}
