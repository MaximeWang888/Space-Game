using Shard.Shared.Core;

namespace Duncan
{
    public class MapGeneratorWrapper 
    {
        public SectorSpecification Map { get; }

        public MapGeneratorWrapper()
        {
            MapGeneratorOptions mapOptions = new MapGeneratorOptions() { Seed = "MohaMax" };
            Map = new MapGenerator(mapOptions).Generate();
        }
    }
}
