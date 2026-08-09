using OpenAI.Chat;

public sealed class AgentSession
{
    public Guid Id { get; } = Guid.NewGuid();

    public MarketAgentState State { get; } = new();

    public string? Summary { get; set; }

    public List<ChatMessage> RecentMessages { get; } = [];

    public List<MarketObservation> Observations { get; } = [];

    public void Reset()
    {
        State.CurrentCommodity = null;
        State.CurrentMarket = null;
        State.CurrentDate = null;

        Summary = null;

        RecentMessages.Clear();
        Observations.Clear();
    }
}

public sealed record MarketObservation(
    string Commodity,
    DateOnly Date,
    decimal Price,
    DateTime RetrievedAt);