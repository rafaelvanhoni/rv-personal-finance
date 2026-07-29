using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using RvPersonalFinance.Api.Domain.Entities;
using RvPersonalFinance.Api.Features.Accounts;
using RvPersonalFinance.Api.Infrastructure.Persistence;
using RvPersonalFinance.Api.Shared;
using RvPersonalFinance.Api.Domain.Enums;

namespace RvPersonalFinance.Tests.Features.Accounts;

public class AccountServiceTests
{
    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static AccountService CreateService(AppDbContext context)
    {
        return new AccountService(context, NullLogger<AccountService>.Instance, new CreateAccountValidator(), new UpdateAccountValidator());
    }

    [Fact]
    public async Task GetAccountById_WhenAccountBelongsToUser_ShouldReturnSuccess()
    {
        // Given
        await using var context = CreateDbContext();

        var userId = Guid.CreateVersion7();

        var account = new Account()
        {
            UserId = userId,
            Name = "Nubank",
            InitialBalance = 500m
        };

        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        var service = CreateService(context);
        
        // When
        var result = await service.GetAccountById(account.Id, userId);
    
        // Then
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(ResultStatus.Success, result.Status);

        Assert.Equal(account.Id, result.Data.Id);
        Assert.Equal(userId, result.Data.UserId);
        Assert.Equal(account.Name, result.Data.Name);
        Assert.Equal(account.InitialBalance, result.Data.InitialBalance);
        Assert.Equal(account.CreatedAt, result.Data.CreatedAt);
    }

    [Fact]
    public async Task GetAccountById_WhenAccountDoesNotExist_ShouldReturnFailure()
    {
        // Given
        await using var context = CreateDbContext();

        var accountId = Guid.CreateVersion7();
        var userId = Guid.CreateVersion7();


        var service = CreateService(context);

        // When
        var result = await service.GetAccountById(accountId, userId);
    
        // Then
        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
        Assert.Equal(ResultStatus.NotFound, result.Status);

        var error = Assert.Single(result.Errors);
        Assert.Equal($"Account not found: {accountId}.", error.Message);
    }

    [Fact]
    public async Task GetAccountById_WhenAccountBelongsToAnotherUser_ShouldReturnFailure()
    {
        // Given
        await using var context = CreateDbContext();

        var userA = Guid.CreateVersion7();
        var userB = Guid.CreateVersion7();

        var account = new Account()
        {
            UserId = userA,
            Name = "Santander",
            InitialBalance = 100m
        };

        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        // When
        var result = await service.GetAccountById(account.Id, userB);
    
        // Then
        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
        Assert.Equal(ResultStatus.NotFound, result.Status);

        var error = Assert.Single(result.Errors);
        Assert.Equal($"Account not found: {account.Id}.", error.Message);
    }

    [Fact]
    public async Task GetAllAccounts_WhenUserHasAccounts_ShouldReturnAccounts()
    {
        // Given
        await using var context = CreateDbContext();

        var userA = Guid.CreateVersion7();
        var userB = Guid.CreateVersion7();

        var account1 = new Account()
        {
            UserId = userA,
            Name = "Itau",
            InitialBalance = 1000.00m
        };
    
        var account2 = new Account()
        {
            UserId = userA,
            Name = "Santander",
            InitialBalance = 2000.00m
        };

        var account3 = new Account()
        {
            UserId = userB,
            Name = "Nubank",
            InitialBalance = 3000.00m
        };

        context.AddRange(account1, account2, account3);
        await context.SaveChangesAsync();

        var service = CreateService(context);
        
        // When
        var result = await service.GetAllAccounts(userA);

        // Then
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(ResultStatus.Success, result.Status);

        var returnedAccounts = result.Data.ToList();
        Assert.Equal(2, returnedAccounts.Count);
        Assert.DoesNotContain(returnedAccounts, a => a.UserId == userB);

        var returnedAccount1 = returnedAccounts.Single(a => a.Id == account1.Id);
        Assert.Equal(userA, returnedAccount1.UserId);
        Assert.Equal(account1.Name, returnedAccount1.Name);
        Assert.Equal(account1.InitialBalance, returnedAccount1.InitialBalance);
        Assert.Equal(account1.CreatedAt, returnedAccount1.CreatedAt);

        var returnedAccount2 = returnedAccounts.Single(a => a.Id == account2.Id);
        Assert.Equal(userA, returnedAccount2.UserId);
        Assert.Equal(account2.Name, returnedAccount2.Name);
        Assert.Equal(account2.InitialBalance, returnedAccount2.InitialBalance);
        Assert.Equal(account2.CreatedAt, returnedAccount2.CreatedAt);
    }

