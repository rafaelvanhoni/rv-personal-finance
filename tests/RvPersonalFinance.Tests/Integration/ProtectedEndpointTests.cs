using System.Net;
using System.Net.Http.Headers;

namespace RvPersonalFinance.Tests.Integration;

public class ProtectedEndpointTests : IntegrationTestBase
{

    public ProtectedEndpointTests(CustomWebApplicationFactory factory) : base(factory)
    {

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
        var user = await RegisterAndLoginAsync();
        var token = user.Login.Token;

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // When
        var response = await _client.GetAsync("/accounts");

        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
