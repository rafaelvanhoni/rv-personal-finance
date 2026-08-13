using System.Net;
using System.Net.Http.Json;
using RvPersonalFinance.Api.Features.Auth;

namespace RvPersonalFinance.Tests.Integration;

public class AuthIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_WhenDataIsValid_ShouldCreateUser()
    {
        // Given
        var request = new RegisterDto()
        {
            Name = "Rafael",
            Email = "rafael@teste.com",
            Password = "123456",
        };

        // When
        var response = await _client.PostAsJsonAsync("auth/register", request);

        // Then
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }    

    [Fact]
    public async Task Register_WhenEmailAlreadyExists_ShouldReturnConflict()
    {
        // Given

        var requestA = new RegisterDto()
        {
            Name = "Rafael",
            Email = "rafael@teste2.com",
            Password = "123456",
        };

        var requestB = new RegisterDto()
        {
            Name = "BBB",
            Email = "rafael@teste2.com",
            Password = "654321",            
        };
    
        // When
        var responseA = await _client.PostAsJsonAsync("auth/register", requestA);
        var responseB = await _client.PostAsJsonAsync("auth/register", requestB);
    
        // Then
        Assert.Equal(HttpStatusCode.Created, responseA.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, responseB.StatusCode);
    }

    [Fact]
    public async Task Login_WhenCredentialsAreValid_ShouldReturnToken()
    {
        // Given
        var requestRegister = new RegisterDto()
        {
            Name = "Joao",
            Email = "joao@teste.com",
            Password = "1234"
        };
        var requestLogin = new LoginDto()
        {
            Email = "joao@teste.com",
            Password = "1234"
        };

        var responseRegister = await _client.PostAsJsonAsync("auth/register", requestRegister);
        Assert.Equal(HttpStatusCode.Created, responseRegister.StatusCode);

        // When
        var responseLogin = await _client.PostAsJsonAsync("auth/login", requestLogin);
        var loginResult = await responseLogin.Content.ReadFromJsonAsync<LoginResponseEnvelope>();


        // Then
        Assert.Equal(HttpStatusCode.OK, responseLogin.StatusCode);
        Assert.NotNull(loginResult);
        Assert.NotNull(loginResult.Data);
        Assert.NotEmpty(loginResult.Data.Token);

    }

    [Fact]
    public async Task Login_WhenCredentialsAreInvalid_ShouldReturnUnauthorized()
    {
        // Given
        var requestRegister = new RegisterDto()
        {
            Name = "Maria",
            Email = "maria@teste.com",
            Password = "1234"
        };
        var requestLogin = new LoginDto()
        {
            Email = "maria@teste.com",
            Password = "12345"
        };

        var responseRegister = await _client.PostAsJsonAsync("auth/register", requestRegister);
        Assert.Equal(HttpStatusCode.Created, responseRegister.StatusCode);

        // When
        var responseLogin = await _client.PostAsJsonAsync("auth/login", requestLogin);
        var loginResult = await responseLogin.Content.ReadFromJsonAsync<LoginResponseEnvelope>();


        // Then
        Assert.Equal(HttpStatusCode.Unauthorized, responseLogin.StatusCode);
        Assert.NotNull(loginResult);
        Assert.Null(loginResult.Data);

    }

    private class LoginResponseEnvelope
    {
        public LoginResponseDto? Data {get; set; }
    }
}

