using CoinApp.Application.Dtos.SupportTickets;
using FluentValidation;

namespace CoinApp.Application.Validators.SupportTickets;

public sealed class CreateSupportTicketRequestValidator : AbstractValidator<CreateSupportTicketRequest>
{
    public CreateSupportTicketRequestValidator()
    {
        RuleFor(x => x.Type)
            .InclusiveBetween(1, 3)
            .WithMessage("validation.ticket_type_invalid");

        RuleFor(x => x.Priority)
            .InclusiveBetween(1, 3)
            .WithMessage("validation.ticket_priority_invalid");

        RuleFor(x => x.Message)
            .NotEmpty()
            .WithMessage("validation.ticket_message_required")
            .MaximumLength(2000)
            .WithMessage("validation.ticket_message_length");
    }
}
