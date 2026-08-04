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
            Type = TransactionType.Expense,
            TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow),
        };

        var transaction3 = new Transaction()
        {
            UserId = userB,
            AccountId = accountB,
            CategoryId = categoryB,
            Description = "salario",
            Amount = 5000m,
            Type = TransactionType.Income,
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

    [Fact]
    public async Task GetAllTransactions_WhenUserHasNoTransactions_ShouldReturnEmptyList()
    {
        // Given
        await using var context = CreateDbContext();

        var userA = Guid.CreateVersion7();
        var userB = Guid.CreateVersion7();
        var accountId = Guid.CreateVersion7();
        var categoryId = Guid.CreateVersion7();
        var transaction = new Transaction()
        {
            UserId = userB,
            AccountId = accountId,
            CategoryId = categoryId,
            Description = "compra 123",
            Amount = 1000m,
            Type = TransactionType.Expense,
            TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow),
        };

        context.Transactions.Add(transaction);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        // When
        var result = await service.GetAllTransactions(userA);

        // Then
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(ResultStatus.Success, result.Status);
        Assert.Empty(result.Data);
    }

    [Fact]
    public async Task Create_WhenDataIsValid_ShouldCreateTransaction()
    {
        // Given
        await using var context = CreateDbContext();

        var userId = Guid.CreateVersion7();
        var accountId = Guid.CreateVersion7();
        var categoryId = Guid.CreateVersion7();

        var user = new User() 
        { 
            Id = userId
        };
        var account = new Account() 
        { 
            Id = accountId, 
            UserId = userId, 
            Name = "Banco do Brasil", 
            InitialBalance = 1000m 
        };
        var category = new Category() 
        { 
            Id = categoryId, 
            UserId = userId, 
            Name = "Alimentação" 
        };

        context.Users.Add(user);
        context.Accounts.Add(account);
        context.Categories.Add(category);

        await context.SaveChangesAsync();

        var dto = new CreateTransactionDto() {
            AccountId = accountId,
            CategoryId = categoryId,
            Description = "Compra",
            Amount = 1500m,
            Type = TransactionType.Expense,
            TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow),
        };

        var service = CreateService(context);
    
        // When
        var result = await service.CreateTransaction(dto, userId);
    
        // Then
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(ResultStatus.Created, result.Status);

        var transaction = result.Data;

        Assert.Equal(userId, transaction.UserId);
        Assert.Equal(accountId, transaction.AccountId);
        Assert.Equal(categoryId, transaction.CategoryId);
        Assert.Equal(dto.Description, transaction.Description);
        Assert.Equal(dto.Amount, transaction.Amount);
        Assert.Equal(dto.Type, transaction.Type);
        Assert.Equal(dto.TransactionDate, transaction.TransactionDate);

        var persistedTransaction = await context.Transactions.SingleAsync(t => t.Id == transaction.Id);
        Assert.Equal(userId, persistedTransaction.UserId);
        Assert.Equal(accountId, persistedTransaction.AccountId);
        Assert.Equal(categoryId, persistedTransaction.CategoryId);
        Assert.Equal(dto.Description, persistedTransaction.Description);
        Assert.Equal(dto.Amount, persistedTransaction.Amount);
        Assert.Equal(dto.Type, persistedTransaction.Type);
        Assert.Equal(dto.TransactionDate, persistedTransaction.TransactionDate);
    }

    [Fact]
    public async Task Create_WhenValidationFails_ShouldReturnFailure()
    {
        // Given
        await using var context = CreateDbContext();

        var userId = Guid.CreateVersion7();
        var accountId = Guid.CreateVersion7();
        var categoryId = Guid.CreateVersion7();

        var dto = new CreateTransactionDto()
        {
            AccountId = accountId,
            CategoryId = categoryId,
            Description = string.Empty,
            Amount = 100m,
            Type = TransactionType.Expense,
            TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        var service = CreateService(context);
    
        // When
        var result = await service.CreateTransaction(dto, userId);
    
        // Then
        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
        Assert.Equal(ResultStatus.ValidationError, result.Status);
        Assert.False(await context.Transactions.AnyAsync());

    }

    [Fact]
    public async Task Create_WhenReferencesDoNotExist_ShouldReturnFailure()
    {
        // Given
        await using var context = CreateDbContext();

        var userId = Guid.CreateVersion7();
        var accountId = Guid.CreateVersion7();
        var categoryId = Guid.CreateVersion7();

        var dto = new CreateTransactionDto()
        {
            AccountId = accountId,
            CategoryId = categoryId,
            Description = "Compra",
            Amount = 200m,
            Type = TransactionType.Expense,
            TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow)
        };        

        var service = CreateService(context);


        // When
        var result = await service.CreateTransaction(dto, userId);
    
        // Then
        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
        Assert.Equal(ResultStatus.ValidationError, result.Status);
        
        Assert.Equal(3, result.Errors.Count);

        var userError = result.Errors.Single(e => e.Property == nameof(Transaction.UserId));
        Assert.Equal($"User not found: {userId}.", userError.Message);
        var accountError = result.Errors.Single(e => e.Property == nameof(Transaction.AccountId));
        Assert.Equal($"Account not found: {accountId}.", accountError.Message);
        var categoryError = result.Errors.Single(e => e.Property == nameof(Transaction.CategoryId));
        Assert.Equal($"Category not found: {categoryId}.", categoryError.Message);

        Assert.False(await context.Transactions.AnyAsync());
    }

    [Fact]
    public async Task Create_WhenAccountBelongsToAnotherUser_ShouldReturnFailure()
    {
        // Given
        await using var context = CreateDbContext();

        var userA = Guid.CreateVersion7();
        var userB = Guid.CreateVersion7();
        var accountId = Guid.CreateVersion7();
        var categoryId = Guid.CreateVersion7();

        var user = new User() 
        { 
            Id = userA
        };
        var accountFromAnotherUser = new Account() 
        { 
            Id = accountId, 
            UserId = userB, 
            Name = "Conta do userB", 
            InitialBalance = 500m 
        };
        var category = new Category() 
        { 
            Id = categoryId, 
            UserId = userA, 
            Name = "Alimentação" 
        };

        context.Users.Add(user);
        context.Accounts.Add(accountFromAnotherUser);
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var dto = new CreateTransactionDto()
        {
            AccountId = accountId,
            CategoryId = categoryId,
            Description = "Compra",
            Amount = 100m,
            Type = TransactionType.Expense,
            TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        var service = CreateService(context);

        // When
        var result = await service.CreateTransaction(dto, userA);

        // Then
        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
        Assert.Equal(ResultStatus.ValidationError, result.Status);
        Assert.False(await context.Transactions.AnyAsync());

        var error = Assert.Single(result.Errors);
        Assert.Equal(nameof(Transaction.AccountId), error.Property);
        Assert.Equal($"Account not found: {accountId}.", error.Message);
    }

    [Fact]
    public async Task Create_WhenCategoryBelongsToAnotherUser_ShouldReturnFailure()
    {
        // Given
        await using var context = CreateDbContext();

        var userA = Guid.CreateVersion7();
        var userB = Guid.CreateVersion7();
        var accountId = Guid.CreateVersion7();
        var categoryId = Guid.CreateVersion7();

        var user = new User() 
        { 
            Id = userA
        };
        var account = new Account() 
        { 
            Id = accountId, 
            UserId = userA, 
            Name = "Conta", 
            InitialBalance = 500m 
        };
        var categoryFromAnotherUser = new Category() 
        { 
            Id = categoryId, 
            UserId = userB, 
            Name = "Alimentação" 
        };

        context.Users.Add(user);
        context.Accounts.Add(account);
        context.Categories.Add(categoryFromAnotherUser);
        await context.SaveChangesAsync();

        var dto = new CreateTransactionDto()
        {
            AccountId = accountId,
            CategoryId = categoryId,
            Description = "Compra",
            Amount = 100m,
            Type = TransactionType.Expense,
            TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        var service = CreateService(context);

        // When
        var result = await service.CreateTransaction(dto, userA);

        // Then
        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
        Assert.Equal(ResultStatus.ValidationError, result.Status);
        Assert.False(await context.Transactions.AnyAsync());

        var error = Assert.Single(result.Errors);
        Assert.Equal(nameof(Transaction.CategoryId), error.Property);
        Assert.Equal($"Category not found: {categoryId}.", error.Message);
    }

    [Fact]
    public async Task Update_WhenTransactionExists_ShouldUpdateTransaction()
    {
        // Given
        await using var context = CreateDbContext();

        var transactionId = Guid.CreateVersion7();
        var userId = Guid.CreateVersion7();
        var accountId = Guid.CreateVersion7();
        var categoryId = Guid.CreateVersion7();

        var user = new User()
        { 
            Id = userId 
        };
        var account = new Account() 
        { 
            Id = accountId, 
            UserId = userId, 
            Name = "Banco do Brasil", 
            InitialBalance = 1000m 
        };
        var category = new Category() 
        { 
            Id = categoryId, 
            UserId = userId, 
            Name = "Alimentação" 
        };

        var transaction = new Transaction()
        {
            Id = transactionId,
            UserId = userId,
            AccountId = accountId,
            CategoryId = categoryId,
            Description = "Compra",
            Amount = 100m,
            Type = TransactionType.Expense,
            TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        context.Users.Add(user);
        context.Accounts.Add(account);
        context.Categories.Add(category);
        context.Transactions.Add(transaction);
        await context.SaveChangesAsync();

        var dto = new UpdateTransactionDto()
        {
            AccountId = accountId,
            CategoryId = categoryId,
            Description = "Another Description",
            Amount = 250m,
            Type = TransactionType.Income,
            TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1))
        };

        var service = CreateService(context);
        
        // When
        var result = await service.UpdateTransaction(transactionId, dto, userId);
    
        // Then
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(ResultStatus.Success, result.Status);

        Assert.Equal(transactionId, result.Data.Id);
        Assert.Equal(userId, result.Data.UserId);
        Assert.Equal(dto.AccountId, result.Data.AccountId);
        Assert.Equal(dto.CategoryId, result.Data.CategoryId);
        Assert.Equal(dto.Description, result.Data.Description);
        Assert.Equal(dto.Amount, result.Data.Amount);
        Assert.Equal(dto.Type, result.Data.Type);
        Assert.Equal(dto.TransactionDate, result.Data.TransactionDate);

        var updatedTransaction = await context.Transactions.SingleAsync(t => t.Id == transactionId);

        Assert.Equal(userId, updatedTransaction.UserId);
        Assert.Equal(dto.AccountId, updatedTransaction.AccountId);
        Assert.Equal(dto.CategoryId, updatedTransaction.CategoryId);
        Assert.Equal(dto.Description, updatedTransaction.Description);
        Assert.Equal(dto.Amount, updatedTransaction.Amount);
        Assert.Equal(dto.Type, updatedTransaction.Type);
        Assert.Equal(dto.TransactionDate, updatedTransaction.TransactionDate);
    }

    [Fact]
    public async Task Update_WhenValidationFails_ShouldReturnFailure()
    {
        // Given
        await using var context = CreateDbContext();

        var transactionId = Guid.CreateVersion7();
        var userId = Guid.CreateVersion7();
        var accountId = Guid.CreateVersion7();
        var categoryId = Guid.CreateVersion7();

        var user = new User()
        { 
            Id = userId 
        };
        var account = new Account() 
        { 
            Id = accountId, 
            UserId = userId, 
            Name = "Santander",
            InitialBalance = 1000m
        };
        var category = new Category() 
        { 
            Id = categoryId, 
            UserId = userId, 
            Name = "Alimentação" 
        };

        var transactionDate = DateOnly.FromDateTime(DateTime.UtcNow);

        var transaction = new Transaction()
        {
            Id = transactionId,
            UserId = userId,
            AccountId = accountId,
            CategoryId = categoryId,
            Description = "Compra",
            Amount = 3000m,
            Type = TransactionType.Expense,
            TransactionDate = transactionDate
        };

        context.Users.Add(user);
        context.Accounts.Add(account);
        context.Categories.Add(category);
        context.Transactions.Add(transaction);
        await context.SaveChangesAsync();

        var dto = new UpdateTransactionDto()
        {
            AccountId = accountId,
            CategoryId = categoryId,
            Description = new string('T', 101),
            Amount = 250m,
            Type = TransactionType.Income,
            TransactionDate = transactionDate.AddDays(-1)
        };

        var service = CreateService(context);

        // When
        var result = await service.UpdateTransaction(transactionId, dto, userId);    
   
        // Then
        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
        Assert.Equal(ResultStatus.ValidationError, result.Status);

        var originalTransaction = await context.Transactions.SingleAsync(t => t.Id == transactionId);
        Assert.Equal(userId, originalTransaction.UserId);
        Assert.Equal(accountId, originalTransaction.AccountId);
        Assert.Equal(categoryId, originalTransaction.CategoryId);
        Assert.Equal("Compra", originalTransaction.Description);
        Assert.Equal(3000m, originalTransaction.Amount);
        Assert.Equal(TransactionType.Expense, originalTransaction.Type);
        Assert.Equal(transactionDate, originalTransaction.TransactionDate);

        var error = Assert.Single(result.Errors);
        Assert.Equal(nameof(UpdateTransactionDto.Description), error.Property);
        Assert.Equal("Description must not exceed 100 characters.", error.Message);
    }

    [Fact]
    public async Task Update_WhenTransactionBelongsToAnotherUser_ShouldReturnFailure()
    {
        // Given
        await using var context = CreateDbContext();

        var transactionId = Guid.CreateVersion7();
        var userA = Guid.CreateVersion7();
        var userB = Guid.CreateVersion7();
        var accountId = Guid.CreateVersion7();
        var categoryId = Guid.CreateVersion7();
        var transactionDate = DateOnly.FromDateTime(DateTime.UtcNow);

        var transaction = new Transaction()
        {
            Id = transactionId,
            UserId = userA,
            AccountId = accountId,
            CategoryId = categoryId,
            Description = "Compra",
            Amount = 3000m,
            Type = TransactionType.Expense,
            TransactionDate = transactionDate
        };

        context.Transactions.Add(transaction);
        await context.SaveChangesAsync();

        var dto = new UpdateTransactionDto()
        {
            AccountId = accountId,
            CategoryId = categoryId,
            Description = "Outra descrição",
            Amount = 250m,
            Type = TransactionType.Income,
            TransactionDate = transactionDate.AddDays(-1)
        };

        var service = CreateService(context);

        // When
        var result = await service.UpdateTransaction(transactionId, dto, userB);

        // Then
        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
        Assert.Equal(ResultStatus.NotFound, result.Status);

        var error = Assert.Single(result.Errors);
        Assert.Equal($"Transaction not found: {transactionId}.", error.Message);
        
        var originalTransaction = await context.Transactions.SingleAsync(t => t.Id == transactionId);

        Assert.Equal(userA, originalTransaction.UserId);
        Assert.Equal(accountId, originalTransaction.AccountId);
        Assert.Equal(categoryId, originalTransaction.CategoryId);
        Assert.Equal("Compra", originalTransaction.Description);
        Assert.Equal(3000m, originalTransaction.Amount);
        Assert.Equal(TransactionType.Expense, originalTransaction.Type);
        Assert.Equal(transactionDate, originalTransaction.TransactionDate);
    }

    [Fact]
    public async Task Update_WhenReferencesDoNotExist_ShouldReturnFailure()
    {
        // Given
        await using var context = CreateDbContext();

        var transactionId = Guid.CreateVersion7();
        var userId = Guid.CreateVersion7();
        var accountId = Guid.CreateVersion7();
        var categoryId = Guid.CreateVersion7();
        var transactionDate = DateOnly.FromDateTime(DateTime.UtcNow);

        var transaction = new Transaction()
        {
            Id = transactionId,
            UserId = userId,
            AccountId = accountId,
            CategoryId = categoryId,
            Description = "Compra",
            Amount = 3000m,
            Type = TransactionType.Expense,
            TransactionDate = transactionDate
        };

        context.Transactions.Add(transaction);
        await context.SaveChangesAsync();

        var dto = new UpdateTransactionDto()
        {
            AccountId = accountId,
            CategoryId = categoryId,
            Description = "Outra descrição",
            Amount = 250m,
            Type = TransactionType.Income,
            TransactionDate = transactionDate.AddDays(-1)
        };

        var service = CreateService(context);

        // When
        var result = await service.UpdateTransaction(transactionId, dto, userId);

        // Then
        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
        Assert.Equal(ResultStatus.ValidationError, result.Status);

        Assert.Equal(3, result.Errors.Count);

        var userError = result.Errors.Single(e => e.Property == nameof(Transaction.UserId));
        Assert.Equal($"User not found: {userId}.", userError.Message);
        var accountError = result.Errors.Single(e => e.Property == nameof(Transaction.AccountId));
        Assert.Equal($"Account not found: {accountId}.", accountError.Message);
        var categoryError = result.Errors.Single(e => e.Property == nameof(Transaction.CategoryId));
        Assert.Equal($"Category not found: {categoryId}.", categoryError.Message);

        var originalTransaction = await context.Transactions.SingleAsync(t => t.Id == transactionId);
        Assert.Equal(userId, originalTransaction.UserId);
        Assert.Equal(accountId, originalTransaction.AccountId);
        Assert.Equal(categoryId, originalTransaction.CategoryId);
        Assert.Equal("Compra", originalTransaction.Description);
        Assert.Equal(3000m, originalTransaction.Amount);
        Assert.Equal(TransactionType.Expense, originalTransaction.Type);
        Assert.Equal(transactionDate, originalTransaction.TransactionDate);
    }

    [Fact]
    public async Task Delete_WhenTransactionExists_ShouldDeleteTransaction()
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
            Description = "compra de uma bermuda",
            Amount = 300m,
            Type = TransactionType.Expense,
            TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow),
        };

        context.Transactions.Add(transaction);
        await context.SaveChangesAsync();

        var service = CreateService(context);
    
        // When
        var result = await service.DeleteTransaction(transactionId, userId);
    
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

        Assert.False(await context.Transactions.AnyAsync(t => t.Id == transactionId));
    }

    [Fact]
    public async Task Delete_WhenTransactionBelongsToAnotherUser_ShouldReturnFailure()
    {
        // Given
        await using var context = CreateDbContext();

        var transactionId = Guid.CreateVersion7();
        var userA = Guid.CreateVersion7();
        var userB = Guid.CreateVersion7();
        var accountId = Guid.CreateVersion7();
        var categoryId = Guid.CreateVersion7();

        var transaction = new Transaction()
        {
            Id = transactionId,
            UserId = userA,
            AccountId = accountId,
            CategoryId = categoryId,
            Description = "compra",
            Amount = 400m,
            Type = TransactionType.Expense,
            TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow),
        };

        context.Transactions.Add(transaction);
        await context.SaveChangesAsync();
        

        var service = CreateService(context);

        // When
        var result = await service.DeleteTransaction(transactionId, userB);
    
        // Then
        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
        Assert.Equal(ResultStatus.NotFound, result.Status);

        var error = Assert.Single(result.Errors);
        Assert.Equal($"Transaction not found: {transactionId}.", error.Message); 

        var existingTransaction = await context.Transactions.SingleAsync(t => t.Id == transactionId);

        Assert.Equal(userA, existingTransaction.UserId);
        Assert.Equal(accountId, existingTransaction.AccountId);
        Assert.Equal(categoryId, existingTransaction.CategoryId);
        Assert.Equal("compra", existingTransaction.Description);
        Assert.Equal(400m, existingTransaction.Amount);
        Assert.Equal(TransactionType.Expense, existingTransaction.Type);               
    }
}