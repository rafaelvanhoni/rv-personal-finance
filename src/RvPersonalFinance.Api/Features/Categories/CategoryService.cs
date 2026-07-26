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

    private async Task<Category?> GetCategoryByIdAsync(Guid id, Guid userId)
    {
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
        return category;
    }
    public async Task<OperationResult<CategoryResponseDto>> GetCategoryById(Guid id, Guid userId)
    {
        var category = await GetCategoryByIdAsync(id, userId);

        if (category is null)
        {
            _logger.LogWarning("Category not found: {CategoryId} for user {UserId}.", id, userId);
            return OperationResult<CategoryResponseDto>.NotFound($"Category not found: {id}.");
        }

        var categoryResponseDto = ToResponseDto(category);

        _logger.LogInformation("Category retrieved: {CategoryId} for user {UserId}.", category.Id, category.UserId);
        return OperationResult<CategoryResponseDto>.Success(categoryResponseDto);
    }

    public async Task<OperationResult<IEnumerable<CategoryResponseDto>>> GetAllCategories(Guid userId)
    {
        var categories = await _context.Categories.Where(c => c.UserId == userId).ToListAsync();
        _logger.LogInformation("Categories retrieved: {Count} for user {UserId}.", categories.Count, userId);

        var categoryResponseDtos = categories
            .Select(ToResponseDto)
            .ToList();

        return OperationResult<IEnumerable<CategoryResponseDto>>.Success(categoryResponseDtos);
    }

    public async Task<OperationResult<CategoryResponseDto>> CreateCategory(CreateCategoryDto dto, Guid userId)
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
            UserId = userId,
            Name = dto.Name.Trim(),
        };

        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Category created: {CategoryId} for user {UserId}.", category.Id, category.UserId);

        var categoryResponseDto = ToResponseDto(category);

        return OperationResult<CategoryResponseDto>.Created(categoryResponseDto);
    }

    public async Task<OperationResult<CategoryResponseDto>> UpdateCategory(Guid id, UpdateCategoryDto dto, Guid userId)
    {
        var category = await GetCategoryByIdAsync(id, userId);
        if (category is null) 
        {
            _logger.LogWarning("Category not found: {CategoryId} for user {UserId}.", id, userId);
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

        category.Name = dto.Name.Trim();

        _context.Categories.Update(category);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Category updated: {CategoryId} for user {UserId}.", category.Id, category.UserId);

        var categoryResponseDto = ToResponseDto(category);

        return OperationResult<CategoryResponseDto>.Success(categoryResponseDto);
    }

    public async Task<OperationResult<CategoryResponseDto>> DeleteCategory(Guid id, Guid userId)
    {
        var category = await GetCategoryByIdAsync(id, userId);
        if (category is null) 
        {
            _logger.LogWarning("Category not found: {CategoryId} for user {UserId}.", id, userId);
            return OperationResult<CategoryResponseDto>.NotFound($"Category not found: {id}.");
        }

        var isInUse = await _context.Transactions
            .AnyAsync(t => t.CategoryId == category.Id && t.UserId == category.UserId);
        if (isInUse)
        {
            _logger.LogWarning(
                "Category cannot be deleted because it is in use: {CategoryId} for user {UserId}.",
                category.Id, 
                category.UserId);
            return OperationResult<CategoryResponseDto>.Conflict(
                $"Category cannot be deleted because it has linked transactions: {id}.");
        }

        var categoryResponseDto = ToResponseDto(category);

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Category deleted: {CategoryId} for user {UserId}.", id, userId);

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