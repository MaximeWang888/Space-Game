using Microsoft.AspNetCore.Hosting; 
using Microsoft.AspNetCore.Mvc.Testing; 
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration; 
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging; 
using Shard.Shared.Core;
using Shard.Shared.Web.IntegrationTests.Clock;
using Xunit.Abstractions;
using Microsoft.AspNetCore.Mvc.Testing.Handlers;
using Microsoft.Extensions.Http;

namespace Shard.Shared.Web.IntegrationTests; 

public abstract partial class BaseIntegrationTests<TEntryPoint, TWebApplicationFactory> 
    : IClassFixture<TWebApplicationFactory>, IHttpMessageHandlerBuilderFilter
    where TEntryPoint : class 
    where TWebApplicationFactory: WebApplicationFactory<TEntryPoint> 
{ 
    private readonly WebApplicationFactory<TEntryPoint> factory; 
	private readonly FakeClock fakeClock = new();
    private readonly FakeHttpHandler httpHandler = new();

    public BaseIntegrationTests(TWebApplicationFactory factory, ITestOutputHelper testOutputHelper) 
    {
        this.factory = factory 
            .WithWebHostBuilder(builder => 
            { 
                builder.ConfigureAppConfiguration(RemoveAllReloadOnChange); 
                builder.ConfigureLogging( 
                    logging => logging.AddProvider(new XunitLoggerProvider(testOutputHelper)));

                builder.ConfigureAppConfiguration(config =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string>()
                        {
                            { "Wormholes:fake-remote:baseUri", "http://10.0.0.42" },
                            { "Wormholes:fake-remote:system", "80ad7191-ef3c-14f0-7be8-e875dad4cfa6" },
                            { "Wormholes:fake-remote:user", "server1" },
                            { "Wormholes:fake-remote:sharedPassword", "caramba" },
                        });
                });

                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton<IClock>(fakeClock);	
                    services.AddSingleton<IStartupFilter>(fakeClock);
                    services.Configure<MapGeneratorOptions>(options =>
                    {
                        options.Seed = "Test application";
                    });
                    services.AddSingleton<IHttpMessageHandlerBuilderFilter>(this);
                });
            }); 
    } 

    private void RemoveAllReloadOnChange(WebHostBuilderContext context, IConfigurationBuilder configuration) 
    { 
        foreach (var source in configuration.Sources.OfType<FileConfigurationSource>()) 
            source.ReloadOnChange = false;
    }

    public Action<HttpMessageHandlerBuilder> Configure(Action<HttpMessageHandlerBuilder> next)
    {
        return builder =>
        {
            builder.AdditionalHandlers.Add(httpHandler);
            next(builder);
        };
    }

    private HttpClient CreateClient()
    {
        var client = factory.CreateDefaultClient(
            factory.ClientOptions.BaseAddress,
            new RedirectHandler(),
            new CookieContainerHandler(),
            new TimeoutHandler());

        client.SetTimeoutIfNotDebug(TimeSpan.FromSeconds(3));

        return client;
    }
}

public abstract class BaseIntegrationTests<TEntryPoint>: BaseIntegrationTests<TEntryPoint, WebApplicationFactory<TEntryPoint>>
    where TEntryPoint : class
{
    public BaseIntegrationTests(WebApplicationFactory<TEntryPoint> factory, ITestOutputHelper testOutputHelper)
        : base(factory, testOutputHelper)
    { }
}
