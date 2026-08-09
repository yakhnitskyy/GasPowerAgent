namespace GasPowerAgent;

public sealed class MarketService
{
    public decimal GetGasPrice(DateOnly date) => 
        date == DateOnly.FromDateTime(DateTime.UtcNow) 
            ? 82.50m 
            : date == DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1) ? 80.00m : 78.25m;
    public decimal GetPowerPrice(DateOnly date) => 
        date == DateOnly.FromDateTime(DateTime.UtcNow) 
            ? 74.20m 
            : date == DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1) ? 76.00m : 73.50m;
    public decimal CalculatePercentageChange(decimal oldValue, decimal newValue) => 
        oldValue == 0 ? throw new ArgumentException("Old value cannot be zero.") 
            : (newValue - oldValue) / oldValue * 100m;
}
