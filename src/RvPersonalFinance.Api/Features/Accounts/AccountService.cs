using System.Linq;
using Microsoft.EntityFrameworkCore;
using RvPersonalFinance.Api.Domain.Entities;
using RvPersonalFinance.Api.Infrastructure.Persistence;
using RvPersonalFinance.Api.Shared;

namespace RvPersonalFinance.Api.Features.Accounts;

public class AccountService
{

    private readonly AppDbContext _context;
    private readonly ILogger<AccountService> _logger;

    public AccountService(AppDbContext context, ILogger<AccountService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<OperationResult<Account>> GetAccountById(Guid id)
    {
        var account = await _context.Accounts.FirstOrDefaultAsync(account => account.Id == id);

        if (account is null)
        {
            _logger.LogWarning("Account not found: {AccountId}", id);
            return new OperationResult<Account>()
            {
                Status = ResultStatus.NotFound,
                Message = $"Account not found: {id}",
            };
        }

        _logger.LogInformation("Account retrieved: {AccountId}", account.Id);
        return new OperationResult<Account>()
        {
            Data = account
        };
    }

    public async Task<OperationResult<IEnumerable<Account>>> GetAll() 
    {
        var accounts = await _context.Accounts.ToListAsync();
        _logger.LogInformation(
            "Accounts retrieved: {Count}",
            accounts.Count);

        return new OperationResult<IEnumerable<Account>>()
        {
            Data = accounts
        };
    }

    public async Task<OperationResult<Account>> CreateAccount (CreateAccountDto dto)
    {
        var account = new Account
        {
            UserId = dto.UserId,
            Name = dto.Name,
            InitialBalance = dto.InitialBalance,
        };

        await _context.Accounts.AddAsync(account);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Account created: {AccountId}", account.Id);

        return new OperationResult<Account>()
        {
          Status = ResultStatus.Created,
          Data = account  
        };
    }

    public async Task<OperationResult<Account>> UpdateAccount (Guid id, UpdateAccountDto dto)
    {

        var result = await GetAccountById(id);

        if (!result.IsSuccess)
        {
            return new OperationResult<Account>()
            {
                Status = result.Status,
                Message = result.Message,
                Data = result.Data
            };
        }

        var account = result.Data!;
        account.UserId = dto.UserId;
        account.Name = dto.Name;
        account.InitialBalance = dto.InitialBalance;

        _context.Accounts.Update(account);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Account updated: {AccountId}", account.Id); 

        return new OperationResult<Account>() { Data = account };

    }

    public async Task<OperationResult<Account>> DeleteAccount(Guid id)
    {
        var result = await GetAccountById(id);
        if (!result.IsSuccess)
        {
            return new OperationResult<Account>()
            {
                Status = result.Status,
                Message = result.Message,
                Data = result.Data
            };
        }   

        var account = result.Data!;

        _context.Accounts.Remove(account);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Account deleted: {AccountId}", id);

        return new OperationResult<Account>() { Data = account };   
    }
}