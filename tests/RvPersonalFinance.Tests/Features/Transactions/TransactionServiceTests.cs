using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using RvPersonalFinance.Api.Infrastructure.Persistence;
using RvPersonalFinance.Api.Features.Transactions;
using RvPersonalFinance.Api.Domain.Entities;
using RvPersonalFinance.Api.Shared;
using RvPersonalFinance.Api.Domain.Enums;

namespace RvPersonalFinance.Tests.Features.Transactions;

public class TransactionServiceTests
{
    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static TransactionService CreateService(AppDbContext context)
    {
        return new TransactionService(context, NullLogger<TransactionService>.Instance, new CreateTransactionValidator(), new UpdateTransactionValidator());
    }    

    [Fact]
    public async Task GetTransactionById_WhenTransactionBelongsToUser_ShouldReturnSuccess()
    {
        // Given
        await using var context = CreateDbContext();

        var transactionId = Guid.CreateVersion7();
        var userId = Guid.CreateVersion7();
        var accountId = Guid.CreateVersion7();
        var categoryId = Guid.CreateVersion7();

        var transaction = new Transaction()
        {
            Id = transactionId,
            UserId = userId,
            AccountId = accountId,
            CategoryId = categoryId,
            Description = "compra de uma camisa",
            Amount = 100m,
            Type = TransactionType.Expense,
            TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow),
        };

        context.Transactions.Add(transaction);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        // When
        var result = await service.GetTransactionById(transactionId, userId);
    
        // Then
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(ResultStatus.Success, result.Status);

        Assert.Equal(transactionId, result.Data.Id);
        Assert.Equal(userId, result.Data.UserId);
        Assert.Equal(accountId, result.Data.AccountId);
        Assert.Equal(categoryId, result.Data.CategoryId);
        Assert.Equal(transaction.Description, result.Data.Description);
        Assert.Equal(transaction.Amount, result.Data.Amount);                                        
        Assert.Equal(transaction.Type, result.Data.Type);                                        
        Assert.Equal(transaction.TransactionDate, result.Data.TransactionDate);        
        Assert.Equal(transaction.CreatedAt, result.Data.CreatedAt);                                
    }

    [Fact]
    public async Task GetTransactionById_WhenTransactionDoesNotExist_ShouldReturnFailure()
    {
        // Given
        await using var context = CreateDbContext();

        var transactionId = Guid.CreateVersion7();
        var userId = Guid.CreateVersion7();
        var service = CreateService(context);

        // When
        var result = await service.GetTransactionById(transactionId, userId);
    
        // Then
        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
        Assert.Equal(ResultStatus.NotFound, result.Status);

        var error = Assert.Single(result.Errors);
        Assert.Equal($"Transaction not found: {transactionId}.", error.Message);
    }

    [Fact]
    public async Task GetTransactionById_WhenTransactionBelongsToAnotherUser_ShouldReturnFailure()
    {
        // Given
        await using var context = CreateDbContext();

        var transactionId = Guid.CreateVersion7();
        var accountId = Guid.CreateVersion7();
        var categoryId = Guid.CreateVersion7();
        var userA = Guid.CreateVersion7();
        var userB = Guid.CreateVersion7();
        
        var transaction = new Transaction()
        {
            Id = transactionId,
            UserId = userA,
            AccountId = accountId,
            CategoryId = categoryId,
            Description = "compra de uma camisa",
            Amount = 100m,
            Type = TransactionType.Expense,
            TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow),
        };

        context.Transactions.Add(transaction);
        await context.SaveChangesAsync();
        
        var service = CreateService(context);

        // When
        var result = await service.GetTransactionById(transactionId, userB);
    
        // Then
        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
        Assert.Equal(ResultStatus.NotFound, result.Status);

        var error = Assert.Single(result.Errors);
        Assert.Equal($"Transaction not found: {transactionId}.", error.Message);
    }

    [Fact]
    public async Task GetAllTransactions_WhenUserHasTransactions_ShouldReturnTransactions()
    {
        // Given
        await using var context = CreateDbContext();

        var accountA = Guid.CreateVersion7();
        var accountB = Guid.CreateVersion7();
        var categoryA = Guid.CreateVersion7();
        var categoryB = Guid.CreateVersion7();
        var userA = Guid.CreateVersion7();
        var userB = Guid.CreateVersion7();
        
        var transaction1 = new Transaction()
        {
            UserId = userA,
            AccountId = accountA,
            CategoryId = categoryA,
            Description = "compra1",
            Amount = 100m,
            Type = TransactionType.Expense,
            TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow),
        };

        var transaction2 = new Transaction()
        {
            UserId = userA,
            AccountId = accountA,
            CategoryId = categoryA,
            Description = "compra2",
            Amount = 500m,
            Type = TransactionType.Income,
            TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow),
        };

        var transaction3 = new Transaction()
        {
            UserId = userB,
            AccountId = accountB,
            CategoryId = categoryB,
            Description = "salario",
            Amount = 5000m,
            Type = TransactionType.Expense,
            TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow),
        };

        context.Transactions.AddRange(transaction1, transaction2, transaction3);
        await context.SaveChangesAsync();

        var service = CreateService(context);
    
        // When
        var result = await service.GetAllTransactions(userA);
    
        // Then
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(ResultStatus.Success, result.Status);

        var returnedTransactions = result.Data.ToList();
        Assert.Equal(2, returnedTransactions.Count);
        Assert.DoesNotContain(returnedTransactions, t => t.UserId == userB);

        var returnedTransaction1 = returnedTransactions.Single(t => t.Id == transaction1.Id);
        Assert.Equal(userA, returnedTransaction1.UserId);
        Assert.Equal(transaction1.AccountId, returnedTransaction1.AccountId);
        Assert.Equal(transaction1.CategoryId, returnedTransaction1.CategoryId);
        Assert.Equal(transaction1.Description, returnedTransaction1.Description);
        Assert.Equal(transaction1.Amount, returnedTransaction1.Amount);
        Assert.Equal(transaction1.Type, returnedTransaction1.Type);
        Assert.Equal(transaction1.TransactionDate, returnedTransaction1.TransactionDate);
        Assert.Equal(transaction1.CreatedAt, returnedTransaction1.CreatedAt);

        var returnedTransaction2 = returnedTransactions.Single(t => t.Id == transaction2.Id);
        Assert.Equal(userA, returnedTransaction2.UserId);
        Assert.Equal(transaction2.AccountId, returnedTransaction2.AccountId);
        Assert.Equal(transaction2.CategoryId, returnedTransaction2.CategoryId);
        Assert.Equal(transaction2.Description, returnedTransaction2.Description);
        Assert.Equal(transaction2.Amount, returnedTransaction2.Amount);
        Assert.Equal(transaction2.Type, returnedTransaction2.Type);
        Assert.Equal(transaction2.TransactionDate, returnedTransaction2.TransactionDate);
        Assert.Equal(transaction2.CreatedAt, returnedTransaction2.CreatedAt);

    }
}