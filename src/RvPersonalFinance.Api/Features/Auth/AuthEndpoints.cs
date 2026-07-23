using RvPersonalFinance.Api.Shared;

namespace RvPersonalFinance.Api.Features.Auth;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        app.MapPost("/auth/register", async (RegisterDto dto, AuthService service) =>
        {
            var result = await service.Register(dto);
            return result.ToHttpResult();
        });

        app.MapPost("/auth/login", async (LoginDto dto, AuthService service) =>
        {
            var result = await service.Login(dto);
            return result.ToHttpResult();
        });
    }
}