using FluentValidation;
using Swaply.Application.ReportManagement;

namespace Swaply.Application.ReportManagement.FluentValidation;

public class ProcessReportValidator : AbstractValidator<ProcessReportRequest>
{
    public ProcessReportValidator()
    {
        RuleFor(x => x.AdminNote)
            .NotEmpty()
            .WithMessage("Admin note is required.")
            .MaximumLength(1000)
            .WithMessage("Admin note must not exceed 1000 characters.");
    }
}
