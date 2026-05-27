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
            _logger.LogWarning("Transaction not found: {TransactionId}", id);
            return new OperationResult<TransactionResponseDto>()
            {
                Status = ResultStatus.NotFound,
                Message = $"Transaction not found: {id}",
            };
        }

        var transactionResponseDto = ToResponseDto(transaction);

        _logger.LogInformation("Transaction retrieved: {TransactionId}", transaction.Id);
        return new OperationResult<TransactionResponseDto>() { Data = transactionResponseDto };

    }

    public async Task<OperationResult<IEnumerable<TransactionResponseDto>>> GetAllTransactions()
    {
        var transactions = await _context.Transactions.ToListAsync();

        _logger.LogInformation("Transactions retrieved: {Count}", transactions.Count);

        var transactionResponseDtos = transactions
            .Select(ToResponseDto)
            .ToList();

        return new OperationResult<IEnumerable<TransactionResponseDto>>()
        {
            Data = transactionResponseDtos,
        };
    }

    public async Task<OperationResult<TransactionResponseDto>> CreateTransaction(CreateTransactionDto dto)
    {
        var validation = await _createValidator.ValidateAsync(dto);
        if (!validation.IsValid)
        {
            _logger.LogWarning("Validation failed for CreateTransaction: {Error}", validation.Errors.First().ErrorMessage);
            return new OperationResult<TransactionResponseDto>()
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
            Description = dto.Description.Trim(),
            Amount = dto.Amount,
            Type = dto.Type,
            TransactionDate = dto.TransactionDate,
        };

        await _context.Transactions.AddAsync(transaction);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Transaction created: {TransactionId}", transaction.Id);

        var transactionResponseDto = ToResponseDto(transaction);

        return new OperationResult<TransactionResponseDto>(){
            Status = ResultStatus.Created,
            Data = transactionResponseDto
        };
    }

    public async Task<OperationResult<TransactionResponseDto>> UpdateTransaction (Guid id, UpdateTransactionDto dto)
    {
        var transaction = await GetTransactionByIdAsync(id);
        if (transaction is null)
        {
            _logger.LogWarning("Transaction not found: {TransactionId}", id);
            return new OperationResult<TransactionResponseDto>()
            {
                Status = ResultStatus.NotFound,
                Message = $"Transaction not found: {id}",
            };
        }

        transaction.UserId = dto.UserId;
        transaction.AccountId = dto.AccountId;
        transaction.CategoryId = dto.CategoryId;
        transaction.Description = dto.Description.Trim();
        transaction.Amount = dto.Amount;
        transaction.Type = dto.Type;
        transaction.TransactionDate = dto.TransactionDate;

        _context.Transactions.Update(transaction);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Transaction updated: {TransactionId}", id);

        var transactionResponseDto = ToResponseDto(transaction);

        return new OperationResult<TransactionResponseDto>() { Data = transactionResponseDto };
    }

    public async Task<OperationResult<TransactionResponseDto>> DeleteTransaction (Guid id)
    {
        var transaction = await GetTransactionByIdAsync(id);
        if (transaction is null)
        {
            _logger.LogWarning("Transaction not found: {TransactionId}", id);
            return new OperationResult<TransactionResponseDto>()
            {
                Status = ResultStatus.NotFound,
                Message = $"Transaction not found: {id}",
            };
        }

        _context.Transactions.Remove(transaction);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Transaction deleted: {TransactionId}", id);

        var transactionResponseDto = ToResponseDto(transaction);

        return new OperationResult<TransactionResponseDto>() { Data = transactionResponseDto };
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

}

