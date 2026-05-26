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
    
    public TransactionService(AppDbContext context, ILogger<TransactionService> logger, IValidator<CreateTransactionDto> createValidator)
    {
        _context = context;
        _logger = logger;
        _createValidator = createValidator;
    }   

    public async Task<OperationResult<Transaction>> GetTransactionById(Guid id)
    {
        var transaction = await _context.Transactions.FirstOrDefaultAsync(transaction => transaction.Id == id);
        if (transaction is null)
        {
            _logger.LogWarning("Transaction not found: {TransactionId}", id);
            return new OperationResult<Transaction>()
            {
                Status = ResultStatus.NotFound,
                Message = $"Transaction not found: {id}",
            };
        }

        _logger.LogInformation("Transaction retrieved: {TransactionId}", transaction.Id);
        return new OperationResult<Transaction>() { Data = transaction };

    }

    public async Task<OperationResult<IEnumerable<Transaction>>> GetAllTransactions()
    {
        var transactions = await _context.Transactions.ToListAsync();

        _logger.LogInformation("Transactions retrieved: {Count}", transactions.Count);
        return new OperationResult<IEnumerable<Transaction>>()
        {
            Data = transactions
        };
    }

    public async Task<OperationResult<Transaction>> CreateTransaction(CreateTransactionDto dto)
    {
        var validation = await _createValidator.ValidateAsync(dto);
        if (!validation.IsValid)
        {
            _logger.LogWarning("Validation failed for CreateTransaction: {Error}", validation.Errors.First().ErrorMessage);
            return new OperationResult<Transaction>()
            {
                Status = ResultStatus.ValidationError,
                Message = validation.Errors.First().ErrorMessage,
            };
        }

        var transaction = new Transaction()
        {
            UserId = dto.UserId,
            AccountId = dto.AccountId,
            CategoryId = dto.CategoryId,
            Description = dto.Description,
            Amount = dto.Amount,
            Type = dto.Type,
            TransactionDate = dto.TransactionDate,
        };

        await _context.Transactions.AddAsync(transaction);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Transaction created: {TransactionId}", transaction.Id);

        return new OperationResult<Transaction>(){
            Status = ResultStatus.Created,
            Data = transaction
        };
    }

    public async Task<OperationResult<Transaction>> UpdateTransaction (Guid id, UpdateTransactionDto dto)
    {
        var result = await GetTransactionById(id);
        if (!result.IsSuccess)
            return result;

        var transaction = result.Data!;
        transaction.UserId = dto.UserId;
        transaction.AccountId = dto.AccountId;
        transaction.CategoryId = dto.CategoryId;
        transaction.Description = dto.Description;
        transaction.Amount = dto.Amount;
        transaction.Type = dto.Type;
        transaction.TransactionDate = dto.TransactionDate;

        _context.Transactions.Update(transaction);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Transaction updated: {TransactionId}", id);

        return new OperationResult<Transaction>() { Data = transaction };
    }

    public async Task<OperationResult<Transaction>> DeleteTransaction (Guid id)
    {
        var result = await GetTransactionById(id);
        if (!result.IsSuccess)
            return result;

        var transaction = result.Data!;
        _context.Transactions.Remove(transaction);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Transaction deleted: {TransactionId}", id);
        return new OperationResult<Transaction>() { Data = transaction };
    }

}

