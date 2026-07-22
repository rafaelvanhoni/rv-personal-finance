using RvPersonalFinance.Api.Shared;

namespace RvPersonalFinance.Api.Features.Dashboard;

public static class DashboardEndpoints
{
    public static void MapDashboardEndpoints(this WebApplication app)
    {
        app.MapGet("/dashboard", async (Guid userId, DashboardService service) =>
        {
            var result = await service.GetDashboard(userId);
            return result.ToHttpResult();
        });
    }
}