namespace GasPowerAgent;

public sealed class DocumentIndexer(TextChunker chunker, EmbeddingService embeddings, InMemoryVectorStore store)
{
    public async Task IndexDirectoryAsync(string directory)
    {
        foreach (var path in Directory.GetFiles(directory, "*.txt"))
        {
            var chunks = chunker.Chunk(await File.ReadAllTextAsync(path));
            Console.WriteLine($"Indexing {Path.GetFileName(path)} ({chunks.Count} chunks)");
            for (var index = 0; index < chunks.Count; index++)
                store.Add(new DocumentChunk($"{Path.GetFileName(path)}:{index}", Path.GetFileName(path), chunks[index], await embeddings.CreateAsync(chunks[index])));
        }
    }
}
