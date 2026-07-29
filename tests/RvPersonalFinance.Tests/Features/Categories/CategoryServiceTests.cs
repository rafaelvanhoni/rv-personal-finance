using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using RvPersonalFinance.Api.Infrastructure.Persistence;
using RvPersonalFinance.Api.Features.Categories;
using RvPersonalFinance.Api.Domain.Entities;
using RvPersonalFinance.Api.Shared;
using RvPersonalFinance.Api.Domain.Enums;

namespace RvPersonalFinance.Tests.Features.Categories;

public class CategoryServiceTests
{
    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static CategoryService CreateService(AppDbContext context)
    {
        return new CategoryService(context, NullLogger<CategoryService>.Instance, new CreateCategoryValidator(), new UpdateCategoryValidator());
    }    

    [Fact]
    public async Task GetCategoryById_WhenCategoryBelongsToUser_ShouldReturnSuccess()
    {
        // Given
        await using var context = CreateDbContext();

        var userId = Guid.CreateVersion7();
        var category = new Category()
        {
            UserId = userId,
            Name = "Supermercado"
        };

        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var service = CreateService(context);
    
        // When
        var result = await service.GetCategoryById(category.Id, userId);
    
        // Then
        Assert.True(result.IsSuccess);  
        Assert.NotNull(result.Data);
        Assert.Equal(ResultStatus.Success, result.Status);

        Assert.Equal(category.Id, result.Data.Id);
        Assert.Equal(userId, result.Data.UserId);
        Assert.Equal(category.Name, result.Data.Name);
        Assert.Equal(category.CreatedAt, result.Data.CreatedAt);

    }

    [Fact]
    public async Task GetCategoryById_WhenCategoryDoesNotExist_ShouldReturnFailure()
    {
        // Given
        await using var context = CreateDbContext();

        var categoryId = Guid.CreateVersion7();
        var userId = Guid.CreateVersion7();

        var service = CreateService(context);

        // When
        var result = await service.GetCategoryById(categoryId, userId);
    
        // Then
        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
        Assert.Equal(ResultStatus.NotFound, result.Status);

        var error = Assert.Single(result.Errors);
        Assert.Equal($"Category not found: {categoryId}.", error.Message);
    }

    [Fact]
    public async Task GetCategoryById_WhenCategoryBelongsToAnotherUser_ShouldReturnFailure()
    {
        // Given
        await using var context = CreateDbContext();

        var userA = Guid.CreateVersion7();
        var userB = Guid.CreateVersion7();

        var category = new Category()
        {
            UserId = userA,
            Name = "Compras",
        };

        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        // When
        var result = await service.GetCategoryById(category.Id, userB);
    
        // Then
        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
        Assert.Equal(ResultStatus.NotFound, result.Status);

        var error = Assert.Single(result.Errors);
        Assert.Equal($"Category not found: {category.Id}.", error.Message);
    }

    [Fact]
    public async Task GetAllCategories_WhenUserHasCategories_ShouldReturnCategories()
    {
        // Given
        await using var context = CreateDbContext();

        var userA = Guid.CreateVersion7();
        var userB = Guid.CreateVersion7();

        var category1 = new Category()
        {
            UserId = userA,
            Name = "Roupas"
        };

        var category2 = new Category()
        {
            UserId = userA,
            Name = "Mercado"
        };

        var category3 = new Category()
        {
            UserId = userB,
            Name = "Lazer"
        };

        context.Categories.AddRange(category1, category2, category3);
        await context.SaveChangesAsync();
    
        var service = CreateService(context);

        // When
        var result = await service.GetAllCategories(userA);
    
        // Then
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(ResultStatus.Success, result.Status);

        var returnedCategories = result.Data.ToList();
        Assert.Equal(2, returnedCategories.Count);
        Assert.DoesNotContain(returnedCategories, c => c.UserId == userB);

        var returnedCategory1 = returnedCategories.Single(c => c.Id == category1.Id);
        Assert.Equal(userA, returnedCategory1.UserId);
        Assert.Equal(category1.Name, returnedCategory1.Name);
        Assert.Equal(category1.CreatedAt, returnedCategory1.CreatedAt);

        var returnedCategory2 = returnedCategories.Single(c => c.Id == category2.Id);
        Assert.Equal(userA, returnedCategory2.UserId);
        Assert.Equal(category2.Name, returnedCategory2.Name);
        Assert.Equal(category2.CreatedAt, returnedCategory2.CreatedAt);

    }

