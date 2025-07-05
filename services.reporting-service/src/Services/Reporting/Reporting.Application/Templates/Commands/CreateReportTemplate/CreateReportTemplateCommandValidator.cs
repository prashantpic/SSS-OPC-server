using FluentValidation;

namespace Reporting.Application.Templates.Commands.CreateReportTemplate;

public class CreateReportTemplateCommandValidator : AbstractValidator<CreateReportTemplateCommand>
{
    public CreateReportTemplateCommandValidator()
    {
        RuleFor(v => v.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

        RuleFor(v => v.DefaultFormat)
            .NotEmpty()
            .Must(BeAValidReportFormat).WithMessage("Invalid report format specified.");

        RuleFor(v => v.DataSources)
            .NotEmpty().WithMessage("At least one data source is required.");
            
        RuleForEach(v => v.DataSources).ChildRules(source =>
        {
            source.RuleFor(s => s.Name).NotEmpty();
            source.RuleFor(s => s.Type).NotEmpty();
        });

        RuleFor(v => v.Branding).NotNull();

        When(v => !string.IsNullOrEmpty(v.Schedule), () =>
        {
            RuleFor(v => v.Schedule)
                .Must(BeAValidCronExpression)
                .WithMessage("The provided schedule is not a valid CRON expression.");
        });
    }

    private bool BeAValidReportFormat(string format)
    {
        return Enum.TryParse<Domain.Enums.ReportFormat>(format, true, out _);
    }
    
    private bool BeAValidCronExpression(string? cron)
    {
        if (string.IsNullOrWhiteSpace(cron)) return true; // Null is valid, empty is not handled by When
        // A real implementation would use a robust library like Cronos
        return cron.Split(' ').Length is >= 5 and <= 6;
    }
}