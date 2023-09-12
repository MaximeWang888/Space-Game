using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Shard.Shared.Web.IntegrationTests;
using Xunit.Abstractions;

namespace DuncanShard.IntegrationTests
{
    public class IntegrationTests : BaseIntegrationTests<Program>
    {
        public IntegrationTests(WebApplicationFactory<Program> factory, ITestOutputHelper testOutputHelper) : base(factory, testOutputHelper)
        {
            
        }
            
    }
}