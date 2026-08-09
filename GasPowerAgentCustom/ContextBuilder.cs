using OpenAI.Chat;

namespace GasPowerAgentCustom;

public sealed class ContextBuilder
{
    public IReadOnlyList<ChatMessage> Build(
        AgentSession session,
        IReadOnlyList<SearchResult> knowledge)
    {
        var messages =
            new List<ChatMessage>();

        messages.Add(
            BuildSystemMessage());

        if (!string.IsNullOrWhiteSpace(
                session.Summary))
        {
            messages.Add(
                new SystemChatMessage(
                    $"""
                    Conversation summary:

                    {session.Summary}
                    """));
        }

        messages.Add(
            new SystemChatMessage(
                BuildStateDescription(
                    session.State)));

        if (knowledge.Count > 0)
        {
            messages.Add(
                new SystemChatMessage(
                    BuildKnowledgeContext(
                        knowledge)));
        }

        messages.AddRange(
            session.RecentMessages);

        return messages;
    }

    private static ChatMessage
        BuildSystemMessage()
    {
        var today =
            DateOnly
                .FromDateTime(DateTime.UtcNow)
                .ToString("yyyy-MM-dd");

        return new SystemChatMessage(
            $"""
            You are a market data assistant.

            You have two information sources:

            1. Retrieved documentation supplied in context.
            2. Tools that can retrieve live market data.

            Use retrieved documentation when answering
            questions about APIs, calculations, behaviour,
            or system documentation.

            Use tools when the user asks for actual market
            values or fresh/live information.

            Never invent market data.

            When answering from retrieved documentation,
            base the answer on that documentation.

            If the documentation does not contain enough
            information, say so.

            Today's date is {today}.
            """);
    }

    private static string
        BuildKnowledgeContext(
            IReadOnlyList<SearchResult> results)
    {
        var sections =
            results.Select(
                x =>
                    $"""
                    SOURCE: {x.Chunk.Source}

                    {x.Chunk.Text}
                    """);

        return $"""
        Retrieved documentation:

        {string.Join(
            "\n\n---\n\n",
            sections)}
        """;
    }

    private static string
        BuildStateDescription(
            MarketAgentState state)
    {
        return $"""
        Current application state:

        Commodity:
        {state.CurrentCommodity ?? "not selected"}

        Market:
        {state.CurrentMarket ?? "not selected"}

        Date:
        {state.CurrentDate?.ToString("yyyy-MM-dd")
         ?? "not selected"}
        """;
    }
}
