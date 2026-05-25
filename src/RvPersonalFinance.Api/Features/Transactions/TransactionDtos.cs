using RvPersonalFinance.Api.Domain.Enums;

namespace RvPersonalFinance.Api.Features.Transactions;

public class CreateTransactionDto
{
    public Guid UserId { get; set; }
    public Guid AccountId { get; set; }
    public Guid CategoryId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    public DateOnly TransactionDate { get; set; }
 }

public class UpdateTransactionDto
{
    public Guid UserId { get; set; }
    public Guid AccountId { get; set; }
    public Guid CategoryId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    public DateOnly TransactionDate { get; set; }
 }

public class TransactionResponseDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public Guid AccountId { get; init; }
    public Guid CategoryId { get; init; }
    public string Description { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public TransactionType Type { get; init; }
    public DateOnly TransactionDate { get; init; }
    public DateTime CreatedAt { get; init; }
}