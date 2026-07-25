using System.Security.Claims;
using RvPersonalFinance.Api.Shared;

namespace RvPersonalFinance.Api.Features.Categories;

public static class CategoryEndpoints
{
    
    public static void MapCategoryEndpoints(this WebApplication app)
    {
        app.MapGet("/categories/{id}", async (Guid id, ClaimsPrincipal user, CategoryService service) =>
        {
            var userId = user.GetUserId();
            var result = await service.GetCategoryById(id, userId);
            return result.ToHttpResult();
        }).RequireAuthorization();

        app.MapGet("/categories", async (ClaimsPrincipal user, CategoryService service) =>
        {
            var userId = user.GetUserId();
            var result = await service.GetAllCategories(userId);
            return result.ToHttpResult();
        }).RequireAuthorization();

        app.MapPost("/categories", async (CreateCategoryDto dto, ClaimsPrincipal user, CategoryService service) =>
        {
            var userId = user.GetUserId();
            var result = await service.CreateCategory(dto, userId);
            if (result.IsSuccess)
                return Results.Created($"/categories/{result.Data?.Id}", result);
            return result.ToHttpResult();
        }).RequireAuthorization();

        app.MapPut("/categories/{id}", async (Guid id, UpdateCategoryDto dto, ClaimsPrincipal user, CategoryService service) =>
        {
            var userId = user.GetUserId();
            var result = await service.UpdateCategory(id, dto, userId);
            return result.ToHttpResult();
        }).RequireAuthorization();

        app.MapDelete("/categories/{id}", async (Guid id, ClaimsPrincipal user, CategoryService service) =>
        {
            var userId = user.GetUserId();
            var result = await service.DeleteCategory(id, userId);
            return result.ToHttpResult();
        }).RequireAuthorization();
    }    
}