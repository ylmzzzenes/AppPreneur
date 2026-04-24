using FluentValidation;
using UniFlow.Business.Contracts.Chat;

namespace UniFlow.Business.Validation;

public sealed class ChatRequestValidator : AbstractValidator<ChatRequest>
{
    public ChatRequestValidator()
    {
        RuleFor(x => x.Message).NotEmpty().MaximumLength(8000);
    }
}
