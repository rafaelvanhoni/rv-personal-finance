using System.Net;

namespace RvPersonalFinance.Tests.Integration;

public class HealthCheckTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    public HealthCheckTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Health_WhenApiIsUp_ShouldReturnHealthy()
    {
        // Given
    
        // When
        var response = await _client.GetAsync("/health");
    
        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
