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
    private readonly IValidator<UpdateCategoryDto> _updateValidator;

    public CategoryService(AppDbContext context, ILogger<CategoryService> logger, IValidator<CreateCategoryDto> createValidator, IValidator<UpdateCategoryDto> updateValidator)
    {
        _context = context;
        _logger = logger;    
        _createValidator = createValidator;
        _updateValidator = updateValidator;
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
            _logger.LogWarning("Category not found: {CategoryId}.", id);
            return OperationResult<CategoryResponseDto>.NotFound($"Category not found: {id}.");
        }

        var categoryResponseDto = ToResponseDto(category);

        _logger.LogInformation("Category retrieved: {CategoryId}.", category.Id);
        return OperationResult<CategoryResponseDto>.Success(categoryResponseDto);
    }

    public async Task<OperationResult<IEnumerable<CategoryResponseDto>>> GetAllCategories()
    {
        var categories = await _context.Categories.ToListAsync();
        _logger.LogInformation("Categories retrieved: {Count}.", categories.Count);

        var categoryResponseDtos = categories
            .Select(ToResponseDto)
            .ToList();

        return OperationResult<IEnumerable<CategoryResponseDto>>.Success(categoryResponseDtos);
    }

    public async Task<OperationResult<CategoryResponseDto>> CreateCategory(CreateCategoryDto dto)
    {
        var validation = await _createValidator.ValidateAsync(dto);
        if (!validation.IsValid)
        {
            var errors = validation.Errors.ToOperationErrors();
            foreach (var item in errors)
            {
                _logger.LogWarning("Validation failed for CreateCategory. ErrorMessage: {ErrorMessage}. Property: {Property}.", item.Message, item.Property);
            }
            
            return OperationResult<CategoryResponseDto>.ValidationError(errors);
        }

        var category = new Category()
        {
            UserId = dto.UserId,
            Name = dto.Name.Trim(),
        };

        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Category created: {CategoryId}.", category.Id);

        var categoryResponseDto = ToResponseDto(category);

        return OperationResult<CategoryResponseDto>.Created(categoryResponseDto);
    }

    public async Task<OperationResult<CategoryResponseDto>> UpdateCategory(Guid id, UpdateCategoryDto dto)
    {
        var category = await GetCategoryByIdAsync(id);
        if (category is null) 
        {
            _logger.LogWarning("Category not found: {CategoryId}", id);
            return OperationResult<CategoryResponseDto>.NotFound($"Category not found: {id}.");
        }

        var validation = await _updateValidator.ValidateAsync(dto);
        if (!validation.IsValid)
        {
            var errors = validation.Errors.ToOperationErrors();
            foreach (var item in errors)
            {
                _logger.LogWarning("Validation failed for UpdateCategory {CategoryId}. ErrorMessage: {ErrorMessage}. Property: {Property}.", id, item.Message, item.Property);                
            }
            return OperationResult<CategoryResponseDto>.ValidationError(errors);
        }

        category.UserId = dto.UserId;
        category.Name = dto.Name.Trim();

        _context.Categories.Update(category);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Category updated: {CategoryId}", category.Id);

        var categoryResponseDto = ToResponseDto(category);

        return OperationResult<CategoryResponseDto>.Success(categoryResponseDto);
    }

    public async Task<OperationResult<CategoryResponseDto>> DeleteCategory(Guid id)
    {
        var category = await GetCategoryByIdAsync(id);
        if (category is null) 
        {
            _logger.LogWarning("Category not found: {CategoryId}.", id);
            return OperationResult<CategoryResponseDto>.NotFound($"Category not found: {id}");
        }

        var categoryResponseDto = ToResponseDto(category);

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Category deleted {CategoryId}", id);

        return OperationResult<CategoryResponseDto>.Success(categoryResponseDto);
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