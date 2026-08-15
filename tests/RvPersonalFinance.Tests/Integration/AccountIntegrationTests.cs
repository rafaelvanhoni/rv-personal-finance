using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using RvPersonalFinance.Api.Features.Accounts;

namespace RvPersonalFinance.Tests.Integration;

public class AccountIntegrationTests : IntegrationTestBase
{

    public AccountIntegrationTests(CustomWebApplicationFactory factory) : base(factory)
    {
        
    }

    [Fact]
    public async Task GetAllAccounts_WhenAuthenticated_ShouldReturnOnlyUserAccounts()
    {
        // Given
        var userA = await RegisterAndLoginAsync();
        var userB = await RegisterAndLoginAsync();

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userA.Login.Token);
        var dtoAccountA1 = new CreateAccountDto()
        {
            Name = userA.Register.Name,
            InitialBalance = 0m,
        };

        var dtoAccountA2 = new CreateAccountDto()
        {
            Name = userA.Register.Name,
            InitialBalance = 100m,
        };

        var responseAccountA1 = await _client.PostAsJsonAsync("/accounts", dtoAccountA1);
        Assert.Equal(HttpStatusCode.Created, responseAccountA1.StatusCode);

        var accountA1 = await responseAccountA1.Content.ReadFromJsonAsync<ResponseEnvelope<AccountResponseDto>>();
        Assert.NotNull(accountA1);
        Assert.NotNull(accountA1.Data);

        var responseAccountA2 = await _client.PostAsJsonAsync("/accounts", dtoAccountA2);
        Assert.Equal(HttpStatusCode.Created, responseAccountA2.StatusCode);

        var accountA2 = await responseAccountA2.Content.ReadFromJsonAsync<ResponseEnvelope<AccountResponseDto>>();
        Assert.NotNull(accountA2);
        Assert.NotNull(accountA2.Data);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userB.Login.Token);
        var dtoAccountB = new CreateAccountDto()
        {
            Name = userB.Register.Name,
            InitialBalance = 10m,
        };

        var responseAccountB = await _client.PostAsJsonAsync("/accounts", dtoAccountB);
        Assert.Equal(HttpStatusCode.Created, responseAccountB.StatusCode);

        var accountB = await responseAccountB.Content.ReadFromJsonAsync<ResponseEnvelope<AccountResponseDto>>();
        Assert.NotNull(accountB);
        Assert.NotNull(accountB.Data);


        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userA.Login.Token);

        // When
        var responseAccounts = await _client.GetAsync("/accounts");
         
        // Then
        Assert.Equal(HttpStatusCode.OK, responseAccounts.StatusCode);

        var accounts = await responseAccounts.Content.ReadFromJsonAsync<ResponseEnvelope<IEnumerable<AccountResponseDto>>>();
        Assert.NotNull(accounts);
        Assert.NotNull(accounts.Data);

        Assert.Equal(2, accounts.Data.Count());
        Assert.Contains(accounts.Data, a => a.Id == accountA1.Data.Id);
        Assert.Contains(accounts.Data, a => a.Id == accountA2.Data.Id);
        Assert.DoesNotContain(accounts.Data, a => a.Id == accountB.Data.Id);

    }
}

