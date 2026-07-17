using Microsoft.EntityFrameworkCore;
using RvPersonalFinance.Api.Infrastructure.Persistence;
using RvPersonalFinance.Api.Domain.Entities;
using RvPersonalFinance.Api.Shared;
using FluentValidation;

namespace RvPersonalFinance.Api.Features.Transactions;

public class TransactionService
{
    private readonly AppDbContext _context;
    private readonly ILogger<TransactionService> _logger;
    private readonly IValidator<CreateTransactionDto> _createValidator;
    private readonly IValidator<UpdateTransactionDto> _updateValidator;
    
    public TransactionService(AppDbContext context, ILogger<TransactionService> logger, IValidator<CreateTransactionDto> createValidator, IValidator<UpdateTransactionDto> updateValidator)
    {
        _context = context;
        _logger = logger;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }   

    private async Task<Transaction?> GetTransactionByIdAsync(Guid id)
    {
        var transaction = await _context.Transactions.FirstOrDefaultAsync(transaction => transaction.Id == id);
        return transaction;        
    }
    public async Task<OperationResult<TransactionResponseDto>> GetTransactionById(Guid id)
    {
        var transaction = await GetTransactionByIdAsync(id);
        if (transaction is null)
        {
            _logger.LogWarning("Transaction not found: {TransactionId}.", id);
            return OperationResult<TransactionResponseDto>.NotFound($"Transaction not found: {id}.");
        }

        var transactionResponseDto = ToResponseDto(transaction);

        _logger.LogInformation("Transaction retrieved: {TransactionId}.", transaction.Id);
        return OperationResult<TransactionResponseDto>.Success(transactionResponseDto);

    }

    public async Task<OperationResult<IEnumerable<TransactionResponseDto>>> GetAllTransactions()
    {
        var transactions = await _context.Transactions.ToListAsync();

        _logger.LogInformation("Transactions retrieved: {Count}.", transactions.Count);

        var transactionResponseDtos = transactions
            .Select(ToResponseDto)
            .ToList();

        return OperationResult<IEnumerable<TransactionResponseDto>>.Success(transactionResponseDtos);
    }

    public async Task<OperationResult<TransactionResponseDto>> CreateTransaction(CreateTransactionDto dto)
    {
        var validation = await _createValidator.ValidateAsync(dto);
        if (!validation.IsValid)
        {
            var errors = validation.Errors.ToOperationErrors();
            foreach (var item in errors)
            {
                _logger.LogWarning("Validation failed for CreateTransaction. ErrorMessage: {ErrorMessage}. Property: {Property}.", item.Message, item.Property);
            }

            return OperationResult<TransactionResponseDto>.ValidationError(errors);
        }

        var referenceErrors = await CheckReferencesAsync(dto.UserId, dto.AccountId, dto.CategoryId);
        if (referenceErrors.Count > 0)
            return OperationResult<TransactionResponseDto>.ValidationError(referenceErrors);

        var transaction = new Transaction()
        {
            UserId = dto.UserId,
            AccountId = dto.AccountId,
            CategoryId = dto.CategoryId,
            Description = dto.Description.Trim(),
            Amount = dto.Amount,
            Type = dto.Type,
            TransactionDate = dto.TransactionDate,
        };

        await _context.Transactions.AddAsync(transaction);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Transaction created: {TransactionId}.", transaction.Id);

        var transactionResponseDto = ToResponseDto(transaction);

        return OperationResult<TransactionResponseDto>.Created(transactionResponseDto);
    }

    public async Task<OperationResult<TransactionResponseDto>> UpdateTransaction (Guid id, UpdateTransactionDto dto)
    {
        var transaction = await GetTransactionByIdAsync(id);
        if (transaction is null)
        {
            _logger.LogWarning("Transaction not found: {TransactionId}.", id);
            return OperationResult<TransactionResponseDto>.NotFound($"Transaction not found: {id}.");
        }

        var validation = await _updateValidator.ValidateAsync(dto);
        if (!validation.IsValid)
        {
            var errors = validation.Errors.ToOperationErrors();
            foreach (var item in errors)
            {
                _logger.LogWarning("Validation failed for UpdateTransaction {TransactionId}. ErrorMessage: {ErrorMessage}. Property: {Property}.", id, item.Message, item.Property);
            }
            
            return OperationResult<TransactionResponseDto>.ValidationError(errors);
        }

        var referenceErrors = await CheckReferencesAsync(transaction.UserId, dto.AccountId, dto.CategoryId);
        if (referenceErrors.Count > 0)
            return OperationResult<TransactionResponseDto>.ValidationError(referenceErrors);

        transaction.AccountId = dto.AccountId;
        transaction.CategoryId = dto.CategoryId;
        transaction.Description = dto.Description.Trim();
        transaction.Amount = dto.Amount;
        transaction.Type = dto.Type;
        transaction.TransactionDate = dto.TransactionDate;

        _context.Transactions.Update(transaction);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Transaction updated: {TransactionId}.", id);

        var transactionResponseDto = ToResponseDto(transaction);

        return OperationResult<TransactionResponseDto>.Success(transactionResponseDto);
    }

    public async Task<OperationResult<TransactionResponseDto>> DeleteTransaction (Guid id)
    {
        var transaction = await GetTransactionByIdAsync(id);
        if (transaction is null)
        {
            _logger.LogWarning("Transaction not found: {TransactionId}.", id);
            return OperationResult<TransactionResponseDto>.NotFound($"Transaction not found: {id}.");
        }

        var transactionResponseDto = ToResponseDto(transaction);

        _context.Transactions.Remove(transaction);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Transaction deleted: {TransactionId}.", id);

        return OperationResult<TransactionResponseDto>.Success(transactionResponseDto);
    }

    private static TransactionResponseDto ToResponseDto(Transaction transaction)
    {
        return new TransactionResponseDto()
        {
            Id = transaction.Id,
            UserId = transaction.UserId,
            AccountId = transaction.AccountId,
            CategoryId = transaction.CategoryId,
            Description = transaction.Description,
            Amount = transaction.Amount,
            Type = transaction.Type,
            TransactionDate = transaction.TransactionDate,
            CreatedAt = transaction.CreatedAt
        };
    }

    private async Task<List<OperationError>> CheckReferencesAsync(Guid userId, Guid accountId, Guid categoryId)
    {
        var referenceErrors = new List<OperationError>();

        var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
        if (!userExists)
        {
            referenceErrors.Add(new OperationError
            {
                Property = nameof(Transaction.UserId),
                Message = $"User not found: {userId}."
            });
            _logger.LogWarning("User not found: {UserId}.", userId);
        }

        var accountExists = await _context.Accounts.AnyAsync(a => a.Id == accountId && a.UserId == userId);
        if (!accountExists)
        {
            referenceErrors.Add(new OperationError
            {
                Property = nameof(Transaction.AccountId),
                Message = $"Account not found: {accountId}."
            });
            _logger.LogWarning("Account not found: {AccountId}.", accountId);
        }

        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == categoryId && c.UserId == userId);
        if (!categoryExists)
        {
            referenceErrors.Add(new OperationError
            {
                Property = nameof(Transaction.CategoryId),
                Message = $"Category not found: {categoryId}."
            });
            _logger.LogWarning("Category not found: {CategoryId}.", categoryId);
        }

        return referenceErrors;
    }
}

