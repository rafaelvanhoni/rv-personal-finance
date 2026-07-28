using System.Security.Claims;
using RvPersonalFinance.Api.Domain.Entities;
using RvPersonalFinance.Api.Shared;

namespace RvPersonalFinance.Api.Features.Accounts;

public static class AccountEndpoints
{
    public static void MapAccountEndpoints(this WebApplication app)
    {
        app.MapGet("/accounts/{id}", async (Guid id, ClaimsPrincipal user, AccountService service) => 
        {
            var userId = user.GetUserId();
            var result = await service.GetAccountById(id, userId);
            return result.ToHttpResult();
        }).RequireAuthorization();

        app.MapGet("/accounts", async (ClaimsPrincipal user, AccountService service) =>
        {
            var userId = user.GetUserId();
            var result = await service.GetAllAccounts(userId);
            return result.ToHttpResult(); 
        }).RequireAuthorization();

        app.MapGet("/accounts/{id}/balance", async (Guid id, ClaimsPrincipal user, AccountService service) =>
        {
            var userId = user.GetUserId();
            var result = await service.CalculateBalance(id, userId);
            return result.ToHttpResult();
        }).RequireAuthorization();

        app.MapPost("/accounts", async (CreateAccountDto dto, ClaimsPrincipal user, AccountService service) =>
        {
            var userId = user.GetUserId();
            var result = await service.CreateAccount(dto, userId);
            if (result.IsSuccess)
                return Results.Created($"/accounts/{result.Data?.Id}", result);
            return result.ToHttpResult();
        }).RequireAuthorization();

        app.MapPut("/accounts/{id}", async (Guid id, UpdateAccountDto dto, ClaimsPrincipal user, AccountService service) =>
        {
            var userId = user.GetUserId();
            var result = await service.UpdateAccount(id, dto, userId);
            return result.ToHttpResult();
        }).RequireAuthorization();

        app.MapDelete("/accounts/{id}", async (Guid id, ClaimsPrincipal user, AccountService service) =>
        {
            var userId = user.GetUserId();
            var result = await service.DeleteAccount(id, userId);
            return result.ToHttpResult();
        }).RequireAuthorization();
    }
}