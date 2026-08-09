namespace GasPowerAgentCustom;

public sealed class MarketService
{
    public decimal GetGasPrice(DateOnly date)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        if (date == today)
            return 82.50m;

        if (date == today.AddDays(-1))
            return 80.00m;

        return 78.25m;
    }

    public decimal GetPowerPrice(DateOnly date)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        if (date == today)
            return 74.20m;

        if (date == today.AddDays(-1))
            return 76.00m;

        return 73.50m;
    }

    public decimal CalculatePercentageChange(
        decimal oldValue,
        decimal newValue)
    {
        if (oldValue == 0)
            throw new ArgumentException(
                "Old value cannot be zero.");

        return (newValue - oldValue)
               / oldValue
               * 100m;
    }
}
