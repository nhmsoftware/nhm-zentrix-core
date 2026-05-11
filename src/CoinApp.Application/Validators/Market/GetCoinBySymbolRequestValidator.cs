using CoinApp.Application.Dtos.Market;
using FluentValidation;

namespace CoinApp.Application.Validators.Market;

public sealed class GetCoinBySymbolRequestValidator : AbstractValidator<GetCoinBySymbolRequest>
{
    public GetCoinBySymbolRequestValidator()
    {
        RuleFor(x => x.Symbol)
            .NotEmpty()
            .WithMessage("validation.coin_symbol_required")
            .MaximumLength(20)
            .WithMessage("validation.coin_symbol_length")
            .Matches("^[A-Za-z0-9._-]+$")
            .WithMessage("validation.coin_symbol_format");
    }
}
