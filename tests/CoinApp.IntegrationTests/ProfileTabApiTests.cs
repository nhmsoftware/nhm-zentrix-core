using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CoinApp.Api.Contracts.Responses;
using CoinApp.Application.Common.Results;
using CoinApp.Application.Dtos.Auth;
using CoinApp.Application.Dtos.Common;
using CoinApp.Application.Dtos.Config;
using CoinApp.Application.Dtos.Profile;
using CoinApp.Application.Dtos.SupportTickets;
using Xunit;

namespace CoinApp.IntegrationTests;

public sealed class ProfileTabApiTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public ProfileTabApiTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ProfileTabEndpoints_WorkEndToEnd()
    {
        var client = _factory.CreateClient();
        var token = await RegisterAndLoginAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var profileResponse = await client.GetAsync("/api/auth/user-profile");
        Assert.Equal(HttpStatusCode.OK, profileResponse.StatusCode);

        var profile = await profileResponse.Content.ReadFromJsonAsync<UserProfileDto>();
        Assert.NotNull(profile);
        Assert.False(string.IsNullOrWhiteSpace(profile!.ReferralCode));
        Assert.Equal("unverified", profile.VerificationStatus);

        var banksResponse = await client.GetAsync("/api/common/banks");
        Assert.Equal(HttpStatusCode.OK, banksResponse.StatusCode);
        Assert.NotNull(await banksResponse.Content.ReadFromJsonAsync<IReadOnlyList<BankDto>>());

        var configResponse = await client.GetAsync("/api/config/list");
        Assert.Equal(HttpStatusCode.OK, configResponse.StatusCode);
        Assert.NotNull(await configResponse.Content.ReadFromJsonAsync<IReadOnlyList<AppConfigDto>>());

        using var verifyContent = CreateVerifyAccountContent();
        var verifyResponse = await client.PostAsync("/api/auth/verify-account", verifyContent);
        Assert.Equal(HttpStatusCode.OK, verifyResponse.StatusCode);

        var verifiedProfile = await verifyResponse.Content.ReadFromJsonAsync<UserProfileDto>();
        Assert.NotNull(verifiedProfile);
        Assert.Equal("waiting", verifiedProfile!.VerificationStatus);
        Assert.NotNull(verifiedProfile.Bank);
        Assert.Equal("970436", verifiedProfile.Bank!.BinBank);

        var withdrawResponse = await client.PostAsJsonAsync("/api/wallet/withdraw", new { money = 100000m });
        Assert.Equal(HttpStatusCode.BadRequest, withdrawResponse.StatusCode);

        var withdrawError = await withdrawResponse.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.NotNull(withdrawError);
        Assert.Equal("wallet.insufficient_balance", withdrawError!.ErrorCode);

        var createTicketResponse = await client.PostAsJsonAsync("/api/tickets", new CreateSupportTicketRequest
        {
            Type = 1,
            Priority = 2,
            Message = "Need help with profile verification"
        });

        Assert.Equal(HttpStatusCode.OK, createTicketResponse.StatusCode);

        var ticket = await createTicketResponse.Content.ReadFromJsonAsync<SupportTicketDto>();
        Assert.NotNull(ticket);
        Assert.Equal("open", ticket!.Status);

        var ticketsResponse = await client.GetAsync("/api/tickets?page=1&status=1");
        Assert.Equal(HttpStatusCode.OK, ticketsResponse.StatusCode);

        var tickets = await ticketsResponse.Content.ReadFromJsonAsync<PaginatedResult<SupportTicketDto>>();
        Assert.NotNull(tickets);
        Assert.Single(tickets!.Items);

        var threadResponse = await client.GetAsync($"/api/tickets/{ticket.Id}?page=1");
        Assert.Equal(HttpStatusCode.OK, threadResponse.StatusCode);

        var thread = await threadResponse.Content.ReadFromJsonAsync<SupportTicketThreadDto>();
        Assert.NotNull(thread);
        Assert.Single(thread!.Messages);

        var replyResponse = await client.PostAsJsonAsync($"/api/tickets/{ticket.Id}/reply", new ReplySupportTicketRequest
        {
            Message = "Adding more information"
        });

        Assert.Equal(HttpStatusCode.OK, replyResponse.StatusCode);
    }

    private static MultipartFormDataContent CreateVerifyAccountContent()
    {
        var content = new MultipartFormDataContent
        {
            { new StringContent("Alice"), "first_name" },
            { new StringContent("Example"), "last_name" },
            { new StringContent("1990-01-01"), "dob" },
            { new StringContent("female"), "gender" },
            { new StringContent("0900000000"), "phone_number" },
            { new StringContent("Ho Chi Minh City"), "address" },
            { new StringContent("970436"), "bin_bank" },
            { new StringContent("123456789"), "account_bank" },
            { new StringContent("ALICE EXAMPLE"), "account_bank_name" }
        };

        content.Add(CreatePngContent(), "cccd_front_image", "front.png");
        content.Add(CreatePngContent(), "cccd_back_image", "back.png");

        return content;
    }

    private static ByteArrayContent CreatePngContent()
    {
        var file = new ByteArrayContent(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });
        file.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        return file;
    }

    private static async Task<string> RegisterAndLoginAsync(HttpClient client)
    {
        var email = $"profile-{Guid.NewGuid():N}@example.com";
        const string password = "Password123!";

        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            FullName = "Alice Example",
            Email = email,
            Password = password
        });

        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = email,
            Password = password
        });

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var loginPayload = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotNull(loginPayload);
        return loginPayload!.AccessToken;
    }
}
