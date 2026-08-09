namespace GasPowerAgentCustom;

public sealed class ConversationCompactor
{
    private const int CompactAt = 16;
    private const int KeepRecent = 8;

    private readonly ConversationSummarizer _summarizer;

    public ConversationCompactor(
        ConversationSummarizer summarizer)
    {
        _summarizer = summarizer;
    }

    public async Task CompactIfNeededAsync(
        AgentSession session)
    {
        if (session.RecentMessages.Count < CompactAt)
            return;

        var countToSummarize =
            session.RecentMessages.Count - KeepRecent;

        var oldMessages =
            session.RecentMessages
                .Take(countToSummarize)
                .ToList();

        var newSummary =
            await _summarizer.SummarizeAsync(
                session.Summary,
                oldMessages);

        session.Summary = newSummary;

        session.RecentMessages.RemoveRange(
            0,
            countToSummarize);
    }
}
