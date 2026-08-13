using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using RvPersonalFinance.Api.Features.Auth;

namespace RvPersonalFinance.Tests.Integration;

public class ProtectedEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ProtectedEndpointTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_ShouldReturnUnauthorized()
    {
        // Given
    
        // When
        var response = await _client.GetAsync("/accounts");
    
        // Then
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithValidToken_ShouldReturnSuccess()
    {
        // Given
        var requestRegister = new RegisterDto()
        {
            Name = "Junior",
            Email = "junior@teste.com",
            Password = "123467"
        };
        var requestLogin = new LoginDto()
        {
            Email = "junior@teste.com",
            Password = "123467"
        };

        var responseRegister = await _client.PostAsJsonAsync("/auth/register", requestRegister);
        Assert.Equal(HttpStatusCode.Created, responseRegister.StatusCode);

        var responseLogin = await _client.PostAsJsonAsync("/auth/login", requestLogin);
        var loginResult = await responseLogin.Content.ReadFromJsonAsync<ResponseEnvelope<LoginResponseDto>>();
        Assert.NotNull(loginResult);
        Assert.NotNull(loginResult.Data);

        var token = loginResult.Data.Token;

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // When
        var response = await _client.GetAsync("/accounts");

        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