    [Fact]
    public async Task GetAllAccounts_WhenUserHasNoAccounts_ShouldReturnEmptyList()
    {
        // Given
        await using var context = CreateDbContext();

        var userId = Guid.CreateVersion7();

        var accountFromAnotherUser = new Account()
        {
            UserId = Guid.CreateVersion7(),
            Name = "Itaú",
            InitialBalance = 300m
        };

        context.Accounts.Add(accountFromAnotherUser);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        // When
        var result = await service.GetAllAccounts(userId);

        // Then
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(ResultStatus.Success, result.Status);
        Assert.Empty(result.Data);
    }

    [Fact]
    public async Task Create_WhenDataIsValid_ShouldCreateAccount()
    { 
        // Given
        await using var context = CreateDbContext();

        var userId = Guid.CreateVersion7();
        var dto = new CreateAccountDto() {
            Name = "Banco do Brasil",
            InitialBalance = 1000m
        };

        var service = CreateService(context);
    
        // When
        var result = await service.CreateAccount(dto, userId);
    
        // Then
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(ResultStatus.Created, result.Status);

        var account = result.Data;

        Assert.Equal(userId, account.UserId);
        Assert.Equal(dto.Name, account.Name);
        Assert.Equal(dto.InitialBalance, account.InitialBalance);

        var persistedAccount = await context.Accounts.SingleAsync(a => a.Id == account.Id);
        Assert.Equal(userId, persistedAccount.UserId);
        Assert.Equal(dto.Name, persistedAccount.Name);
        Assert.Equal(dto.InitialBalance, persistedAccount.InitialBalance);

    }

    [Fact]
    public async Task Create_WhenValidationFails_ShouldReturnFailure()
    {
        // Given
        await using var context = CreateDbContext();

        var userId = Guid.CreateVersion7();
        var dto = new CreateAccountDto()
        {
            Name = new string('A', 150),
            InitialBalance = 10.00m
        };

        var service = CreateService(context);
    
        // When
        var result = await service.CreateAccount(dto, userId);

        // Then
        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
        Assert.Equal(ResultStatus.ValidationError, result.Status);
        Assert.False(await context.Accounts.AnyAsync());
    }

    [Fact]
    public async Task Update_WhenAccountExists_ShouldUpdateAccount()
    {
        // Given
        await using var context = CreateDbContext();

        var accountId = Guid.CreateVersion7();
        var userId = Guid.CreateVersion7();
        var account = new Account()
        {
            Id = accountId,
            UserId = userId,
            Name = "Name",
            InitialBalance = 100m,
        };

        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        var accountDto = new UpdateAccountDto() 
        {
            Name = "Updated",
            InitialBalance = 500m,
        };

        var service = CreateService(context);
    
        // When
        var result = await service.UpdateAccount(accountId, accountDto, userId);
    
        // Then
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(ResultStatus.Success, result.Status);

        Assert.Equal(accountId, result.Data.Id);
        Assert.Equal(userId, result.Data.UserId);
        Assert.Equal(accountDto.Name, result.Data.Name);
        Assert.Equal(accountDto.InitialBalance, result.Data.InitialBalance);

        var updatedAccount = await context.Accounts.SingleAsync(a => a.Id == accountId);
        Assert.Equal(userId, updatedAccount.UserId);
        Assert.Equal(accountDto.Name, updatedAccount.Name);
        Assert.Equal(accountDto.InitialBalance, updatedAccount.InitialBalance);
    }

    [Fact]
    public async Task Update_WhenAccountBelongsToAnotherUser_ShouldReturnFailure()
    {
        // Given
        await using var context = CreateDbContext();

        var accountId = Guid.CreateVersion7();
        var userA = Guid.CreateVersion7();
        var userB = Guid.CreateVersion7();
        var account = new Account()
        {
            Id = accountId,
            UserId = userA,
            Name = "Bradesco",
            InitialBalance = 2500.00m,  
        };    

        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        var accountDto = new UpdateAccountDto()
        {
            Name = "Santander",
            InitialBalance = 500m,
        };

        var service = CreateService(context);

        // When
        var result = await service.UpdateAccount(accountId, accountDto, userB);
    
        // Then
        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
        Assert.Equal(ResultStatus.NotFound, result.Status);

        var originalAccount = await context.Accounts.SingleAsync(a => a.Id == accountId);
        Assert.Equal(userA, originalAccount.UserId);
        Assert.Equal("Bradesco", originalAccount.Name);
        Assert.Equal(2500m, originalAccount.InitialBalance);
    }

