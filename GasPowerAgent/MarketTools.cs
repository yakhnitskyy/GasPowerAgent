using System.ComponentModel;
using System.Text.Json;

namespace GasPowerAgent;

public sealed class MarketTools(MarketService marketService)
{
    private string? _currentCommodity;
    private DateOnly? _currentDate;

    [Description("Gets the live gas price for a date in yyyy-MM-dd format.")]
    public string GetGasPrice([Description("Date in yyyy-MM-dd format.")] string date)
    {
        var parsedDate = DateOnly.Parse(date);
        _currentCommodity = "gas";
        _currentDate = parsedDate;
        return JsonSerializer.Serialize(new { success = true, commodity = "gas", date = parsedDate, price = marketService.GetGasPrice(parsedDate) });
    }

    [Description("Gets the live power price for a date in yyyy-MM-dd format.")]
    public string GetPowerPrice([Description("Date in yyyy-MM-dd format.")] string date)
    {
        var parsedDate = DateOnly.Parse(date);
        _currentCommodity = "power";
        _currentDate = parsedDate;
        return JsonSerializer.Serialize(new { success = true, commodity = "power", date = parsedDate, price = marketService.GetPowerPrice(parsedDate) });
    }

    [Description("Calculates the percentage change from an old value to a new value.")]
    public string CalculatePercentageChange([Description("The original value.")] decimal oldValue, [Description("The new value.")] decimal newValue) =>
        JsonSerializer.Serialize(new { success = true, percentageChange = marketService.CalculatePercentageChange(oldValue, newValue) });

    public void ResetState() => (_currentCommodity, _currentDate) = (null, null);
    public string StateDescription() => $"""
                                         STATE
                                         Commodity: {_currentCommodity ?? "not selected"}
                                         Date: {_currentDate?.ToString("yyyy-MM-dd") ?? "not selected"}
                                         Conversation history: managed by the Microsoft Agent Framework session.
                                         """;
}
