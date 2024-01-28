using Shard.Shared.Web.IntegrationTests.Asserts;
using Shard.Shared.Web.IntegrationTests.TestEntities;
using System.Net;

namespace Shard.Shared.Web.IntegrationTests;

public partial class BaseIntegrationTests<TEntryPoint, TWebApplicationFactory>
{
    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "5")]
    public async Task BuildingStarportThenFetchingAllBuildingsIncludesStarport()
    {
        using var client = CreateClient();
        var (originalBuilding, _) = await BuildStarport(client);

        var response = await client.GetAsync($"{originalBuilding.UserPath}/buildings");
        await response.AssertSuccessStatusCode();

        var buildings = (await response.AssertSuccessJsonAsync()).AssertArray();
        var building = buildings.AssertSingle().AssertObject();
        Assert.Equal(originalBuilding.ToString(), building.ToString());
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "5")]
    public async Task BuildingStarportThenFetchingBuildingByIdReturnsStarport()
    {
        using var client = CreateClient();
        var (originalBuilding, _) = await BuildStarport(client);
        var building = await RefreshBuilding(client, originalBuilding);

        Assert.Equal(originalBuilding.ToString(), building.ToString());
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "5")]
    public async Task BuildingStarportThenWaiting4MinReturnsUnbuiltStarport()
    {
        using var client = CreateClient();
        var (originalBuilding, _) = await BuildStarport(client);

        await fakeClock.Advance(TimeSpan.FromMinutes(4));
        var building = await RefreshBuilding(client, originalBuilding);

        Assert.False(building.IsBuilt);
        Assert.Equal(fakeClock.Now.AddMinutes(1), building.EstimatedBuildTime);
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "5")]
    public async Task BuildingStarportThenWaiting5MinReturnsBuiltStarport()
    {
        using var client = CreateClient();
        var (originalBuilding, _) = await BuildStarport(client);

        await fakeClock.Advance(TimeSpan.FromMinutes(5));
        var building = await RefreshBuilding(client, originalBuilding);

        Assert.True(building.IsBuilt);
        Assert.Null(building.EstimatedBuildTime);
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "5")]
    public async Task QueuingScoutOnBuiltStarportImmediatlyReturnsOne()
    {
        using var client = CreateClient();
        var originalBuilding = await BuildAndWaitStarportAsync(client);

        var response = await client.PostAsJsonAsync(originalBuilding.QueueUrl, new
        {
            type = "scout"
        });
        await response.AssertSuccessStatusCode();

        var unit = new Unit(originalBuilding.UserPath, await response.AssertSuccessJsonAsync());
        Assert.NotNull(unit.Id);
        Assert.Equal("scout", unit.Type);
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "5")]
    public async Task QueuingScoutOnBuiltStarportCost5Carbon5Iron()
    {
        using var client = CreateClient();
        var originalBuilding = await BuildAndWaitStarportAsync(client);

        await AssertResourceQuantity(client, originalBuilding.UserPath, "carbon", 20);
        await AssertResourceQuantity(client, originalBuilding.UserPath, "iron", 10);

        var response = await client.PostAsJsonAsync(originalBuilding.QueueUrl, new
        {
            type = "scout"
        });
        await response.AssertSuccessStatusCode();
        await AssertResourceQuantity(client, originalBuilding.UserPath, "carbon", 15);
        await AssertResourceQuantity(client, originalBuilding.UserPath, "iron", 5);
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "5")]
    public async Task QueuingBuilderOnBuiltStarportCost5Carbon10Iron()
    {
        using var client = CreateClient();
        var originalBuilding = await BuildAndWaitStarportAsync(client);

        await AssertResourceQuantity(client, originalBuilding.UserPath, "carbon", 20);
        await AssertResourceQuantity(client, originalBuilding.UserPath, "iron", 10);

        var response = await client.PostAsJsonAsync(originalBuilding.QueueUrl, new
        {
            type = "builder"
        });
        await response.AssertSuccessStatusCode();
        await AssertResourceQuantity(client, originalBuilding.UserPath, "carbon", 15);
        await AssertResourceQuantity(client, originalBuilding.UserPath, "iron", 0);
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "5")]
    public async Task QueuingScoutForInvalidUserReturns404()
    {
        using var client = CreateClient();
        var originalBuilding = await BuildAndWaitStarportAsync(client);

        var response = await client.PostAsJsonAsync($"{originalBuilding.UserPath}z/buildings/{originalBuilding.Id}/queue", new
        {
            type = "scout"
        });
        await response.AssertStatusEquals(HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "5")]
    public async Task QueuingScoutForInvalidBuildingReturns404()
    {
        using var client = CreateClient();
        var originalBuilding = await BuildAndWaitStarportAsync(client);

        var response = await client.PostAsJsonAsync($"{originalBuilding.UserPath}/buildings/{originalBuilding.Id}z/queue", new
        {
            type = "scout"
        });
        await response.AssertStatusEquals(HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "5")]
    public async Task QueuingScoutOnMineReturns400()
    {
        using var client = CreateClient();
        var (originalBuilding, _) = await BuildMine(client);

        await fakeClock.Advance(TimeSpan.FromMinutes(5));
        var response = await client.PostAsJsonAsync(originalBuilding.QueueUrl, new
        {
            type = "scout"
        });
        await response.AssertStatusEquals(HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "5")]
    public async Task QueuingScoutOnUnBuiltStarportReturns400()
    {
        using var client = CreateClient();
        var (originalBuilding, _) = await BuildStarport(client);

        var response = await client.PostAsJsonAsync(originalBuilding.QueueUrl, new
        {
            type = "scout"
        });
        await response.AssertStatusEquals(HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "5")]
    public async Task QueuingScoutIfNotEnoughResourcesReturns400()
    {
        using var client = CreateClient();

        var originalBuilding = await BuildAndWaitStarportAsync(client);
        await ChangeUserResources(originalBuilding.UserPath, resoucesQuantity =>
        {
            resoucesQuantity.Carbon = 20;
            resoucesQuantity.Iron = 0;
        });

        var response = await client.PostAsJsonAsync(originalBuilding.QueueUrl, new
        {
            type = "scout"
        });
        await response.AssertStatusEquals(HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "5")]
    public async Task QueuingScoutIfNotEnoughIronDoesNotSpendCarbon()
    {
        using var client = CreateClient();

        var originalBuilding = await BuildAndWaitStarportAsync(client);
        await ChangeUserResources(originalBuilding.UserPath, resoucesQuantity =>
        {
            resoucesQuantity.Carbon = 20;
            resoucesQuantity.Iron = 0;
        });

        var response = await client.PostAsJsonAsync(originalBuilding.QueueUrl, new
        {
            type = "scout"
        });
        await response.AssertStatusEquals(HttpStatusCode.BadRequest);
        await AssertResourceQuantity(client, originalBuilding.UserPath, "carbon", 20);
        await AssertResourceQuantity(client, originalBuilding.UserPath, "iron", 0);
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "5")]
    public async Task QueuingScoutIfNotEnoughCarbonDoesNotSpendIron()
    {
        using var client = CreateClient();

        var originalBuilding = await BuildAndWaitStarportAsync(client);
        await ChangeUserResources(originalBuilding.UserPath, resoucesQuantity =>
        {
            resoucesQuantity.Carbon = 0;
            resoucesQuantity.Iron = 10;
        });

        var response = await client.PostAsJsonAsync(originalBuilding.QueueUrl, new
        {
            type = "scout"
        });
        await response.AssertStatusEquals(HttpStatusCode.BadRequest);
        await AssertResourceQuantity(client, originalBuilding.UserPath, "carbon", 0);
        await AssertResourceQuantity(client, originalBuilding.UserPath, "iron", 10);
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "6")]
    public async Task QueuingCargoOnBuiltStarportCosts10Carbon10Iron5Gold()
    {
        using var client = factory.CreateClient();

        var originalBuilding = await BuildAndWaitStarportAsync(client);
        await ChangeUserResources(originalBuilding.UserPath, resoucesQuantity =>
        {
            resoucesQuantity.Carbon = 10;
            resoucesQuantity.Iron = 10;
            resoucesQuantity.Gold = 5;
        });

        var response = await client.PostAsJsonAsync(originalBuilding.QueueUrl, new
        {
            type = "cargo"
        });
        await response.AssertSuccessStatusCode();
        await AssertResourceQuantity(client, originalBuilding.UserPath, "carbon", 0);
        await AssertResourceQuantity(client, originalBuilding.UserPath, "iron", 0);
        await AssertResourceQuantity(client, originalBuilding.UserPath, "gold", 0);
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "6")]
    public async Task CanLoadResourcesInCargo()
    {
        using var client = factory.CreateClient();
        var originalUnit = await CreateTransportAndLoadScenario(client);

        Assert.NotNull(originalUnit.ResourcesQuantity);
        Assert.Equal(15, originalUnit.ResourcesQuantity.Water);
        Assert.Equal(27, originalUnit.ResourcesQuantity.Oxygen);

        var unit = await RefreshUnit(client, originalUnit);

        Assert.NotNull(unit.ResourcesQuantity);
        Assert.Equal(15, unit.ResourcesQuantity.Water);
        Assert.Equal(27, unit.ResourcesQuantity.Oxygen);
    }

    private async Task<Unit> CreateTransportAndLoadScenario(HttpClient client,
        string unitType = "cargo", int waterLoaded = 15, bool shouldFail = false)
    {
        var originalBuilding = await BuildAndWaitStarportAsync(client);
        await ChangeUserResources(originalBuilding.UserPath, resoucesQuantity =>
        {
            resoucesQuantity.Carbon = 10;
            resoucesQuantity.Iron = 10;
            resoucesQuantity.Gold = 5;
            resoucesQuantity.Water = 25;
            resoucesQuantity.Oxygen = 30;
            resoucesQuantity.Aluminium = 11;
        });

        var queuingResponse = await client.PostAsJsonAsync(originalBuilding.QueueUrl, new
        {
            type = unitType
        });
        await queuingResponse.AssertSuccessStatusCode();
        var unit = new Unit(originalBuilding.UserPath, await queuingResponse.AssertSuccessJsonAsync());

        unit.SetResourcesQuantity(resoucesQuantity =>
        {
            resoucesQuantity.Water = waterLoaded;
            resoucesQuantity.Oxygen = 27;
        });

        if (!shouldFail)
        {
            return await client.PutAsync(unit);
        }
        else
        {
            var loadingResponse = await client.PutTestEntityAsync(unit.Url, unit);
            await loadingResponse.AssertStatusEquals(HttpStatusCode.BadRequest);
            return unit;
        }
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "6")]
    public async Task LoadingResourcesIntoCargoRemovesSaidResources()
    {
        using var client = factory.CreateClient();
        var originalUnit = await CreateTransportAndLoadScenario(client);

        await AssertResourceQuantity(client, originalUnit.UserPath, "water", 10);
        await AssertResourceQuantity(client, originalUnit.UserPath, "oxygen", 3);
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "6")]
    public async Task CannotLoadResourcesInBuilder()
    {
        using var client = factory.CreateClient();
        await CreateTransportAndLoadScenario(client,
            unitType: "builder", shouldFail: true);
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "6")]
    public async Task CanUnloadSomeResourcesFromCargo()
    {
        using var client = factory.CreateClient();
        var originalUnit = await CreateTransportAndLoadScenario(client);

        originalUnit.SetResourcesQuantity(resoucesQuantity =>
        {
            resoucesQuantity.Water = 10;
            resoucesQuantity.Oxygen = 17;
        });
        var unit = await client.PutAsync(originalUnit);

        Assert.NotNull(unit.ResourcesQuantity);
        Assert.Equal(10, unit.ResourcesQuantity.Water);
        Assert.Equal(17, unit.ResourcesQuantity.Oxygen);

        await AssertResourceQuantity(client, originalUnit.UserPath, "water", 15);
        await AssertResourceQuantity(client, originalUnit.UserPath, "oxygen", 13);
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "6")]
    public async Task CanLoadAndUnloadSomeResourcesFromCargoAtTheSameTime()
    {
        using var client = factory.CreateClient();
        var originalUnit = await CreateTransportAndLoadScenario(client);

        originalUnit.SetResourcesQuantity(resoucesQuantity =>
        {
            resoucesQuantity.Water = 5;
            resoucesQuantity.Oxygen = 20;
            resoucesQuantity.Aluminium = 9;
        });
        var unit = await client.PutAsync(originalUnit);

        Assert.NotNull(unit.ResourcesQuantity);
        Assert.Equal(5, unit.ResourcesQuantity.Water);
        Assert.Equal(20, unit.ResourcesQuantity.Oxygen);
        Assert.Equal(9, unit.ResourcesQuantity.Aluminium);

        await AssertResourceQuantity(client, originalUnit.UserPath, "water", 20);
        await AssertResourceQuantity(client, originalUnit.UserPath, "oxygen", 10);
        await AssertResourceQuantity(client, originalUnit.UserPath, "aluminium", 2);
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "6")]
    public async Task CannotLoadMoreResourcesThanUserHas()
    {
        using var client = factory.CreateClient();
        await CreateTransportAndLoadScenario(client,
            waterLoaded: 26, shouldFail: true);
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "6")]
    public async Task CannotLoadResourcesIfNoStarport()
    {
        await CannotLoadOrUnloadResourcesIfNoStarport(resoucesQuantity =>
        {
            resoucesQuantity.Water = 16;
            resoucesQuantity.Oxygen = 27;
        });
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "6")]
    public async Task CannotUnLoadResourcesIfNoStarport()
    {
        await CannotLoadOrUnloadResourcesIfNoStarport(resoucesQuantity =>
        {
            resoucesQuantity.Water = 12;
            resoucesQuantity.Oxygen = 27;
        });
    }

    private async Task CannotLoadOrUnloadResourcesIfNoStarport(Action<ResourcesQuantity> resourceMutator)
    {
        using var client = factory.CreateClient();
        var originalUnit = await CreateTransportAndLoadScenario(client);

        originalUnit.DestinationPlanet = null;
        var unitAfterMove = await client.PutAsync(originalUnit);

        unitAfterMove.SetResourcesQuantity(resourceMutator);
        var loadingResponse = await client.PutTestEntityAsync(unitAfterMove.Url, unitAfterMove);

        await loadingResponse.AssertStatusEquals(HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "6")]
    public async Task CanMoveUnitWithoutUnloadingResources()
    {
        using var client = factory.CreateClient();
        var originalUnit = await CreateTransportAndLoadScenario(client);

        originalUnit.DestinationPlanet = null;
        var unit = await client.PutAsync(originalUnit);

        Assert.NotNull(unit.ResourcesQuantity);
        Assert.Equal(15, unit.ResourcesQuantity.Water);
        Assert.Equal(27, unit.ResourcesQuantity.Oxygen);
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "6")]
    public async Task CanPutCargoWithoutChangingResourcesWithoutStarport()
    {
        using var client = factory.CreateClient();
        var originalUnit = await CreateTransportAndLoadScenario(client);

        originalUnit.DestinationPlanet = null;
        var unitAfterMove = await client.PutAsync(originalUnit);

        var unit = await client.PutAsync(unitAfterMove);

        Assert.NotNull(unit.ResourcesQuantity);
        Assert.Equal(15, unit.ResourcesQuantity.Water);
        Assert.Equal(27, unit.ResourcesQuantity.Oxygen);
    }
}
