using FluentValidation;
using UniFlow.Business.Contracts.Syllabus;

namespace UniFlow.Business.Validation;

public sealed class SyllabusConfirmRequestValidator : AbstractValidator<SyllabusConfirmRequest>
{
    public SyllabusConfirmRequestValidator()
    {
        RuleFor(x => x.ScanId).NotEmpty();
        RuleFor(x => x.CourseCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.CourseTitle).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Items).NotEmpty().WithMessage("At least one task item is required.");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.Title).NotEmpty().MaximumLength(512);
            item.RuleFor(i => i.Description).MaximumLength(4000).When(i => !string.IsNullOrWhiteSpace(i.Description));
            item.RuleFor(i => i.Type).MaximumLength(128).When(i => !string.IsNullOrWhiteSpace(i.Type));
            item.RuleFor(i => i.PriorityScore).InclusiveBetween(1, 100).When(i => i.PriorityScore.HasValue);
        });
    }
}