    [Fact]
    public async Task Update_WhenValidationFails_ShouldReturnFailure()
    {
        // Given
        await using var context = CreateDbContext();

        var accountId = Guid.CreateVersion7();
        var userId = Guid.CreateVersion7();
        var account = new Account()
        {
            Id = accountId,
            UserId = userId,
            Name = "Banrisul",
            InitialBalance = 1500.00m,  
        };    

        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        var accountDto = new UpdateAccountDto()
        {
            InitialBalance = 500m,
        };

        var service = CreateService(context);
    
        // When
        var result = await service.UpdateAccount(accountId, accountDto, userId);

        // Then
        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
        Assert.Equal(ResultStatus.ValidationError, result.Status);

        var originalAccount = await context.Accounts.SingleAsync(a => a.Id == accountId);
        Assert.Equal(userId, originalAccount.UserId);
        Assert.Equal("Banrisul", originalAccount.Name);
        Assert.Equal(1500m, originalAccount.InitialBalance);
    }

    [Fact]
    public async Task Delete_WhenAccountExists_ShouldDeleteAccount()
    {
        // Given
        await using var context = CreateDbContext();
    
        var accountId = Guid.CreateVersion7();
        var userId = Guid.CreateVersion7();
        var account = new Account()
        {
            Id = accountId,
            UserId = userId,
            Name = "Crefisa",
            InitialBalance = 5500m,  
        };

        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        var service = CreateService(context);
        
        // When
        var result = await service.DeleteAccount(accountId, userId);

        // Then
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(ResultStatus.Success, result.Status);

        Assert.Equal(userId, result.Data.UserId);
        Assert.Equal("Crefisa", result.Data.Name);
        Assert.Equal(5500m, result.Data.InitialBalance);

        var deletedAccount = await context.Accounts.SingleOrDefaultAsync(a => a.Id == accountId);
        Assert.Null(deletedAccount);
    }

    [Fact]
    public async Task Delete_WhenAccountBelongsToAnotherUser_ShouldReturnFailure()
    {
        // Given
        await using var context = CreateDbContext();

        var accountId = Guid.CreateVersion7();
        var userA = Guid.CreateVersion7();
        var userB = Guid.CreateVersion7();
        var account = new Account()
        {
            Id = accountId,
            UserId = userA,
            Name = "Crefisa",
            InitialBalance = 5500m,  
        };

        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        var service = CreateService(context);
        
        // When
        var result = await service.DeleteAccount(accountId, userB);
        
        // Then
        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
        Assert.Equal(ResultStatus.NotFound, result.Status);

        var existingAccount = await context.Accounts.SingleAsync(a => a.Id == accountId);
        Assert.Equal(userA, existingAccount.UserId);
        Assert.Equal("Crefisa", existingAccount.Name);
        Assert.Equal(5500m, existingAccount.InitialBalance);

    }

    [Fact]
    public async Task Delete_WhenAccountHasTransactions_ShouldReturnFailure()
    {
        // Given
        await using var context = CreateDbContext();
    
        var accountId = Guid.CreateVersion7();
        var userId = Guid.CreateVersion7();
        var account = new Account()
        {
            Id = accountId,
            UserId = userId,
            Name = "Crefisa",
            InitialBalance = 5500m,  
        };

        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        var transaction = new Transaction()
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            AccountId = accountId,
            CategoryId = Guid.CreateVersion7(),
            Description = "New Transaction",
            Amount = 1000m,
            Type = TransactionType.Expense
        };

        context.Transactions.Add(transaction);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        // When
        var result = await service.DeleteAccount(accountId, userId);

        // Then
        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
        Assert.Equal(ResultStatus.Conflict, result.Status);

        var existingAccount = await context.Accounts.SingleAsync(a => a.Id == accountId);
       
        Assert.Equal(userId, existingAccount.UserId);
        Assert.Equal("Crefisa", existingAccount.Name);
        Assert.Equal(5500m, existingAccount.InitialBalance);

    }
}