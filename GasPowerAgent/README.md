# GasPowerAgent

`GasPowerAgent` is a small .NET console application that demonstrates an AI market assistant built with the **Microsoft Agent Framework** and OpenAI's Responses API.

It is a framework-based rewrite of `../GasPowerAgentCustom`. Both projects answer the same kinds of questions, but this version delegates agent execution, tool calling, and conversational history to Microsoft Agent Framework instead of implementing an agent loop by hand.

## What it can do

The agent can combine three sources of information:

1. **Conversation history** — remembers earlier turns during the active session.
2. **Retrieved documentation** — searches local text documents using embeddings and cosine similarity (RAG).
3. **Live tools** — calls explicitly registered C# functions for market data and calculations.

For example:

```text
What is today's gas price?

And yesterday?

What is the percentage change?

How is gas balance calculated?

How is gas balance calculated, and what is today's gas price?
```

The last example can use RAG to explain the calculation and a tool to obtain the price.

## Architecture

```text
                         User
                           |
                           v
                  Console application
                           |
          +----------------+----------------+
          |                                 |
          v                                 v
  KnowledgeRetriever                    AIAgent
  (local RAG search)          (Microsoft Agent Framework)
          |                                 |
          v                                 v
 InMemoryVectorStore              AgentSession + tools
          |                                 |
          v                                 v
  Embedded text documents             MarketService
```

Each user message follows this sequence:

1. The application embeds the user question with `text-embedding-3-small`.
2. The `KnowledgeRetriever` searches the in-memory vector store for the three most relevant document chunks.
3. The retrieved text and user question are submitted to the Microsoft Agent Framework agent.
4. The agent decides whether it can answer from the documentation and conversation history or needs a tool.
5. If required, Agent Framework invokes an allowed C# function and returns its result to the model.
6. The agent produces the final answer and saves the exchange in its `AgentSession`.

## Microsoft Agent Framework responsibilities

The core framework code is created in `Program.cs`:

```csharp
var agent = new OpenAIClient(apiKey)
    .GetResponsesClient()
    .AsAIAgent(
    model: "gpt-5.6-luna",
    name: "MarketAgent",
    instructions: AgentInstructions,
    tools: [ /* registered AIFunction tools */ ]);

AgentSession session = await agent.CreateSessionAsync();
var response = await agent.RunAsync(message, session);
```

This replaces the custom project's manual loop that had to:

- call the chat model;
- inspect the completion reason;
- parse requested tool calls;
- run tools;
- append tool results to the conversation;
- repeat until the model returns a final answer.

With Agent Framework, registered tools are invoked automatically as part of an agent run, and `AgentSession` stores the ongoing conversation. The application still owns its business logic and keeps the strict tool boundary.

## Tools

Only three functions are exposed to the agent. The model cannot execute arbitrary C# code.

| Tool | Purpose | Implementation |
| --- | --- | --- |
| `GetGasPrice(date)` | Returns a gas price for a requested date | `MarketService.GetGasPrice` |
| `GetPowerPrice(date)` | Returns a power price for a requested date | `MarketService.GetPowerPrice` |
| `CalculatePercentageChange(oldValue, newValue)` | Calculates percentage change | `MarketService.CalculatePercentageChange` |

`MarketTools` wraps these operations with `Description` attributes, then `AIFunctionFactory.Create(...)` makes them available to Agent Framework. Each tool result is serialized as JSON so the model can use structured data safely.

The current market values are intentionally deterministic sample data:

| Commodity | Today | Yesterday | Other dates |
| --- | ---: | ---: | ---: |
| Gas | 82.50 | 80.00 | 78.25 |
| Power | 74.20 | 76.00 | 73.50 |

Replace `MarketService` with an API client or database implementation when connecting to real market data.

## Retrieval-Augmented Generation (RAG)

The `Docs` folder is the local knowledge base:

```text
Docs/
├── gas-balances.txt
├── market-alerts.txt
└── power-prices.txt
```

At startup, the app:

1. Reads each `.txt` document.
2. Splits it into paragraph-based chunks of roughly 800 characters.
3. Creates an OpenAI embedding for every chunk.
4. Stores each chunk and embedding in `InMemoryVectorStore`.

For every question, the app embeds the question, compares it with document vectors by cosine similarity, and passes the top three matching chunks to the agent as reference material.

RAG is for stable knowledge, such as how a balance is calculated or what a market alert means. Tools are for fresh values and actions.

> Retrieved documents are treated as untrusted reference content. The prompt explicitly tells the agent not to follow any instructions contained inside a document.

## Session and state

The active `AgentSession` preserves conversational context, so natural follow-up questions work:

```text
User: What is today's gas price?
Agent: ...
User: And yesterday?
```

`MarketTools` also records the last commodity and date used by a market-price tool. This is shown through `/state` and is application state, not a substitute for the agent conversation.

`/reset` creates a brand-new Agent Framework session and clears this small application state.

## Prerequisites

- .NET 10 SDK
- An OpenAI API key with access to:
  - the configured chat model (`gpt-5.6-luna` by default);
  - `text-embedding-3-small` for document indexing.

The main packages are:

```text
Microsoft.Agents.AI.OpenAI  1.6.1
OpenAI                      2.12.0
```

## Run the application

From this project directory:

```powershell
$env:OPENAI_API_KEY = "your-api-key"
dotnet run --project .\GasPowerAgent.csproj
```

The key is read only from `OPENAI_API_KEY`; it is not stored in source code. To use a different chat model, update the `model` argument in `Program.cs`.

## Console commands

| Command | Description |
| --- | --- |
| `/state` | Shows the last commodity and date retrieved via a market tool. |
| `/reset` | Starts a new Agent Framework conversation session and clears the displayed application state. |
| `/exit` | Stops the application. |

## Project structure

```text
GasPowerAgent/
├── Docs/                       Local RAG source documents
├── Program.cs                  Agent setup, tools, RAG, and console UI
├── GasPowerAgent.csproj        .NET project and package references
└── README.md                   This guide
```

The sample keeps the code in one file so the entire flow is easy to follow. In a production application, split the types into separate files, use dependency injection, persist the vector index and session state, add authentication/authorization, and add tracing/telemetry.

## Building

```powershell
dotnet build .\GasPowerAgent.csproj
```

The project has been built successfully with no compiler errors.
