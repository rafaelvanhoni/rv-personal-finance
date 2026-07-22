using FluentValidation;
using Microsoft.EntityFrameworkCore;
using RvPersonalFinance.Api.Domain.Entities;
using RvPersonalFinance.Api.Infrastructure.Persistence;
using RvPersonalFinance.Api.Shared;
using RvPersonalFinance.Api.Domain.Enums;

namespace RvPersonalFinance.Api.Features.Accounts;

public class AccountService
{

    private readonly AppDbContext _context;
    private readonly ILogger<AccountService> _logger;
    private readonly IValidator<CreateAccountDto> _createValidator;
    private readonly IValidator<UpdateAccountDto> _updateValidator;

    public AccountService(AppDbContext context, ILogger<AccountService> logger, IValidator<CreateAccountDto> createValidator, IValidator<UpdateAccountDto> updateValidator)
    {
        _context = context;
        _logger = logger;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    private async Task<Account?> GetAccountByIdAsync(Guid id)
    {
        var account = await _context.Accounts.FirstOrDefaultAsync(account => account.Id == id);
        return account;
    }
    public async Task<OperationResult<AccountResponseDto>> GetAccountById(Guid id)
    {
        var account = await GetAccountByIdAsync(id);

        if (account is null)
        {
            _logger.LogWarning("Account not found: {AccountId}.", id);

            return OperationResult<AccountResponseDto>.NotFound($"Account not found: {id}.");
        }

        var accountResponseDto = ToResponseDto(account);

        _logger.LogInformation("Account retrieved: {AccountId}.", account.Id);
        return OperationResult<AccountResponseDto>.Success(accountResponseDto);
    }

    public async Task<OperationResult<IEnumerable<AccountResponseDto>>> GetAllAccounts() 
    {
        var accounts = await _context.Accounts.ToListAsync();
        _logger.LogInformation("Accounts retrieved: {Count}.", accounts.Count);

        var accountResponseDtos = accounts
            .Select(ToResponseDto)
            .ToList();

        return OperationResult<IEnumerable<AccountResponseDto>>.Success(accountResponseDtos);
    }

    public async Task<OperationResult<AccountResponseDto>> CreateAccount (CreateAccountDto dto)
    {

        var validation = await _createValidator.ValidateAsync(dto);
        if (!validation.IsValid)
        {
            var errors = validation.Errors.ToOperationErrors();

            foreach (var item in errors)
            {
                _logger.LogWarning("Validation failed for CreateAccount. ErrorMessage: {ErrorMessage}. Property: {Property}.", item.Message, item.Property);
            }
            return OperationResult<AccountResponseDto>.ValidationError(errors);
        }        

        var account = new Account
        {
            UserId = dto.UserId,
            Name = dto.Name.Trim(),
            InitialBalance = dto.InitialBalance,
        };

        await _context.Accounts.AddAsync(account);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Account created: {AccountId}.", account.Id);

        var accountResponseDto = ToResponseDto(account);

        return OperationResult<AccountResponseDto>.Created(accountResponseDto);
    }

    public async Task<OperationResult<AccountResponseDto>> UpdateAccount (Guid id, UpdateAccountDto dto)
    {
        var account = await GetAccountByIdAsync(id);
        if (account is null)
        {
            _logger.LogWarning("Account not found: {AccountId}.", id);
            return OperationResult<AccountResponseDto>.NotFound($"Account not found: {id}.");
        }

        var validation = await _updateValidator.ValidateAsync(dto);
        if (!validation.IsValid)
        {
            var errors = validation.Errors.ToOperationErrors();
            foreach (var item in errors)
            {
                _logger.LogWarning("Validation failed for UpdateAccount {AccountId}. ErrorMessage: {ErrorMessage}. Property: {Property}.", id, item.Message, item.Property);                
            }
            return OperationResult<AccountResponseDto>.ValidationError(errors);
        }

        account.UserId = dto.UserId;
        account.Name = dto.Name.Trim();
        account.InitialBalance = dto.InitialBalance;

        _context.Accounts.Update(account);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Account updated: {AccountId}.", account.Id); 

        var accountResponseDto = ToResponseDto(account);

        return OperationResult<AccountResponseDto>.Success(accountResponseDto);
    }

    public async Task<OperationResult<AccountResponseDto>> DeleteAccount(Guid id)
    {
        var account = await GetAccountByIdAsync(id);
        if (account is null)
        {
            _logger.LogWarning("Account not found: {AccountId}.", id);
            return OperationResult<AccountResponseDto>.NotFound($"Account not found: {id}.");
        }

        var accountResponseDto = ToResponseDto(account);

        _context.Accounts.Remove(account);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Account deleted: {AccountId}.", id);

        return OperationResult<AccountResponseDto>.Success(accountResponseDto);
    }

    public async Task<OperationResult<AccountBalanceDto>> CalculateBalance(Guid id)
    {
        var account = await GetAccountByIdAsync(id);

        if (account is null)
        {
            _logger.LogWarning("Account not found: {AccountId}.", id);
            return OperationResult<AccountBalanceDto>.NotFound($"Account not found: {id}.");
        }

        var initialBalance = account.InitialBalance;
        var totalIncome = await _context.Transactions
                            .Where(t => t.AccountId == account.Id && t.UserId == account.UserId && t.Type == TransactionType.Income)
                            .SumAsync(t => t.Amount);
        var totalExpense = await _context.Transactions
                            .Where(t => t.AccountId == account.Id && t.UserId == account.UserId && t.Type == TransactionType.Expense)
                            .SumAsync(t => t.Amount);
        var currentBalance = initialBalance + totalIncome - totalExpense;

        var balanceDto = new AccountBalanceDto()
        {
            InitialBalance = initialBalance,
            TotalIncome = totalIncome,
            TotalExpense = totalExpense,
            CurrentBalance = currentBalance
        };        

        return OperationResult<AccountBalanceDto>.Success(balanceDto);
    }

    private static AccountResponseDto ToResponseDto(Account account)
    {
        return new AccountResponseDto()
        {
            Id = account.Id,
            UserId = account.UserId,
            Name = account.Name,
            InitialBalance = account.InitialBalance,
            CreatedAt = account.CreatedAt
        };
    }
}