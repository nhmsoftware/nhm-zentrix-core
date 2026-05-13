using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CoinApp.Api.Auth;
using CoinApp.Api.Contracts.Responses;
using CoinApp.Application.Dtos.Auth;
using CoinApp.Domain.Entities;
using CoinApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CoinApp.IntegrationTests;

public sealed class AuthApiTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public AuthApiTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Register_Login_Me_Flow_Works_EndToEnd()
    {
        var client = CreateClient();
        var email = $"alice-{Guid.NewGuid():N}@example.com";
        const string password = "Password123!";

        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            FullName = "Alice Example",
            Email = email.ToUpperInvariant(),
            Password = password
        });

        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        var registerPayload = await registerResponse.Content.ReadFromJsonAsync<AuthResponseDto>();

        Assert.NotNull(registerPayload);
        Assert.False(string.IsNullOrWhiteSpace(registerPayload!.AccessToken));
        Assert.Equal(email, registerPayload.User.Email);

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = email.ToUpperInvariant(),
            Password = password
        });

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var loginPayload = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();

        Assert.NotNull(loginPayload);
        Assert.False(string.IsNullOrWhiteSpace(loginPayload!.AccessToken));

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginPayload.AccessToken);

        var meResponse = await client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);

        var mePayload = await meResponse.Content.ReadFromJsonAsync<AuthUserDto>();

        Assert.NotNull(mePayload);
        Assert.Equal(loginPayload.User.Id, mePayload!.Id);
        Assert.Equal(email, mePayload.Email);
    }

    [Fact]
    public async Task Register_ReturnsConflict_WhenEmailAlreadyExists()
    {
        var client = CreateClient();
        var email = $"bob-{Guid.NewGuid():N}@example.com";

        await SeedUserAsync("Bob Example", email, "Password123!");

        var response = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            FullName = "Bob Example",
            Email = email,
            Password = "Password123!"
        });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();

        Assert.NotNull(error);
        Assert.Equal("auth.email_already_exists", error!.ErrorCode);
    }

    private HttpClient CreateClient()
    {
        return _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
    }

    private async Task SeedUserAsync(string fullName, string email, string password)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var user = new User
        {
            FullName = fullName,
            Email = email.ToLowerInvariant(),
            IsActive = true
        };

        user.PasswordHash = new PasswordHashService().HashPassword(user, password);

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
    }
}
