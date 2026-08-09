using OpenAI.Embeddings;

namespace GasPowerAgentCustom;

public sealed class EmbeddingService
{
    private readonly EmbeddingClient _client;

    public EmbeddingService(
        EmbeddingClient client)
    {
        _client = client;
    }

    public async Task<float[]> CreateAsync(
        string text)
    {
        OpenAIEmbedding embedding =
            await _client.GenerateEmbeddingAsync(text);

        return embedding
            .ToFloats()
            .ToArray();
    }
}
