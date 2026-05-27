using FluentValidation;

namespace RvPersonalFinance.Api.Features.Transactions;

public class CreateTransactionValidator : AbstractValidator<CreateTransactionDto>
{
    
    public CreateTransactionValidator()
    {
        RuleFor(x => x.UserId).NotEqual(Guid.Empty).WithMessage("UserId is required.");
        RuleFor(x => x.AccountId).NotEqual(Guid.Empty).WithMessage("AccountId is required.");
        RuleFor(x => x.CategoryId).NotEqual(Guid.Empty).WithMessage("CategoryId is required.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(100).WithMessage("Description must not exceed 100 characters.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero.")
            .Must(x => x.Scale <= 2).WithMessage("Amount cannot have more than 2 decimal places");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid transaction type.");

        RuleFor(x => x.TransactionDate)
            .NotEmpty().WithMessage("Transaction date is required.");            
    }
}

public class UpdateTransactionValidator : AbstractValidator<UpdateTransactionDto>
{
    public UpdateTransactionValidator()
    {
        RuleFor(x => x.UserId).NotEqual(Guid.Empty).WithMessage("UserId is required.");
        RuleFor(x => x.AccountId).NotEqual(Guid.Empty).WithMessage("AccountId is required.");
        RuleFor(x => x.CategoryId).NotEqual(Guid.Empty).WithMessage("CategoryId is required.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(100).WithMessage("Description must not exceed 100 characters.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero.")
            .Must(x => x.Scale <= 2).WithMessage("Amount cannot have more than 2 decimal places");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid transaction type.");

        RuleFor(x => x.TransactionDate)
            .NotEmpty().WithMessage("Transaction date is required.");        
    }
}