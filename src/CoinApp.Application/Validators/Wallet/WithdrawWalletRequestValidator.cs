using CoinApp.Application.Dtos.Wallet;
using FluentValidation;

namespace CoinApp.Application.Validators.Wallet;

public sealed class WithdrawWalletRequestValidator : AbstractValidator<WithdrawWalletRequest>
{
    public WithdrawWalletRequestValidator()
    {
        RuleFor(x => x.Money)
            .GreaterThan(0)
            .WithMessage("validation.wallet_money_positive");
    }
}
