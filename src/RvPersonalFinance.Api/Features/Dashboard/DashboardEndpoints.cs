using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using RvPersonalFinance.Api.Shared;

namespace RvPersonalFinance.Api.Features.Dashboard;

public static class DashboardEndpoints
{
    public static void MapDashboardEndpoints(this WebApplication app)
    {
        app.MapGet("/dashboard", async (ClaimsPrincipal user, DashboardService service) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
            var result = await service.GetDashboard(userId);
            return result.ToHttpResult();
        }).RequireAuthorization();
    }
}