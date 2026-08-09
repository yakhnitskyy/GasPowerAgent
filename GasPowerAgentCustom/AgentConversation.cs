using OpenAI.Chat;

public sealed class AgentConversation
{
    private readonly List<ChatMessage> _messages = [];

    public IReadOnlyList<ChatMessage> Messages => _messages;

    public AgentConversation()
    {
        Reset();
    }

    public void Add(ChatMessage message)
    {
        _messages.Add(message);
    }

    public void Reset()
    {
        _messages.Clear();

        var today =
            DateOnly
                .FromDateTime(DateTime.UtcNow)
                .ToString("yyyy-MM-dd");

        _messages.Add(
            new SystemChatMessage(
                $"""
                 You are a market data assistant.

                 Use the provided tools whenever market data
                 or calculations are required.

                 Never invent market prices.

                 You may use previous conversation results
                 when the user clearly refers to them.

                 If the user requests current, latest, or
                 up-to-date market data, call the relevant
                 market-data tool again instead of relying
                 on an old result.

                 Today's date is {today}.
                 """));
    }
}