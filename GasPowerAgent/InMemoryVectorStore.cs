namespace GasPowerAgent;

public sealed class InMemoryVectorStore
{
    private readonly List<DocumentChunk> _chunks = [];
    public void Add(DocumentChunk chunk) => _chunks.Add(chunk);
    public IReadOnlyList<SearchResult> Search(float[] query, int topK) => 
        _chunks
            .Select(chunk => new SearchResult(chunk, CosineSimilarity(query, chunk.Embedding)))
            .OrderByDescending(x => x.Score)
            .Take(topK)
            .ToList();
    private static double CosineSimilarity(float[] a, float[] b)
    {
        if (a.Length != b.Length) throw new ArgumentException("Vector dimensions differ.");
        double dot = 0, normA = 0, normB = 0;
        for (var index = 0; index < a.Length; index++) { dot += a[index] * b[index]; normA += a[index] * a[index]; normB += b[index] * b[index]; }
        return normA == 0 || normB == 0 ? 0 : dot / (Math.Sqrt(normA) * Math.Sqrt(normB));
    }
}
