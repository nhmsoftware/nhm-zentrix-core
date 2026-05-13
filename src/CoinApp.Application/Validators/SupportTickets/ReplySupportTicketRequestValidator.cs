using CoinApp.Application.Dtos.SupportTickets;
using FluentValidation;

namespace CoinApp.Application.Validators.SupportTickets;

public sealed class ReplySupportTicketRequestValidator : AbstractValidator<ReplySupportTicketRequest>
{
    public ReplySupportTicketRequestValidator()
    {
        RuleFor(x => x.Message)
            .NotEmpty()
            .WithMessage("validation.ticket_message_required")
            .MaximumLength(2000)
            .WithMessage("validation.ticket_message_length");
    }
}
