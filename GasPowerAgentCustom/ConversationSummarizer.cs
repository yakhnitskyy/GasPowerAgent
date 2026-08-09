using OpenAI.Chat;

public sealed class ConversationSummarizer
{
    private readonly ChatClient _client;

    public ConversationSummarizer(ChatClient client)
    {
        _client = client;
    }

    public async Task<string> SummarizeAsync(
        string? previousSummary,
        IReadOnlyList<ChatMessage> messages)
    {
        var prompt = new List<ChatMessage>
        {
            new SystemChatMessage(
                """
                Summarize the conversation for another AI agent.

                Preserve:
                - user intentions
                - selected commodities and markets
                - important numeric values
                - dates
                - tool results that may be referenced later
                - unresolved requests

                Be concise.

                Do not add information that was not present.
                """)
        };

        if (!string.IsNullOrWhiteSpace(previousSummary))
        {
            prompt.Add(
                new UserChatMessage(
                    $"""
                     Existing summary:

                     {previousSummary}
                     """));
        }

        foreach (var message in messages)
        {
            prompt.Add(message);
        }

        ChatCompletion completion =
            await _client.CompleteChatAsync(prompt);

        return completion.Content[0].Text;
    }
}