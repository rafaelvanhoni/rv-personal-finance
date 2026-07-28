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

        var errors = Assert.Single(result.Errors);
        Assert.Equal($"Account not found: {accountId}.", errors.Message);
    }

    [Fact]
    public async Task GetAccountById_WhenAccountBelongsToAnotherUser_ShouldReturnFailure()
    {
        // Given
        await using var context = CreateDbContext();

        var userId = Guid.CreateVersion7();

        var account = new Account()
        {
            UserId = userId,
            Name = "Santander",
            InitialBalance = 100m
        };

        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        // When
        var result = await service.GetAccountById(account.Id, Guid.CreateVersion7());
    
        // Then
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task GetAccounts_WhenUserHasAccounts_ShouldReturnAccounts()
    {
        // Given
        await using var context = CreateDbContext();

        var userId = Guid.CreateVersion7();

        var account = new Account()
        {
            UserId = userId,
            Name = "HSBC",
            InitialBalance = 1000.00m
        };
    
        context.Add(account);
        await context.SaveChangesAsync();

        var service = CreateService(context);
        
        // When
        var result = await service.GetAllAccounts(userId);

        // Then
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);

        var returnedAccount = Assert.Single(result.Data);
        Assert.Equal(account.Id, returnedAccount.Id);
        Assert.Equal(account.UserId, returnedAccount.UserId);
        Assert.Equal(account.Name, returnedAccount.Name);
        Assert.Equal(account.InitialBalance, returnedAccount.InitialBalance);
        Assert.Equal(account.CreatedAt, returnedAccount.CreatedAt);
    }

    [Fact]
    public async Task GetAccounts_WhenUserHasNoAccounts_ShouldReturnEmptyList()
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
        Assert.Empty(await context.Accounts.ToListAsync());
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

        var accountUpdated = await context.Accounts.SingleAsync(a => a.Id == accountId);
        Assert.Equal(accountId, accountUpdated.Id);
        Assert.Equal(userId, accountUpdated.UserId);
        Assert.Equal(accountDto.Name, accountUpdated.Name);
        Assert.Equal(accountDto.InitialBalance, accountUpdated.InitialBalance);
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

        var existingAccount = await context.Accounts.SingleOrDefaultAsync(a => a.Id == accountId);
        Assert.NotNull(existingAccount);
       
        Assert.Equal(userId, existingAccount.UserId);
        Assert.Equal("Crefisa", existingAccount.Name);
        Assert.Equal(5500m, existingAccount.InitialBalance);

    }
}