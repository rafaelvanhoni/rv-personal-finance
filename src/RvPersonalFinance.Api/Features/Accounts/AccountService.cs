using FluentValidation;
using Microsoft.EntityFrameworkCore;
using RvPersonalFinance.Api.Domain.Entities;
using RvPersonalFinance.Api.Infrastructure.Persistence;
using RvPersonalFinance.Api.Shared;

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
            _logger.LogWarning("Account not found: {AccountId}", id);
            return new OperationResult<AccountResponseDto>()
            {
                Status = ResultStatus.NotFound,
                Message = $"Account not found: {id}",
            };
        }

        var accountResponseDto = ToResponseDto(account);

        _logger.LogInformation("Account retrieved: {AccountId}", account.Id);
        return new OperationResult<AccountResponseDto>()
        {
            Data = accountResponseDto
        };
    }

    public async Task<OperationResult<IEnumerable<AccountResponseDto>>> GetAllAccounts() 
    {
        var accounts = await _context.Accounts.ToListAsync();
        _logger.LogInformation(
            "Accounts retrieved: {Count}",
            accounts.Count);

        var accountResponseDtos = accounts
            .Select(ToResponseDto)
            .ToList();

        return new OperationResult<IEnumerable<AccountResponseDto>>()
        {
            Data = accountResponseDtos
        };
    }

    public async Task<OperationResult<AccountResponseDto>> CreateAccount (CreateAccountDto dto)
    {

        var validation = await _createValidator.ValidateAsync(dto);
        if (!validation.IsValid)
        {
            _logger.LogWarning("Validation failed for CreateAccount: {Error}", validation.Errors.First().ErrorMessage);
            return new OperationResult<AccountResponseDto>()
            {
                Status = ResultStatus.ValidationError,
                Message = validation.Errors.First().ErrorMessage,
            };
        }        

        var account = new Account
        {
            UserId = dto.UserId,
            Name = dto.Name.Trim(),
            InitialBalance = dto.InitialBalance,
        };

        await _context.Accounts.AddAsync(account);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Account created: {AccountId}", account.Id);

        var accountResponseDto = ToResponseDto(account);

        return new OperationResult<AccountResponseDto>()
        {
          Status = ResultStatus.Created,
          Data = accountResponseDto  
        };
    }

    public async Task<OperationResult<AccountResponseDto>> UpdateAccount (Guid id, UpdateAccountDto dto)
    {
        var account = await GetAccountByIdAsync(id);
        if (account is null)
        {
            _logger.LogWarning("Account not found: {AccountId}", id);
            return new OperationResult<AccountResponseDto>()
            {
                Status = ResultStatus.NotFound,
                Message = $"Account not found: {id}",
            };            
        }

        var validation = await _updateValidator.ValidateAsync(dto);
        if (!validation.IsValid)
        {
            _logger.LogWarning("Validation failed for UpdateAccount {AccountId}. Error: {Error}", id, validation.Errors.First().ErrorMessage);
            return new OperationResult<AccountResponseDto>()
            {
                Status = ResultStatus.ValidationError,
                Message = validation.Errors.First().ErrorMessage,
            };
        }

        account.UserId = dto.UserId;
        account.Name = dto.Name.Trim();
        account.InitialBalance = dto.InitialBalance;

        _context.Accounts.Update(account);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Account updated: {AccountId}", account.Id); 

        var accountResponseDto = ToResponseDto(account);

        return new OperationResult<AccountResponseDto>() { Data = accountResponseDto };
    }

    public async Task<OperationResult<AccountResponseDto>> DeleteAccount(Guid id)
    {
        var account = await GetAccountByIdAsync(id);
        if (account is null)
        {
            _logger.LogWarning("Account not found: {AccountId}", id);
            return new OperationResult<AccountResponseDto>()
            {
                Status = ResultStatus.NotFound,
                Message = $"Account not found: {id}",
            };            
        }

        var accountResponseDto = ToResponseDto(account);

        _context.Accounts.Remove(account);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Account deleted: {AccountId}", id);

        return new OperationResult<AccountResponseDto>() { Data = accountResponseDto };   
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