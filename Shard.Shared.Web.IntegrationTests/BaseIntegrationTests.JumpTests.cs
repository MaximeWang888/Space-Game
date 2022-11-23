using Microsoft.AspNetCore.Mvc.Testing;
using Shard.Shared.Web.IntegrationTests.Asserts;
using Shard.Shared.Web.IntegrationTests.TestEntities;
using System.Net;

namespace Shard.Shared.Web.IntegrationTests;

public partial class BaseIntegrationTests<TEntryPoint, TWebApplicationFactory>
{
    private readonly string jumpingUserId = Guid.NewGuid().ToString();
    private readonly string jumpingUnitId = Guid.NewGuid().ToString();
    private readonly DateTimeOffset jumpingUserDateOfCreation = DateTimeOffset.Now.AddYears(-5);

    private async Task<User> ReceivingNewUser_BaseScenario()
    {
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = CreateShardAuthorizationHeader(
            "fake-remote", "caramba");

        var response = await client.PutAsJsonAsync($"users/{jumpingUserId}", new
        {
            id = jumpingUserId,
            pseudo = "remote.user",
            dateOfCreation = jumpingUserDateOfCreation
        });

        return new User(await response.AssertSuccessJsonAsync());
    }

    private async Task<Unit> ReceivingJumpingCargo_BaseScenario()
    {
        var user = await ReceivingNewUser_BaseScenario();

        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = CreateShardAuthorizationHeader(
            "fake-remote", "caramba");

        var response = await client.PutAsJsonAsync($"{user.Url}/units/{jumpingUnitId}", new
        {
            id = jumpingUnitId,
            type = "cargo",
            health = 5,
            resourcesQuantity = new
            {
                water = 12,
                oxygen = 16
            }
        });

        return new Unit(user.Url, await response.AssertSuccessJsonAsync());
    }
    private async Task<Unit> ReceivingJumpingCargo_WithAdditionalFetch()
    {
        var unit = await ReceivingJumpingCargo_BaseScenario();

        using var client = factory.CreateClient();

        using var unitsResponse = await client.GetAsync(unit.UserPath + "/units");
        var json = await unitsResponse.AssertSuccessJsonAsync();

        return new Unit(unit.UserPath, json.AssertArray().AssertSingle());
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "6")]
    public async Task ReceivingNewUser_Works()
    {
        using var client = factory.CreateClient();
        await ReceivingNewUser_BaseScenario();
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "6")]
    public async Task ReceivingNewUser_CreatesUserWithCorrectInfo()
    {
        using var client = factory.CreateClient();
        var user = await ReceivingNewUser_BaseScenario();

        Assert.Equal(jumpingUserId, user.Id);
        Assert.Equal("remote.user", user.Pseudo);
        Assert.Equal(jumpingUserDateOfCreation, user.DateOfCreation);

        var user2 = await GetUser(user.Url, client);
        Assert.Equal(user.ToString(), user2.ToString());
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "6")]
    public async Task ReceivingNewUser_CreatesUserWithNoUnitNorResources()
    {
        using var client = factory.CreateClient();
        var user = await ReceivingNewUser_BaseScenario();

        var resourceKinds = new[]
        {
                "carbon",
                "iron",
                "gold",
                "aluminium",
                "titanium",
                "water",
                "oxygen",
            };

        foreach (var resourceKind in resourceKinds)
            Assert.Equal(0, user.ResourcesQuantity[resourceKind]);

        using var unitsResponse = await client.GetAsync(user.Url + "/units");
        var json = await unitsResponse.AssertSuccessJsonAsync();
        json.AssertArray().AssertEmpty();
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "6")]
    public async Task ReceivingJumpingCargo_Works()
    {
        using var client = factory.CreateClient();
        await ReceivingJumpingCargo_BaseScenario();
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "6")]
    public async Task ReceivingJumpingCargo_UnitIsSoleUnitOfUser()
    {
        var fetchedUnit = await ReceivingJumpingCargo_WithAdditionalFetch();
        Assert.Equal(jumpingUnitId, fetchedUnit.Id);
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "6")]
    public async Task ReceivingJumpingCargo_UnitLandInExpectedSystem()
    {
        var fetchedUnit = await ReceivingJumpingCargo_WithAdditionalFetch();
        Assert.Equal(jumpingUnitId, fetchedUnit.Id);
        Assert.Equal("80ad7191-ef3c-14f0-7be8-e875dad4cfa6", fetchedUnit.System);
        Assert.Null(fetchedUnit.Planet);
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "6")]
    public async Task ReceivingJumpingCargo_UnitKeepsResourcesAndHealthPoints()
    {
        var fetchedUnit = await ReceivingJumpingCargo_WithAdditionalFetch();
        Assert.Equal(5, fetchedUnit.Health);
        Assert.NotNull(fetchedUnit.ResourcesQuantity);
        Assert.Equal(12, fetchedUnit.ResourcesQuantity.Water);
        Assert.Equal(16, fetchedUnit.ResourcesQuantity.Oxygen);
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "6")]
    public async Task JumpingCargo_RedirectsToTheUri()
    {
        var (response, _, unitEntry) = await JumpingCargo_StandardScenario();

        await response.AssertStatusEquals(HttpStatusCode.PermanentRedirect);
        Assert.Equal(unitEntry.ExpectedUri, response.Headers.Location?.ToString());
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "6")]
    public async Task JumpingCargo_SendsUserDetails()
    {
        var (_, userEntry, _) = await JumpingCargo_StandardScenario();

        Assert.NotNull(userEntry.ActualRequestContent);
        var userBody = new User(await userEntry.ActualRequestContent.AssertJsonAsync());

        Assert.Equal(userEntry.ExpectedUri.Split('/').Last(), userBody.Id);
        Assert.Equal("johny", userBody.Pseudo);
        userBody.Json["dateOfCreation"].AssertDateTime();
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "6")]
    public async Task JumpingCargo_SendsUnitDetails()
    {
        var (_, _, unitEntry) = await JumpingCargo_StandardScenario();

        Assert.NotNull(unitEntry.ActualRequestContent);
        var unitBody = new Unit($"users/{jumpingUserId}/units", await unitEntry.ActualRequestContent.AssertJsonAsync());

        Assert.Equal(unitEntry.ExpectedUri.Split('/').Last(), unitBody.Id);
        Assert.Equal("cargo", unitBody.Type);
        Assert.Equal(100, unitBody.Health);
        Assert.NotNull(unitBody.ResourcesQuantity);
        Assert.Equal(15, unitBody.ResourcesQuantity.Water);
        Assert.Equal(27, unitBody.ResourcesQuantity.Oxygen);
    }

    private async Task<(HttpResponseMessage response, FakeHttpHandler.Entry userEntry, FakeHttpHandler.Entry unitEntry)>
        JumpingCargo_StandardScenario()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions()
        {
            AllowAutoRedirect = false
        });
        var unit = await CreateTransportReadyToJump(client);

        var userEntry = httpHandler.AddHandler(HttpMethod.Put, "http://10.0.0.42/" + unit.UserPath, HttpStatusCode.OK, null);
        var unitEntry = httpHandler.AddHandler(HttpMethod.Put, "http://10.0.0.42/" + unit.Url, HttpStatusCode.OK, null);

        unit.DestinationShard = "fake-remote";
        var jumpingResponse = await client.PutTestEntityAsync(unit.Url, unit);
        return (jumpingResponse, userEntry, unitEntry);
    }

    private async Task<Unit> CreateTransportReadyToJump(HttpClient client)
    {
        var unit = await CreateTransportAndLoadScenario(client);
        unit.DestinationSystem = "80ad7191-ef3c-14f0-7be8-e875dad4cfa6";
        unit.DestinationPlanet = null;
        await client.PutAsync(unit);

        await fakeClock.Advance(TimeSpan.FromHours(1));
        return await RefreshUnit(client, unit);
    }
}
