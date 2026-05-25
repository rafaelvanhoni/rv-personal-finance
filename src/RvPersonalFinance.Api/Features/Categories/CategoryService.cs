using Microsoft.EntityFrameworkCore;
using RvPersonalFinance.Api.Domain.Entities;
using RvPersonalFinance.Api.Infrastructure.Persistence;
using RvPersonalFinance.Api.Shared;

namespace RvPersonalFinance.Api.Features.Categories;

public class CategoryService
{
    private readonly AppDbContext _context;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(AppDbContext context, ILogger<CategoryService> logger)
    {
        _context = context;
        _logger = logger;    
    }

    public async Task<OperationResult<Category>> GetCategoryById(Guid id)
    {
        var category = await _context.Categories.FirstOrDefaultAsync(category => category.Id == id);

        if (category is null)
        {
            _logger.LogWarning("Category not found: {CategoryId}", id);
            return new OperationResult<Category>
            {
                Status = ResultStatus.NotFound,
                Message = $"Category not found: {id}",
            };
        }

        _logger.LogInformation("Category Retrieved: {CategoryId}", category.Id);
        return new OperationResult<Category>() { Data = category };
    }

    public async Task<OperationResult<IEnumerable<Category>>> GetAll()
    {
        var categories = await _context.Categories.ToListAsync();
        _logger.LogInformation("Categories retrieved: {Count}", categories.Count);

        return new OperationResult<IEnumerable<Category>>() { Data = categories };
    }

    public async Task<OperationResult<Category>> CreateCategory(CreateCategoryDto dto)
    {
        var category = new Category()
        {
            UserId = dto.UserId,
            Name = dto.Name,
        };

        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Category created: {CategoryId}", category.Id);

        return new OperationResult<Category>()
        {
            Status = ResultStatus.Created,
            Data = category
        };
    }

    public async Task<OperationResult<Category>> UpdateCategory(Guid id, UpdateCategoryDto dto)
    {
        var result = await GetCategoryById(id);
        if (!result.IsSuccess)
            return result;

        var category = result.Data!;
        category.UserId = dto.UserId;
        category.Name = dto.Name;

        _context.Categories.Update(category);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Category updated: {CategoryId}", category.Id);

        return new OperationResult<Category>() { Data = category };
    }

    public async Task<OperationResult<Category>> DeleteCategory(Guid id)
    {
        var result = await GetCategoryById(id);
        if (!result.IsSuccess)
            return result;

        var category = result.Data!;
        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Category deleted {CategoryId}", id);
        
        return new OperationResult<Category>() { Data = category };
    }
}