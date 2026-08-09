using GasPowerAgentCustom;
using OpenAI.Chat;
using OpenAI.Embeddings;

internal class Program
{
    public static async Task Main(string[] args)
    {
        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        
        var embeddingClient = new EmbeddingClient(model: "text-embedding-3-small", apiKey: apiKey);

        var embeddingService = new EmbeddingService(embeddingClient);

        var vectorStore = new InMemoryVectorStore();

        var chunker = new TextChunker(maxCharacters: 800);

        var indexer = new DocumentIndexer(chunker, embeddingService, vectorStore);

        var retriever = new KnowledgeRetriever(embeddingService, vectorStore);

        Console.WriteLine("Building knowledge index...");

        await indexer.IndexDirectoryAsync(
            "Docs");

        Console.WriteLine("Knowledge index ready.");

        var client = new ChatClient(
            model: "gpt-5.1",
            apiKey: apiKey);

        var marketService = new MarketService();

        var toolExecutor = new ToolExecutor(marketService);

        var session = new AgentSession();

        var contextBuilder = new ContextBuilder();

        var summarizer = new ConversationSummarizer(client);

        var compactor = new ConversationCompactor(summarizer);

        var agent = new MarketAgent(
            client,
            toolExecutor,
            session,
            contextBuilder,
            compactor,
            retriever);

        Console.WriteLine("Market Agent V3");

        while (true)
        {
            Console.WriteLine();
            Console.Write("> ");

            var input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
                continue;

            if (input == "/exit")
                break;

            if (input == "/reset")
            {
                agent.Reset();

                Console.WriteLine(
                    "Session reset.");

                continue;
            }

            if (input == "/state")
            {
                ShowState(session);
                continue;
            }

            if (input == "/context")
            {
                ShowContext(session, contextBuilder);

                continue;
            }

            try
            {
                var result =
                    await agent.RunAsync(input);

                Console.WriteLine();
                Console.WriteLine($"AGENT: {result}");

                Console.WriteLine();

                Console.WriteLine($"Recent messages: " + $"{session.RecentMessages.Count}");

                Console.WriteLine($"Summary: " +
                                  $"{(session.Summary is null ? "no" : "yes")}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"ERROR: {ex.Message}");
            }
        }

        static void ShowState(
            AgentSession session)
        {
            Console.WriteLine();
            Console.WriteLine("STATE");

            Console.WriteLine($"Commodity: " + $"{session.State.CurrentCommodity}");

            Console.WriteLine($"Market: " + $"{session.State.CurrentMarket}");

            Console.WriteLine($"Date: " + $"{session.State.CurrentDate}");

            Console.WriteLine();

            Console.WriteLine("SUMMARY:");

            Console.WriteLine(
                session.Summary ?? "<none>");
        }

        static void ShowContext(
            AgentSession session,
            ContextBuilder builder)
        {
            var context = builder.Build(session, []);

            Console.WriteLine();
            Console.WriteLine($"LLM context contains {context.Count} messages.");

            foreach (var message in context)
            {
                Console.WriteLine(
                    message.GetType().Name);
            }
        }
    }
}
