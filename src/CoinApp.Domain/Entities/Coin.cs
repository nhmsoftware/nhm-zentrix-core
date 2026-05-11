namespace CoinApp.Domain.Entities;

public sealed class Coin : BaseEntity
{
    public string Symbol { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Decimals { get; set; }
    public bool IsActive { get; set; } = true;
}

