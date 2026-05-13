using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CoinApp.IntegrationTests;

public sealed class ApiSmokeTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public ApiSmokeTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public void Factory_CanBeCreated()
    {
        Assert.NotNull(_factory);
    }
}
