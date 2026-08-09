namespace GasPowerAgentCustom;

public sealed class InMemoryVectorStore
{
    private readonly List<DocumentChunk> _chunks = [];

    public void Add(DocumentChunk chunk)
    {
        _chunks.Add(chunk);
    }

    public IReadOnlyList<SearchResult> Search(
        float[] queryEmbedding,
        int topK = 3)
    {
        return _chunks
            .Select(
                chunk => new SearchResult(
                    chunk,
                    CosineSimilarity(
                        queryEmbedding,
                        chunk.Embedding)))
            .OrderByDescending(x => x.Score)
            .Take(topK)
            .ToList();
    }

    private static double CosineSimilarity(
        float[] a,
        float[] b)
    {
        if (a.Length != b.Length)
        {
            throw new ArgumentException(
                "Vector dimensions differ.");
        }

        double dot = 0;
        double normA = 0;
        double normB = 0;

        for (var i = 0; i < a.Length; i++)
        {
            dot += a[i] * b[i];

            normA += a[i] * a[i];
            normB += b[i] * b[i];
        }

        if (normA == 0 || normB == 0)
        {
            return 0;
        }

        return dot /
               (Math.Sqrt(normA) *
                Math.Sqrt(normB));
    }
}

public sealed record SearchResult(
    DocumentChunk Chunk,
    double Score);
