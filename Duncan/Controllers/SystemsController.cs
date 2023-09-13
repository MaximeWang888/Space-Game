using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Shard.Shared.Core;

namespace Duncan.Controllers
{
    public class SystemsController : ControllerBase
    {
        [HttpGet("/systems")]

        public IReadOnlyList<SystemSpecification> GetSystems()
        {
            MapGeneratorOptions mapOptions = new MapGeneratorOptions();
            mapOptions.Seed = "MohaMax";
            MapGenerator map = new MapGenerator(mapOptions);
            SectorSpecification systems = map.Generate();
            return systems.Systems;
        }

        [HttpGet("/systems/{systemName}")]
        public SystemSpecification GetOneSystem(string systemName)
        {
            MapGeneratorOptions mapOptions = new MapGeneratorOptions();
            mapOptions.Seed = "MohaMax";
            MapGenerator map = new MapGenerator(mapOptions);
            SectorSpecification systems = map.Generate();
            IReadOnlyList <SystemSpecification> filteredSystem = systems.Systems.Where(system => system.Name == systemName).ToList();
            return filteredSystem[0];
        }
    }
}