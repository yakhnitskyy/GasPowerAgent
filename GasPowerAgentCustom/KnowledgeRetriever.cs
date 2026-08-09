namespace GasPowerAgentCustom;

public sealed class KnowledgeRetriever
{
    private readonly EmbeddingService _embeddings;
    private readonly InMemoryVectorStore _store;

    public KnowledgeRetriever(
        EmbeddingService embeddings,
        InMemoryVectorStore store)
    {
        _embeddings = embeddings;
        _store = store;
    }

    public async Task<IReadOnlyList<SearchResult>>
        RetrieveAsync(
            string query,
            int topK = 3)
    {
        var queryEmbedding =
            await _embeddings.CreateAsync(query);

        return _store.Search(
            queryEmbedding,
            topK);
    }
}
