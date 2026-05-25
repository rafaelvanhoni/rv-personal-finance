using RvPersonalFinance.Api.Shared;

namespace RvPersonalFinance.Api.Features.Transactions;

public static class TransactionEndpoints
{
    public static void MapTransactionEndpoints(this WebApplication app)
    {
        app.MapGet("/transactions/{id}", async (Guid id, TransactionService service) =>
        {
            var result = await service.GetTransactionById(id);
            return result.ToHttpResult();
        });

        app.MapGet("/transactions", async (TransactionService service) =>
        {
            var result = await service.GetAll();
            return result.ToHttpResult();
        });

        app.MapPost("/transactions", async (CreateTransationDto dto, TransactionService service) =>
        {
            var result = await service.CreateTransaction(dto);
            if (result.IsSuccess)
                return Results.Created($"/transactions/{result.Data?.Id}", result);
            return result.ToHttpResult();
        });

        app.MapPut("/transactions/{id}", async (Guid id, UpdateTransactionDto dto, TransactionService service) =>
        {
            var result = await service.UpdateTransaction(id, dto);
            return result.ToHttpResult();
        });

        app.MapDelete("/transactions/{id}", async (Guid id, TransactionService service) =>
        {
            var result = await service.DeleteTransaction(id);
            return result.ToHttpResult();
        });
    }
}