    [Fact]
    public async Task GetAllCategories_WhenUserHasNoCategories_ShouldReturnEmptyList()
    {
        // Given
        await using var context = CreateDbContext();

        var userA = Guid.CreateVersion7();
        var userB = Guid.CreateVersion7();

        var category = new Category()
        {
            UserId = userA,
            Name = "Roupas"
        };

        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var service = CreateService(context);
        
        // When
        var result = await service.GetAllCategories(userB);
    
        // Then
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(ResultStatus.Success, result.Status);
        Assert.Empty(result.Data);
    }

    [Fact]
    public async Task Create_WhenDataIsValid_ShouldCreateCategory()
    {
        // Given
        await using var context = CreateDbContext();
    
        var userId = Guid.CreateVersion7();
        var dto = new CreateCategoryDto()
        {
            Name = "Roupas",
        }; 

        var service = CreateService(context);

        // When
        var result = await service.CreateCategory(dto, userId);

        // Then
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(ResultStatus.Created, result.Status);

        var category = result.Data;

        Assert.Equal(userId, category.UserId);
        Assert.Equal(dto.Name, category.Name);

        var persistedCategory = await context.Categories.SingleAsync(c => c.Id == category.Id);
        Assert.Equal(userId, persistedCategory.UserId);
        Assert.Equal(dto.Name, persistedCategory.Name);
    }

    [Fact]
    public async Task Create_WhenValidationFails_ShouldReturnFailure()
    {
        // Given
        await using var context = CreateDbContext();

        var userId = Guid.CreateVersion7();
        var dto = new CreateCategoryDto() 
        {
            Name = new string('A', 51),    
        };

        var service = CreateService(context);

        // When
        var result = await service.CreateCategory(dto, userId);
    
        // Then
        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
        Assert.Equal(ResultStatus.ValidationError, result.Status);
        Assert.False(await context.Categories.AnyAsync());
    }

    [Fact]
    public async Task Update_WhenCategoryExists_ShouldUpdateCategory()
    {
        // Given
        await using var context = CreateDbContext();

        var categoryId = Guid.CreateVersion7();
        var userId = Guid.CreateVersion7();
        var category = new Category()
        {
            Id = categoryId,
            UserId = userId,
            Name = "Comida",
        };

        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var categoryDto = new UpdateCategoryDto()
        {
            Name = "Updated"
        };

        var service = CreateService(context);

        // When
        var result = await service.UpdateCategory(categoryId, categoryDto, userId);

        // Then
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(ResultStatus.Success, result.Status);

        Assert.Equal(categoryId, result.Data.Id);
        Assert.Equal(userId, result.Data.UserId);
        Assert.Equal(categoryDto.Name, result.Data.Name);

        var updatedCategory = await context.Categories.SingleAsync(c => c.Id == categoryId);
        Assert.Equal(userId, updatedCategory.UserId);
        Assert.Equal(categoryDto.Name, updatedCategory.Name);
    }

    [Fact]
    public async Task Update_WhenCategoryBelongsToAnotherUser_ShouldReturnFailure()
    {
        // Given
        await using var context = CreateDbContext();

        var categoryId = Guid.CreateVersion7();
        var userA = Guid.CreateVersion7();
        var userB = Guid.CreateVersion7();

        var category = new Category()
        {
            Id = categoryId,
            UserId = userA,
            Name = "Roupas",
        };

        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var categoryDto = new UpdateCategoryDto()
        {
            Name = "New Category"
        };

        var service = CreateService(context);

        // When
        var result = await service.UpdateCategory(categoryId, categoryDto, userB);
    
        // Then
        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
        Assert.Equal(ResultStatus.NotFound, result.Status);

        var originalCategory = await context.Categories.SingleAsync(c => c.Id == categoryId);
        Assert.Equal(userA, originalCategory.UserId);
        Assert.Equal("Roupas", originalCategory.Name);
    }

