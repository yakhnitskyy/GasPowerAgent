using OpenAI.Chat;

namespace GasPowerAgentCustom;

public sealed class MarketAgent
{
    private const int MaxIterations = 10;

    private readonly ChatClient _client;
    private readonly ToolExecutor _toolExecutor;
    private readonly AgentSession _session;
    private readonly ContextBuilder _contextBuilder;
    private readonly ConversationCompactor _compactor;

    private readonly ChatCompletionOptions _options;
    private readonly KnowledgeRetriever _retriever;

    public MarketAgent(
        ChatClient client,
        ToolExecutor toolExecutor,
        AgentSession session,
        ContextBuilder contextBuilder,
        ConversationCompactor compactor,
        KnowledgeRetriever retriever)
    {
        _client = client;
        _toolExecutor = toolExecutor;
        _session = session;
        _contextBuilder = contextBuilder;
        _compactor = compactor;
        _retriever = retriever;

        _options = new ChatCompletionOptions
        {
            Tools =
            {
                AgentTools.GetGasPriceTool,
                AgentTools.GetPowerPriceTool,
                AgentTools.CalculatePercentageChangeTool
            }
        };
    }

   public async Task<string> RunAsync(
    string userMessage)
{
    //
    // 1. Retrieve RAG knowledge for this USER request.
    //
    var knowledge =
        await _retriever.RetrieveAsync(
            userMessage,
            topK: 3);

    //
    // 2. Build the initial context ONCE.
    //
    var messages =
        _contextBuilder
            .Build(
                _session,
                knowledge)
            .ToList();

    //
    // 3. Add the new user message.
    //
    var userChatMessage =
        new UserChatMessage(userMessage);

    messages.Add(userChatMessage);

    _session.RecentMessages.Add(
        userChatMessage);

    //
    // 4. Agent loop.
    //
    for (var iteration = 1;
         iteration <= MaxIterations;
         iteration++)
    {
        Console.WriteLine();
        Console.WriteLine(
            $"--- Agent iteration {iteration} ---");

        ChatCompletion completion =
            await _client.CompleteChatAsync(
                messages,
                _options);

        switch (completion.FinishReason)
        {
            case ChatFinishReason.Stop:
            {
                var assistantMessage =
                    new AssistantChatMessage(
                        completion);

                //
                // Keep both working context
                // and persistent session in sync.
                //
                messages.Add(
                    assistantMessage);

                _session.RecentMessages.Add(
                    assistantMessage);

                await _compactor
                    .CompactIfNeededAsync(
                        _session);

                return completion.Content.Count > 0
                    ? completion.Content[0].Text
                    : string.Empty;
            }

            case ChatFinishReason.ToolCalls:
            {
                //
                // Assistant requested one or more tools.
                //
                var assistantMessage =
                    new AssistantChatMessage(
                        completion);

                messages.Add(
                    assistantMessage);

                _session.RecentMessages.Add(
                    assistantMessage);

                //
                // IMPORTANT:
                // EVERY tool call must receive a
                // matching ToolChatMessage.
                //
                foreach (var toolCall
                         in completion.ToolCalls)
                {
                    var result =
                        _toolExecutor.Execute(
                            toolCall);

                    Console.WriteLine(
                        $"  TOOL RESULT: {result}");

                    var toolMessage =
                        new ToolChatMessage(
                            toolCall.Id,
                            result);

                    //
                    // Add immediately after
                    // assistant tool call.
                    //
                    messages.Add(
                        toolMessage);

                    _session.RecentMessages.Add(
                        toolMessage);

                    UpdateState(
                        toolCall);
                }

                //
                // Continue with SAME messages list.
                //
                break;
            }

            default:
                throw new InvalidOperationException(
                    $"Unexpected finish reason: " +
                    $"{completion.FinishReason}");
        }
    }

    throw new InvalidOperationException(
        $"Maximum agent iterations " +
        $"({MaxIterations}) exceeded.");
}

    private void UpdateState(
        ChatToolCall toolCall)
    {
        switch (toolCall.FunctionName)
        {
            case "get_gas_price":
                _session.State.CurrentCommodity = "gas";
                break;

            case "get_power_price":
                _session.State.CurrentCommodity = "power";
                break;
        }
    }

    public void Reset()
    {
        _session.Reset();
    }
}
