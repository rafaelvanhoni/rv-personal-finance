using FluentValidation;
using Microsoft.EntityFrameworkCore;
using RvPersonalFinance.Api.Domain.Entities;
using RvPersonalFinance.Api.Infrastructure.Persistence;
using RvPersonalFinance.Api.Shared;

namespace RvPersonalFinance.Api.Features.Categories;

public class CategoryService
{
    private readonly AppDbContext _context;
    private readonly ILogger<CategoryService> _logger;
    private readonly IValidator<CreateCategoryDto> _createValidator;

    public CategoryService(AppDbContext context, ILogger<CategoryService> logger, IValidator<CreateCategoryDto> createValidator)
    {
        _context = context;
        _logger = logger;    
        _createValidator = createValidator;
    }

    private async Task<Category?> GetCategoryByIdAsync(Guid id)
    {
        var category = await _context.Categories.FirstOrDefaultAsync(category => category.Id == id);
        return category;
    }
    public async Task<OperationResult<CategoryResponseDto>> GetCategoryById(Guid id)
    {
        var category = await GetCategoryByIdAsync(id);

        if (category is null)
        {
            _logger.LogWarning("Category not found: {CategoryId}", id);
            return new OperationResult<CategoryResponseDto>
            {
                Status = ResultStatus.NotFound,
                Message = $"Category not found: {id}",
            };
        }

        var categoryResponseDto = ToResponseDto(category);

        _logger.LogInformation("Category Retrieved: {CategoryId}", category.Id);
        return new OperationResult<CategoryResponseDto>() { Data = categoryResponseDto };
    }

    public async Task<OperationResult<IEnumerable<CategoryResponseDto>>> GetAllCategories()
    {
        var categories = await _context.Categories.ToListAsync();
        _logger.LogInformation("Categories retrieved: {Count}", categories.Count);

        var categoryResponseDtos = categories
            .Select(ToResponseDto)
            .ToList();

        return new OperationResult<IEnumerable<CategoryResponseDto>>() 
        { 
            Data = categoryResponseDtos 
        };
    }

    public async Task<OperationResult<CategoryResponseDto>> CreateCategory(CreateCategoryDto dto)
    {
        var validation = await _createValidator.ValidateAsync(dto);
        if (!validation.IsValid)
        {
            _logger.LogWarning("Validation failed for CreateCategory: {Error}", validation.Errors.First().ErrorMessage);
            return new OperationResult<CategoryResponseDto>()
            {
                Status = ResultStatus.ValidationError,
                Message = validation.Errors.First().ErrorMessage,
            };
        }

        var category = new Category()
        {
            UserId = dto.UserId,
            Name = dto.Name.Trim(),
        };

        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Category created: {CategoryId}", category.Id);

        var categoryResponseDto = ToResponseDto(category);

        return new OperationResult<CategoryResponseDto>()
        {
            Status = ResultStatus.Created,
            Data = categoryResponseDto
        };
    }

    public async Task<OperationResult<CategoryResponseDto>> UpdateCategory(Guid id, UpdateCategoryDto dto)
    {
        var category = await GetCategoryByIdAsync(id);
        if (category is null) 
        {
            _logger.LogWarning("Category not found: {CategoryId}", id);
            return new OperationResult<CategoryResponseDto>
            {
                Status = ResultStatus.NotFound,
                Message = $"Category not found: {id}",
            };

        }

        category.UserId = dto.UserId;
        category.Name = dto.Name.Trim();

        _context.Categories.Update(category);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Category updated: {CategoryId}", category.Id);

        var categoryResponseDto = ToResponseDto(category);

        return new OperationResult<CategoryResponseDto>() { Data = categoryResponseDto };
    }

    public async Task<OperationResult<CategoryResponseDto>> DeleteCategory(Guid id)
    {
        var category = await GetCategoryByIdAsync(id);
        if (category is null) 
        {
            _logger.LogWarning("Category not found: {CategoryId}", id);
            return new OperationResult<CategoryResponseDto>
            {
                Status = ResultStatus.NotFound,
                Message = $"Category not found: {id}",
            };

        }

        var categoryResponseDto = ToResponseDto(category);

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Category deleted {CategoryId}", id);

        return new OperationResult<CategoryResponseDto>() { Data = categoryResponseDto };
    }

    private static CategoryResponseDto ToResponseDto(Category category)
    {
        return new CategoryResponseDto()
        {
            Id = category.Id,
            UserId = category.UserId,
            Name = category.Name,
            CreatedAt = category.CreatedAt
        };
    }
}