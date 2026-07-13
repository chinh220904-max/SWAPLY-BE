using FluentValidation;

namespace Swaply.Application.AdminManagement.FluentValidation;

public class LockUserRequest
{
    public Guid Id { get; set; }
}

public class LockUserValidator : AbstractValidator<LockUserRequest>
{
    public LockUserValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("User id is required.");
    }
}
