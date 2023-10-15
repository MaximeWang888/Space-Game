using Duncan.Interfaces;
using Shard.Shared.Core;

namespace Duncan
{
    public class MapGeneratorWrapper : IMapGenerator
    {
        private readonly SectorSpecification map;

        public MapGeneratorWrapper()
        {
            MapGeneratorOptions mapOptions = new MapGeneratorOptions() { Seed = "MohaMax" };
            map = new MapGenerator(mapOptions).Generate();
        }

        public SectorSpecification getGenerator()
        {
            return map;
        }
    }
}
