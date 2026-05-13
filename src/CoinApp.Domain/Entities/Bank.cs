namespace CoinApp.Domain.Entities;

public sealed class Bank : BaseEntity
{
    public string Bin { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ShortName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
