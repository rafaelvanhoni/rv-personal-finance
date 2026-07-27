using RvPersonalFinance.Api.Features.Categories;

namespace RvPersonalFinance.Tests.Features.Categories;

public class CategoryValidatorsTests
{
    [Fact]
    public void CreateCategory_WithValidData_ShouldBeValid()
    {
        // Given
        var validator = new CreateCategoryValidator();

        var dto = new CreateCategoryDto() 
        {
            Name = new string('A', 50)
        };
    
        // When
        var result = validator.Validate(dto);
    
        // Then
        Assert.True(result.IsValid);
    }

    [Fact]
    public void CreateCategory_WithEmptyName_ShouldBeInvalid()
    {
        // Given
        var validator = new CreateCategoryValidator();
    
        var dto = new CreateCategoryDto()
        {
            Name = string.Empty
        };
    
        // When
        var result = validator.Validate(dto);
        
        // Then
        Assert.False(result.IsValid);
    }

    [Fact]
    public void CreateCategory_WithNameLongerThan50Characters_ShouldBeInvalid()
    {
        // Given
        var validator = new CreateCategoryValidator();

        var dto = new CreateCategoryDto()
        {
            Name = new string('A', 51)
        };
    
        // When
        var result = validator.Validate(dto);

        // Then
        Assert.False(result.IsValid);
    }

    [Fact]
    public void UpdateCategory_WithValidData_ShouldBeValid()
    {
        // Given
        var validator = new UpdateCategoryValidator();

        var dto = new UpdateCategoryDto() 
        {
            Name = new string('A', 50)
        };
    
        // When
        var result = validator.Validate(dto);
    
        // Then
        Assert.True(result.IsValid);
    }

    [Fact]
    public void UpdateCategory_WithEmptyName_ShouldBeInvalid()
    {
        // Given
        var validator = new UpdateCategoryValidator();
    
        var dto = new UpdateCategoryDto()
        {
            Name = string.Empty
        };
    
        // When
        var result = validator.Validate(dto);
        
        // Then
        Assert.False(result.IsValid);
    }

    [Fact]
    public void UpdateCategory_WithNameLongerThan50Characters_ShouldBeInvalid()
    {
        // Given
        var validator = new UpdateCategoryValidator();

        var dto = new UpdateCategoryDto()
        {
            Name = new string('A', 51)
        };
    
        // When
        var result = validator.Validate(dto);

        // Then
        Assert.False(result.IsValid);
    }    
}