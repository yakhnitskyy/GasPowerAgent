namespace GasPowerAgentCustom;

public sealed class DocumentIndexer
{
    private readonly TextChunker _chunker;
    private readonly EmbeddingService _embeddings;
    private readonly InMemoryVectorStore _store;

    public DocumentIndexer(
        TextChunker chunker,
        EmbeddingService embeddings,
        InMemoryVectorStore store)
    {
        _chunker = chunker;
        _embeddings = embeddings;
        _store = store;
    }

    public async Task IndexDirectoryAsync(
        string directory)
    {
        foreach (var path
                 in Directory.GetFiles(
                     directory,
                     "*.txt"))
        {
            var text =
                await File.ReadAllTextAsync(path);

            var chunks =
                _chunker.Chunk(text);

            Console.WriteLine(
                $"Indexing {Path.GetFileName(path)} " +
                $"({chunks.Count} chunks)");

            for (var i = 0;
                 i < chunks.Count;
                 i++)
            {
                var chunk = chunks[i];

                var embedding =
                    await _embeddings.CreateAsync(chunk);

                _store.Add(
                    new DocumentChunk(
                        Id:
                        $"{Path.GetFileName(path)}:{i}",
                        Source:
                        Path.GetFileName(path),
                        Text:
                        chunk,
                        Embedding:
                        embedding));
            }
        }
    }
}
