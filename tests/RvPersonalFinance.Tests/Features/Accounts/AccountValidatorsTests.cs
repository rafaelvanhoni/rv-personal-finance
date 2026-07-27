using RvPersonalFinance.Api.Features.Accounts;

namespace RvPersonalFinance.Tests.Features.Accounts;

public class AccountValidatorsTests
{

    [Fact]
    public void CreateAccount_WithValidData_ShouldBeValid()
    {
        // Given
        var validator = new CreateAccountValidator();

        var dto = new CreateAccountDto()
        {
            Name = new string('A', 80),
            InitialBalance = 100m
        };

        // When
        var result = validator.Validate(dto);
    
        // Then
        Assert.True(result.IsValid);
    }

    [Fact]
    public void CreateAccount_WithEmptyName_ShouldBeInvalid()
    {
        // Given
        var validator = new CreateAccountValidator();

        var dto = new CreateAccountDto()
        {
            Name = string.Empty,
            InitialBalance = 100m
        };
        // When
        var result = validator.Validate(dto);

        // Then
        Assert.False(result.IsValid);
    }

    [Fact]
    public void CreateAccount_WithNameLongerThan80Characters_ShouldBeInvalid()
    {
        // Given
        var validator = new CreateAccountValidator();

        var dto = new CreateAccountDto()
        {
            Name = new string('A', 81)
        };
    
        // When
        var result = validator.Validate(dto);

        // Then
        Assert.False(result.IsValid);
    }

    [Fact]
    public void CreateAccount_WithNegativeInitialBalance_ShouldBeInvalid()
    {
        // Given
        var validator = new CreateAccountValidator();

        var dto = new CreateAccountDto()
        {
            Name = new string('A', 80),
            InitialBalance = -500m    
        };
    
        // When
        var result = validator.Validate(dto);
    
        // Then
        Assert.False(result.IsValid);
    }

    [Fact]
    public void CreateAccount_WithMoreThanTwoDecimalPlaces_ShouldBeInvalid()
    {
        // Given
        var validator = new CreateAccountValidator();
    
        var dto = new CreateAccountDto()
        {
            Name = new string('A', 80),
            InitialBalance = 1000.12345m
        };
        // When
        var result = validator.Validate(dto);
    
        // Then
        Assert.False(result.IsValid);
    }

    [Fact]
    public void UpdateAccount_WithValidData_ShouldBeValid()
    {
        // Given
        var validator = new UpdateAccountValidator();

        var dto = new UpdateAccountDto
        {
            Name = new string('A', 80),
            InitialBalance = 100m
        };
    
        // When
        var result = validator.Validate(dto);
    
        // Then
        Assert.True(result.IsValid);
    }

    [Fact]
    public void UpdateAccount_WithEmptyName_ShouldBeInvalid()
    {
        // Given
        var validator = new UpdateAccountValidator();

        var dto = new UpdateAccountDto
        {
            Name = string.Empty,
            InitialBalance = 400m
        };
    
        // When
        var result = validator.Validate(dto);
    
        // Then
        Assert.False(result.IsValid);
    }

    [Fact]
    public void UpdateAccount_WithNameLongerThan80Characters_ShouldBeInvalid()
    {
        // Given
        var validator = new UpdateAccountValidator();

        var dto = new UpdateAccountDto
        {
            Name = new string('A', 81)
        };
    
        // When
        var result = validator.Validate(dto);
    
        // Then
        Assert.False(result.IsValid);
    }

    [Fact]
    public void UpdateAccount_WithNegativeInitialBalance_ShouldBeInvalid()
    {
        // Given
        var validator = new UpdateAccountValidator();
    
        var dto = new UpdateAccountDto
        {
            Name = new string('A', 80),
            InitialBalance = -250.50m
        };

        // When
        var result = validator.Validate(dto);
    
        // Then
        Assert.False(result.IsValid);
    }

    [Fact]
    public void UpdateAccount_WithMoreThanTwoDecimalPlaces_ShouldBeInvalid()
    {
        // Given
        var validator = new UpdateAccountValidator();
    
        var dto = new UpdateAccountDto
        {
            Name = new string('A', 80),
            InitialBalance = 100.98765m
        };
        // When
        var result = validator.Validate(dto);
    
        // Then
        Assert.False(result.IsValid);
    }
}
