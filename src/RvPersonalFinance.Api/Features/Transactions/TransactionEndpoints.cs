using System.Security.Claims;
using RvPersonalFinance.Api.Shared;

namespace RvPersonalFinance.Api.Features.Transactions;

public static class TransactionEndpoints
{
    public static void MapTransactionEndpoints(this WebApplication app)
    {
        app.MapGet("/transactions/{id}", async (Guid id, ClaimsPrincipal user, TransactionService service) =>
        {
            var userId = user.GetUserId();
            
            var result = await service.GetTransactionById(id, userId);

            return result.ToHttpResult();
        }).RequireAuthorization();

        app.MapGet("/transactions", async (ClaimsPrincipal user, TransactionService service) =>
        {
            var userId = user.GetUserId();
            var result = await service.GetAllTransactions(userId);
            return result.ToHttpResult();
        }).RequireAuthorization();

        app.MapPost("/transactions", async (CreateTransactionDto dto, ClaimsPrincipal user, TransactionService service) =>
        {
            var userId = user.GetUserId();
            var result = await service.CreateTransaction(dto, userId);
            if (result.IsSuccess)
                return Results.Created($"/transactions/{result.Data?.Id}", result);
            return result.ToHttpResult();
        }).RequireAuthorization();

        app.MapPut("/transactions/{id}", async (Guid id, UpdateTransactionDto dto, ClaimsPrincipal user, TransactionService service) =>
        {
            var userId = user.GetUserId();
            var result = await service.UpdateTransaction(id, dto, userId);
            return result.ToHttpResult();
        }).RequireAuthorization();

        app.MapDelete("/transactions/{id}", async (Guid id, ClaimsPrincipal user, TransactionService service) =>
        {
            var userId = user.GetUserId();
            var result = await service.DeleteTransaction(id, userId);
            return result.ToHttpResult();
        }).RequireAuthorization();
    }

}