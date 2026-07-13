using FluentValidation;
using Swaply.Domain.Enums;

namespace Swaply.Application.ReportManagement.FluentValidation;

public class CreateReportValidator : AbstractValidator<CreateReportRequest>
{
    public CreateReportValidator()
    {
        RuleFor(x => x.TargetType)
            .IsInEnum()
            .WithMessage("Invalid target type.");

        RuleFor(x => x.TargetId)
            .NotEmpty()
            .WithMessage("Target ID is required.");

        RuleFor(x => x.Reason)
            .IsInEnum()
            .WithMessage("Invalid report reason.");

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage("Description must not exceed 1000 characters.");
    }
}
