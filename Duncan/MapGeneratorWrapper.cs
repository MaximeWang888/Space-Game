using Shard.Shared.Core;

namespace Duncan
{
    public class MapGeneratorWrapper 
    {
        public SectorSpecification Map { get; }

        public MapGeneratorWrapper()
        {
            MapGeneratorOptions mapOptions = new MapGeneratorOptions() { Seed = "Test application" };
            Map = new MapGenerator(mapOptions).Generate();
        }
    }
}
