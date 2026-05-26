using FluentValidation;

namespace RvPersonalFinance.Api.Features.Accounts;

public class CreateAccountValidator : AbstractValidator<CreateAccountDto>
{

    public CreateAccountValidator()
    {
        RuleFor(x => x.UserId)
            .NotEqual(Guid.Empty).WithMessage("UserId is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(80).WithMessage("Name must not exceed 80 characters.");
            
        
        RuleFor(x => x.InitialBalance)
            .GreaterThanOrEqualTo(0m)
            .WithMessage("Initial balance must be zero or greater.")
            .Must(x => x.Scale <= 2)
            .WithMessage("Initial balance cannot have more than 2 decimal places");
    }

}