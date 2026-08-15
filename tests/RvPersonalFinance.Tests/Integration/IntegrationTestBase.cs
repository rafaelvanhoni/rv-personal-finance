using System.Net;
using System.Net.Http.Json;
using RvPersonalFinance.Api.Features.Accounts;
using RvPersonalFinance.Api.Features.Auth;

namespace RvPersonalFinance.Tests.Integration;

public abstract class IntegrationTestBase : IClassFixture<CustomWebApplicationFactory>
{
    protected readonly HttpClient _client;

    protected IntegrationTestBase(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    protected async Task<(RegisterDto Register, LoginResponseDto Login)> RegisterAndLoginAsync()
    {
        var id = Guid.CreateVersion7();
        var name = $"Usuario {id}";
        var email = $"usuario_{id}@teste.com";
        var password = "12345";

        var requestRegister = new RegisterDto()
        {
            Name = name,
            Email = email,
            Password = password
        };
        var requestLogin = new LoginDto()
        {
            Email = email,
            Password = password
        };        

        var responseRegister = await _client.PostAsJsonAsync("auth/register", requestRegister);
        Assert.Equal(HttpStatusCode.Created, responseRegister.StatusCode);

        var responseLogin = await _client.PostAsJsonAsync("auth/login", requestLogin);
        Assert.Equal(HttpStatusCode.OK, responseLogin.StatusCode);

        var resultLogin = await responseLogin.Content.ReadFromJsonAsync<ResponseEnvelope<LoginResponseDto>>();
        Assert.NotNull(resultLogin);
        Assert.NotNull(resultLogin.Data);
        
        return (requestRegister, resultLogin.Data);
    }

    protected async Task<AccountResponseDto> CreateAccountAsync()
    {
        var dtoAccount = new CreateAccountDto()
        {
            Name = $"Account {Guid.CreateVersion7()}",
            InitialBalance = 0m,
        };    

        var responseAccount = await _client.PostAsJsonAsync("/accounts", dtoAccount);
        Assert.Equal(HttpStatusCode.Created, responseAccount.StatusCode);

        var account = await responseAccount.Content.ReadFromJsonAsync<ResponseEnvelope<AccountResponseDto>>();
        Assert.NotNull(account);
        Assert.NotNull(account.Data);
        
        return account.Data;
    }
}