using Microsoft.AspNetCore.Mvc.Testing;
using Shard.Duncan;
using Shard.Shared.Web.IntegrationTests;
using Xunit.Abstractions;

namespace Duncan.IntegrationTest
{
    public class IntegrationTests : BaseIntegrationTests<Program>
    {
        public IntegrationTests(WebApplicationFactory<Program> factory, ITestOutputHelper testOutputHelper)
            : base(factory, testOutputHelper) { }
    }
}