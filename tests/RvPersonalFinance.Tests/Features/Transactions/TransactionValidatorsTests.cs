using RvPersonalFinance.Api.Features.Transactions;
using RvPersonalFinance.Api.Domain.Enums;

namespace RvPersonalFinance.Tests.Features.Transactions;

public class TransactionValidatorsTests
{
    [Fact]
    public void CreateTransaction_WithValidData_ShouldBeValid()
    {
        // Given
        var validator = new CreateTransactionValidator();

        var dto = new CreateTransactionDto()
        {
            AccountId = Guid.NewGuid(),
            CategoryId = Guid.NewGuid(),
            Description = new string('T', 100),
            Amount = 100.50m,
            Type = TransactionType.Expense,
            TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        // When
        var result = validator.Validate(dto);
    
        // Then
        Assert.True(result.IsValid);
    }

    [Fact]
    public void CreateTransaction_WithEmptyAccountId_ShouldBeInvalid()
    {
        // Given
        var validator = new CreateTransactionValidator();

        var dto = new CreateTransactionDto()
        {
            AccountId = Guid.Empty,
            CategoryId = Guid.NewGuid(),
            Description = new string('T', 100),
            Amount = 100.50m,
            Type = TransactionType.Expense,
            TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        // When
        var result = validator.Validate(dto);
    
        // Then
        Assert.False(result.IsValid);
    }

    [Fact]
    public void CreateTransaction_WithEmptyCategoryId_ShouldBeInvalid()
    {
        // Given
        var validator = new CreateTransactionValidator();

        var dto = new CreateTransactionDto()
        {
            AccountId = Guid.NewGuid(),
            CategoryId = Guid.Empty,
            Description = new string('T', 100),
            Amount = 100.50m,
            Type = TransactionType.Expense,
            TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        // When
        var result = validator.Validate(dto);
    
        // Then
        Assert.False(result.IsValid);
    }

    [Fact]
    public void CreateTransaction_WithEmptyDescription_ShouldBeInvalid()
    {
        // Given
        var validator = new CreateTransactionValidator();

        var dto = new CreateTransactionDto()
        {
            AccountId = Guid.NewGuid(),
            CategoryId = Guid.NewGuid(),
            Description = string.Empty,
            Amount = 100.50m,
            Type = TransactionType.Expense,
            TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        // When
        var result = validator.Validate(dto);
    
        // Then
        Assert.False(result.IsValid);
    }

    [Fact]
    public void CreateTransaction_WithDescriptionLongerThan100Characters_ShouldBeInvalid()
    {
        // Given
        var validator = new CreateTransactionValidator();

        var dto = new CreateTransactionDto()
        {
            AccountId = Guid.NewGuid(),
            CategoryId = Guid.NewGuid(),
            Description = new string('T', 101),
            Amount = 100.50m,
            Type = TransactionType.Expense,
            TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        // When
        var result = validator.Validate(dto);
    
        // Then
        Assert.False(result.IsValid);
    }

    [Fact]
    public void CreateTransaction_WithZeroAmount_ShouldBeInvalid()
    {
        // Given
        var validator = new CreateTransactionValidator();

        var dto = new CreateTransactionDto()
        {
            AccountId = Guid.NewGuid(),
            CategoryId = Guid.NewGuid(),
            Description = new string('T', 100),
            Amount = 0m,
            Type = TransactionType.Expense,
            TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        // When
        var result = validator.Validate(dto);
    
        // Then
        Assert.False(result.IsValid);
    }

    [Fact]
    public void CreateTransaction_WithMoreThanTwoDecimalPlaces_ShouldBeInvalid()
    {
        // Given
        var validator = new CreateTransactionValidator();

        var dto = new CreateTransactionDto()
        {
            AccountId = Guid.NewGuid(),
            CategoryId = Guid.NewGuid(),
            Description = new string('T', 100),
            Amount = 100.1234m,
            Type = TransactionType.Expense,
            TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        // When
        var result = validator.Validate(dto);
    
        // Then
        Assert.False(result.IsValid);
    }

    [Fact]
    public void CreateTransaction_WithInvalidTransactionType_ShouldBeInvalid()
    {
        // Given
        var validator = new CreateTransactionValidator();

        var dto = new CreateTransactionDto()
        {
            AccountId = Guid.NewGuid(),
            CategoryId = Guid.NewGuid(),
            Description = new string('T', 100),
            Amount = 100m,
            Type = (TransactionType)999,
            TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        // When
        var result = validator.Validate(dto);
    
        // Then
        Assert.False(result.IsValid);
    }

    [Fact]
    public void CreateTransaction_WithEmptyTransactionDate_ShouldBeInvalid()
    {
        // Given
        var validator = new CreateTransactionValidator();

        var dto = new CreateTransactionDto()
        {
            AccountId = Guid.NewGuid(),
            CategoryId = Guid.NewGuid(),
            Description = new string('T', 100),
            Amount = 100.10m,
            Type = TransactionType.Expense,
            TransactionDate = default(DateOnly)
        };

        // When
        var result = validator.Validate(dto);
    
        // Then
        Assert.False(result.IsValid);
    }

    [Fact]
    public void UpdateTransaction_WithValidData_ShouldBeValid()
    {
        // Given
        var validator = new UpdateTransactionValidator();

        var dto = new UpdateTransactionDto()
        {
            AccountId = Guid.NewGuid(),
            CategoryId = Guid.NewGuid(),
            Description = new string('T', 100),
            Amount = 100.50m,
            Type = TransactionType.Expense,
            TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        // When
        var result = validator.Validate(dto);
    
        // Then
        Assert.True(result.IsValid);
    }

    [Fact]
    public void UpdateTransaction_WithEmptyAccountId_ShouldBeInvalid()
    {
        // Given
        var validator = new UpdateTransactionValidator();

        var dto = new UpdateTransactionDto()
        {
            AccountId = Guid.Empty,
            CategoryId = Guid.NewGuid(),
            Description = new string('T', 100),
            Amount = 100.50m,
            Type = TransactionType.Expense,
            TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        // When
        var result = validator.Validate(dto);
    
        // Then
        Assert.False(result.IsValid);
    }

    [Fact]
    public void UpdateTransaction_WithEmptyCategoryId_ShouldBeInvalid()
    {
        // Given
        var validator = new UpdateTransactionValidator();

        var dto = new UpdateTransactionDto()
        {
            AccountId = Guid.NewGuid(),
            CategoryId = Guid.Empty,
            Description = new string('T', 100),
            Amount = 100.50m,
            Type = TransactionType.Expense,
            TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        // When
        var result = validator.Validate(dto);
    
        // Then
        Assert.False(result.IsValid);
    }

    [Fact]
    public void UpdateTransaction_WithEmptyDescription_ShouldBeInvalid()
    {
        // Given
        var validator = new UpdateTransactionValidator();

        var dto = new UpdateTransactionDto()
        {
            AccountId = Guid.NewGuid(),
            CategoryId = Guid.NewGuid(),
            Description = string.Empty,
            Amount = 100.50m,
            Type = TransactionType.Expense,
            TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        // When
        var result = validator.Validate(dto);
    
        // Then
        Assert.False(result.IsValid);
    }

    [Fact]
    public void UpdateTransaction_WithDescriptionLongerThan100Characters_ShouldBeInvalid()
    {
        // Given
        var validator = new UpdateTransactionValidator();

        var dto = new UpdateTransactionDto()
        {
            AccountId = Guid.NewGuid(),
            CategoryId = Guid.NewGuid(),
            Description = new string('T', 101),
            Amount = 100.50m,
            Type = TransactionType.Expense,
            TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        // When
        var result = validator.Validate(dto);
    
        // Then
        Assert.False(result.IsValid);
    }

    [Fact]
    public void UpdateTransaction_WithZeroAmount_ShouldBeInvalid()
    {
        // Given
        var validator = new UpdateTransactionValidator();

        var dto = new UpdateTransactionDto()
        {
            AccountId = Guid.NewGuid(),
            CategoryId = Guid.NewGuid(),
            Description = new string('T', 100),
            Amount = 0m,
            Type = TransactionType.Expense,
            TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        // When
        var result = validator.Validate(dto);
    
        // Then
        Assert.False(result.IsValid);
    }

    [Fact]
    public void UpdateTransaction_WithMoreThanTwoDecimalPlaces_ShouldBeInvalid()
    {
        // Given
        var validator = new UpdateTransactionValidator();

        var dto = new UpdateTransactionDto()
        {
            AccountId = Guid.NewGuid(),
            CategoryId = Guid.NewGuid(),
            Description = new string('T', 100),
            Amount = 100.1234m,
            Type = TransactionType.Expense,
            TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        // When
        var result = validator.Validate(dto);
    
        // Then
        Assert.False(result.IsValid);
    }

    [Fact]
    public void UpdateTransaction_WithInvalidTransactionType_ShouldBeInvalid()
    {
        // Given
        var validator = new UpdateTransactionValidator();

        var dto = new UpdateTransactionDto()
        {
            AccountId = Guid.NewGuid(),
            CategoryId = Guid.NewGuid(),
            Description = new string('T', 100),
            Amount = 100m,
            Type = (TransactionType)999,
            TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        // When
        var result = validator.Validate(dto);
    
        // Then
        Assert.False(result.IsValid);
    }

    [Fact]
    public void UpdateTransaction_WithEmptyTransactionDate_ShouldBeInvalid()
    {
        // Given
        var validator = new UpdateTransactionValidator();

        var dto = new UpdateTransactionDto()
        {
            AccountId = Guid.NewGuid(),
            CategoryId = Guid.NewGuid(),
            Description = new string('T', 100),
            Amount = 100.10m,
            Type = TransactionType.Expense,
            TransactionDate = default(DateOnly)
        };

        // When
        var result = validator.Validate(dto);
    
        // Then
        Assert.False(result.IsValid);
    }
}