# Agent Basics

`AgentBasics` is a .NET learning workspace containing two versions of the same market-data AI assistant. They demonstrate the same core agent capabilities while using different orchestration approaches.

## Projects

| Project | Description | Best for |
| --- | --- | --- |
| [GasPowerAgentCustom](./GasPowerAgentCustom/README.md) | A from-first-principles implementation that manually manages prompts, tool calls, conversation history, context, compaction, and RAG. | Learning how an agent works internally. |
| [GasPowerAgent](./GasPowerAgent/README.md) | A Microsoft Agent Framework version that uses `AIAgent`, `AgentSession`, and registered `AIFunction` tools. | Learning how a framework reduces agent orchestration code. |

## Shared scenario

Both projects are console-based market assistants. They can:

- answer questions about sample gas and power prices;
- calculate percentage changes;
- retrieve relevant content from local text documents using RAG;
- combine documentation and tool results in one answer;
- maintain an active conversational session so follow-up questions work.

Example questions:

```text
What is today's gas price?
And yesterday?
What's the percentage change?
How is gas balance calculated?
How is gas balance calculated, and what is today's gas price?
```

## How the approaches differ

```text
GasPowerAgentCustom
User -> custom context builder -> OpenAI chat call -> custom tool loop -> custom session state

GasPowerAgent
User -> RAG enrichment -> Microsoft Agent Framework AIAgent -> registered C# functions -> AgentSession
```

### GasPowerAgentCustom

The custom implementation makes the lower-level mechanics visible:

- builds model messages with `ContextBuilder`;
- checks completion reasons and repeats its own agent loop;
- dispatches tools through `ToolExecutor`;
- stores tool messages and recent history itself;
- summarizes older conversation messages with `ConversationCompactor`;
- maintains explicit application state in `MarketAgentState`.

It is intentionally educational rather than production-ready.

### GasPowerAgent

The framework version keeps the same business logic and local RAG design, but delegates agent orchestration to Microsoft Agent Framework:

- `AIAgent` coordinates the model interaction;
- `AgentSession` retains the active conversation;
- `AIFunctionFactory` exposes only approved C# functions to the model;
- OpenAI's Responses API is used so `gpt-5.6-luna` can call functions;
- the application still controls document retrieval, tool implementations, and data access.

## RAG in both projects

Both projects index the `.txt` files in their `Docs` directory at startup:

```text
Documents -> text chunks -> embeddings -> in-memory vector store
```

For each question, the application creates an embedding, finds the most similar chunks with cosine similarity, and gives those chunks to the model as reference material. This is Retrieval-Augmented Generation (RAG); it supplies relevant information at request time and does not retrain the model.

## Running the projects

Use an OpenAI API key supplied through an environment variable:

```powershell
$env:OPENAI_API_KEY = "your-api-key"
```

Then run either project:

```powershell
dotnet run --project .\GasPowerAgentCustom\GasPowerAgentCustom.csproj
dotnet run --project .\GasPowerAgent\GasPowerAgent.csproj
```

`GasPowerAgent` is configured for `gpt-5.6-luna` and `text-embedding-3-small`. Ensure the API key has access to both models.

> Do not commit API keys to source control. If a key has been included in a file or shared accidentally, revoke and rotate it.
