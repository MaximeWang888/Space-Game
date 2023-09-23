using System.Runtime.InteropServices.JavaScript;
using Microsoft.AspNetCore.Mvc;
using Shard.Shared.Core;

namespace DuncanShard.Controllers;

[ApiController]
[Route("[controller]")]
public class SystemsController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<SystemsController> _logger;

    public SystemsController(ILogger<SystemsController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
    }

    [HttpGet("/")]
    public SectorSpecification GetSystems()
    {
        MapGenerator mapGenerator = new MapGenerator(new MapGeneratorOptions(){Seed = "Mohamax"});
        return mapGenerator.Generate();
    }
}