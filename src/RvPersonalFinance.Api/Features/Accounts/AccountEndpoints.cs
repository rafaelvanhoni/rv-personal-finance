using RvPersonalFinance.Api.Shared;

namespace RvPersonalFinance.Api.Features.Accounts;

public static class AccountEndpoints
{
    public static void MapAccountEndpoints(this WebApplication app)
    {
        app.MapGet("/accounts/{id}", async (Guid id, AccountService service) => 
        {
            var result = await service.GetAccountById(id);
            return result.ToHttpResult();
        });

        app.MapGet("/accounts", async (AccountService service) =>
        {
           var result = await service.GetAllAccounts();
           return result.ToHttpResult(); 
        });

        app.MapGet("/accounts/{id}/balance", async (Guid id, AccountService service) =>
        {
            var result = await service.CalculateBalance(id);
            return result.ToHttpResult();
        });

        app.MapPost("/accounts", async (CreateAccountDto dto, AccountService service) =>
        {
            var result = await service.CreateAccount(dto);
            if (result.IsSuccess)
                return Results.Created($"/accounts/{result.Data?.Id}", result);
            return result.ToHttpResult();
        });

        app.MapPut("/accounts/{id}", async (Guid id, UpdateAccountDto dto, AccountService service) =>
        {
            var result = await service.UpdateAccount(id, dto);
            return result.ToHttpResult();
        });

        app.MapDelete("/accounts/{id}", async (Guid id, AccountService service) =>
        {
            var result = await service.DeleteAccount(id);
            return result.ToHttpResult();
        });
    }
}