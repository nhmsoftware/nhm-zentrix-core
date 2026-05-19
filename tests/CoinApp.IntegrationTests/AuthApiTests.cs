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

        var registerPayload = await ReadDataAsync<RegisterResponseDto>(registerResponse);

        Assert.Equal(email, registerPayload.User.Email);
        Assert.True(registerPayload.EmailVerificationRequired);
        Assert.False(string.IsNullOrWhiteSpace(registerPayload.EmailVerificationCode));

        var loginBeforeVerifyResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = email.ToUpperInvariant(),
            Password = password
        });

        Assert.Equal(HttpStatusCode.Forbidden, loginBeforeVerifyResponse.StatusCode);

        var verifyEmailResponse = await client.PostAsJsonAsync("/api/auth/verify-email", new VerifyEmailRequest
        {
            Email = email.ToUpperInvariant(),
            Code = registerPayload.EmailVerificationCode!
        });

        Assert.Equal(HttpStatusCode.OK, verifyEmailResponse.StatusCode);

        var verifyEmailPayload = await ReadDataAsync<EmailVerificationResponseDto>(verifyEmailResponse);

        Assert.True(verifyEmailPayload.EmailVerified);
        Assert.NotNull(verifyEmailPayload.EmailVerifiedAtUtc);

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = email.ToUpperInvariant(),
            Password = password
        });

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var loginPayload = await ReadDataAsync<AuthResponseDto>(loginResponse);

        Assert.False(string.IsNullOrWhiteSpace(loginPayload.AccessToken));

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginPayload.AccessToken);

        var meResponse = await client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);

        var mePayload = await ReadDataAsync<AuthUserDto>(meResponse);

        Assert.Equal(loginPayload.User.Id, mePayload.Id);
        Assert.Equal(email, mePayload.Email);

        var logoutResponse = await client.PostAsync("/api/auth/logout", content: null);

        Assert.Equal(HttpStatusCode.OK, logoutResponse.StatusCode);

        var meAfterLogoutResponse = await client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, meAfterLogoutResponse.StatusCode);

        var meAfterLogoutError = await meAfterLogoutResponse.Content.ReadFromJsonAsync<ApiErrorResponse>();

        Assert.NotNull(meAfterLogoutError);
        Assert.Equal("Chưa xác thực hoặc phiên đăng nhập đã hết hạn.", meAfterLogoutError!.Message);
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
        Assert.Equal("Email đã tồn tại.", error!.Message);
    }

    [Fact]
    public async Task ForgotPassword_VerifyResetCode_ResetPassword_WorksEndToEnd()
    {
        var client = CreateClient();
        var email = $"carol-{Guid.NewGuid():N}@example.com";
        const string oldPassword = "OldPassword123!";
        const string newPassword = "NewPassword123!";

        await SeedUserAsync("Carol Example", email, oldPassword, emailVerified: true);

        var loginBeforeResetResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = email,
            Password = oldPassword
        });

        Assert.Equal(HttpStatusCode.OK, loginBeforeResetResponse.StatusCode);

        var loginBeforeResetPayload = await ReadDataAsync<AuthResponseDto>(loginBeforeResetResponse);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginBeforeResetPayload.AccessToken);

        var meBeforeResetResponse = await client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.OK, meBeforeResetResponse.StatusCode);

        client.DefaultRequestHeaders.Authorization = null;

        var forgotPasswordResponse = await client.PostAsJsonAsync("/api/auth/forgot-password", new ForgotPasswordRequest
        {
            Email = email.ToUpperInvariant()
        });

        Assert.Equal(HttpStatusCode.OK, forgotPasswordResponse.StatusCode);

        var forgotPasswordPayload = await ReadDataAsync<ForgotPasswordResponseDto>(forgotPasswordResponse);

        Assert.Equal(email, forgotPasswordPayload.Email);
        Assert.False(string.IsNullOrWhiteSpace(forgotPasswordPayload.ResetCode));
        Assert.NotNull(forgotPasswordPayload.ResetCodeExpiresAtUtc);

        var verifyResetCodeResponse = await client.PostAsJsonAsync("/api/auth/verify-reset-code", new VerifyResetCodeRequest
        {
            Email = email.ToUpperInvariant(),
            Code = forgotPasswordPayload.ResetCode!
        });

        Assert.Equal(HttpStatusCode.OK, verifyResetCodeResponse.StatusCode);

        var verifyResetCodePayload = await ReadDataAsync<VerifyResetCodeResponseDto>(verifyResetCodeResponse);

        Assert.Equal(email, verifyResetCodePayload.Email);
        Assert.False(string.IsNullOrWhiteSpace(verifyResetCodePayload.ResetToken));

        var resetPasswordResponse = await client.PostAsJsonAsync("/api/auth/reset-password", new ResetPasswordRequest
        {
            ResetToken = verifyResetCodePayload.ResetToken,
            Password = newPassword
        });

        Assert.Equal(HttpStatusCode.OK, resetPasswordResponse.StatusCode);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginBeforeResetPayload.AccessToken);

        var meAfterResetResponse = await client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, meAfterResetResponse.StatusCode);

        client.DefaultRequestHeaders.Authorization = null;

        var oldPasswordLoginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = email,
            Password = oldPassword
        });

        Assert.Equal(HttpStatusCode.Unauthorized, oldPasswordLoginResponse.StatusCode);

        var newPasswordLoginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = email,
            Password = newPassword
        });

        Assert.Equal(HttpStatusCode.OK, newPasswordLoginResponse.StatusCode);
    }

    [Fact]
    public async Task ChangePassword_RevokesOldToken_AndAllowsLoginWithNewPassword()
    {
        var client = CreateClient();
        var email = $"david-{Guid.NewGuid():N}@example.com";
        const string currentPassword = "CurrentPassword123!";
        const string newPassword = "ChangedPassword123!";

        await SeedUserAsync("David Example", email, currentPassword, emailVerified: true);

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = email,
            Password = currentPassword
        });

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var loginPayload = await ReadDataAsync<AuthResponseDto>(loginResponse);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginPayload.AccessToken);

        var changePasswordResponse = await client.PostAsJsonAsync("/api/auth/change-password", new ChangePasswordRequest
        {
            CurrentPassword = currentPassword,
            NewPassword = newPassword
        });

        Assert.Equal(HttpStatusCode.OK, changePasswordResponse.StatusCode);

        var changePasswordPayload = await ReadDataAsync<ChangePasswordResponseDto>(changePasswordResponse);

        Assert.True(changePasswordPayload.Success);

        var meWithOldTokenResponse = await client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, meWithOldTokenResponse.StatusCode);

        client.DefaultRequestHeaders.Authorization = null;

        var oldPasswordLoginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = email,
            Password = currentPassword
        });

        Assert.Equal(HttpStatusCode.Unauthorized, oldPasswordLoginResponse.StatusCode);

        var newPasswordLoginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = email,
            Password = newPassword
        });

        Assert.Equal(HttpStatusCode.OK, newPasswordLoginResponse.StatusCode);
    }

    private static async Task<T> ReadDataAsync<T>(HttpResponseMessage response)
    {
        var payload = await response.Content.ReadFromJsonAsync<ApiSuccessResponse<T>>();

        Assert.NotNull(payload);
        Assert.NotNull(payload!.Data);

        return payload.Data;
    }

    private HttpClient CreateClient()
    {
        return _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
    }

    private async Task SeedUserAsync(string fullName, string email, string password, bool emailVerified = false)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var user = new User
        {
            FullName = fullName,
            Email = email.ToLowerInvariant(),
            IsActive = true,
            EmailVerifiedAtUtc = emailVerified ? DateTime.UtcNow : null
        };

        user.PasswordHash = new PasswordHashService().HashPassword(user, password);

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
    }
}
