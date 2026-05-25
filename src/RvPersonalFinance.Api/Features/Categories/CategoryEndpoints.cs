using RvPersonalFinance.Api.Shared;

namespace RvPersonalFinance.Api.Features.Categories;

public static class CategoryEndpoints
{
    
    public static void MapCategoryEndpoints(this WebApplication app)
    {
        app.MapGet("/categories/{id}", async (Guid id, CategoryService service) =>
        {
            var result = await service.GetCategoryById(id);
            return result.ToHttpResult();
        });

        app.MapGet("/categories", async (CategoryService service) =>
        {
            var result = await service.GetAll();
            return result.ToHttpResult();
        });

        app.MapPost("/categories", async (CreateCategoryDto dto, CategoryService service) =>
        {
            var result = await service.CreateCategory(dto);
            if (result.IsSuccess)
                return Results.Created($"/categories/{result.Data?.Id}", result);
            return result.ToHttpResult();
        });

        app.MapPut("/categories/{id}", async (Guid id, UpdateCategoryDto dto, CategoryService service) =>
        {
            var result = await service.UpdateCategory(id, dto);
            return result.ToHttpResult();
        });

        app.MapDelete("/categories/{id}", async (Guid id, CategoryService service) =>
        {
            var result = await service.DeleteCategory(id);
            return result.ToHttpResult();
        });
    }
    
}