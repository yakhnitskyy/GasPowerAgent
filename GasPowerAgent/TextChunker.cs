namespace GasPowerAgent;

public sealed class TextChunker(int maxCharacters = 800)
{
    public IReadOnlyList<string> Chunk(string text)
    {
        var chunks = new List<string>();
        var current = "";
        foreach (var paragraph in text.Split("\n\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (current.Length + paragraph.Length + 2 > maxCharacters && current.Length > 0) { chunks.Add(current); current = paragraph; }
            else current = current.Length == 0 ? paragraph : $"{current}\n\n{paragraph}";
        }
        if (current.Length > 0) chunks.Add(current);
        return chunks;
    }
}
