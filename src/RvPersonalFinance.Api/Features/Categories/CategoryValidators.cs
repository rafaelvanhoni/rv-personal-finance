using FluentValidation;

namespace RvPersonalFinance.Api.Features.Categories;

public class CreateCategoryValidator : AbstractValidator<CreateCategoryDto>
{
    public CreateCategoryValidator()
    {
        RuleFor(x => x.UserId).NotEqual(Guid.Empty).WithMessage("UserId is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(50).WithMessage("Name must not exceed 50 characters.");
    }
}