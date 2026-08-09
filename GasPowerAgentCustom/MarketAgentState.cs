public sealed class MarketAgentState
{
    public string? CurrentCommodity { get; set; }

    public string? CurrentMarket { get; set; }

    public DateOnly? CurrentDate { get; set; }
}