    [Fact]
    public async Task Update_WhenValidationFails_ShouldReturnFailure()
    {
        // Given
        await using var context = CreateDbContext();
    
        var categoryId = Guid.CreateVersion7();
        var userId = Guid.CreateVersion7();

        var category = new Category()
        {
            Id = categoryId,
            UserId = userId,
            Name = "Roupas",
        };

        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var categoryDto = new UpdateCategoryDto()
        {
            Name = new string('A', 51),
        };

        var service = CreateService(context);
        
        // When
        var result = await service.UpdateCategory(categoryId, categoryDto, userId);
    
        // Then
        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
        Assert.Equal(ResultStatus.ValidationError, result.Status);

        var originalCategory = await context.Categories.SingleAsync(c => c.Id == categoryId);
        Assert.Equal(userId, originalCategory.UserId);
        Assert.Equal("Roupas", originalCategory.Name);
    }

    [Fact]
    public async Task Delete_WhenCategoryExists_ShouldDeleteCategory()
    {
        // Given
        await using var context = CreateDbContext();
    
        var categoryId = Guid.CreateVersion7();
        var userId = Guid.CreateVersion7();
        
        var category = new Category()
        {
            Id = categoryId,
            UserId = userId,
            Name = "Compras",    
        };

        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        // When
        var result = await service.DeleteCategory(categoryId, userId);
     
        // Then
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(ResultStatus.Success, result.Status);

        Assert.Equal(userId, result.Data.UserId);
        Assert.Equal("Compras", result.Data.Name);

        var deletedCategory = await context.Categories.SingleOrDefaultAsync(c => c.Id == categoryId);
        Assert.Null(deletedCategory);
    }

    [Fact]
    public async Task Delete_WhenCategoryBelongsToAnotherUser_ShouldReturnFailure()
    {
        // Given
        await using var context = CreateDbContext();

        var categoryId = Guid.CreateVersion7();
        var userA = Guid.CreateVersion7();
        var userB = Guid.CreateVersion7();
        
        var category = new Category()
        {
            Id = categoryId,
            UserId = userA,
            Name = "Compras",    
        };

        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        // When
        var result = await service.DeleteCategory(categoryId, userB);

        // Then
        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
        Assert.Equal(ResultStatus.NotFound, result.Status);

        var existingCategory = await context.Categories.SingleAsync(c => c.Id == categoryId);
        Assert.Equal(userA, existingCategory.UserId);
        Assert.Equal("Compras", existingCategory.Name);
    }

    [Fact]
    public async Task Delete_WhenCategoryHasTransactions_ShouldReturnFailure()
    {
        // Given
        await using var context = CreateDbContext();
    
        var categoryId = Guid.CreateVersion7();
        var userId = Guid.CreateVersion7();
        var category = new Category()
        {
            Id = categoryId,
            UserId = userId,
            Name = "Compras",
        };

        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var transaction = new Transaction()
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            AccountId = Guid.CreateVersion7(),
            CategoryId = categoryId,
            Description = "New Transaction",
            Amount = 1000m,
            Type = TransactionType.Expense
        };

        context.Transactions.Add(transaction);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        // When
        var result = await service.DeleteCategory(categoryId, userId);

        // Then
        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
        Assert.Equal(ResultStatus.Conflict, result.Status);

        var existingCategory = await context.Categories.SingleAsync(a => a.Id == categoryId);
       
        Assert.Equal(userId, existingCategory.UserId);
        Assert.Equal("Compras", existingCategory.Name);

    }
}