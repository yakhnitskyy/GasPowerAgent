namespace GasPowerAgentCustom;

public sealed class TextChunker
{
    private readonly int _maxCharacters;

    public TextChunker(
        int maxCharacters = 800)
    {
        _maxCharacters = maxCharacters;
    }

    public IReadOnlyList<string> Chunk(
        string text)
    {
        var paragraphs =
            text.Split(
                "\n\n",
                StringSplitOptions.RemoveEmptyEntries |
                StringSplitOptions.TrimEntries);

        var chunks = new List<string>();

        var current = "";

        foreach (var paragraph in paragraphs)
        {
            if (current.Length +
                paragraph.Length + 2 >
                _maxCharacters)
            {
                if (!string.IsNullOrWhiteSpace(current))
                {
                    chunks.Add(current);
                }

                current = paragraph;
            }
            else
            {
                if (current.Length > 0)
                {
                    current += "\n\n";
                }

                current += paragraph;
            }
        }

        if (!string.IsNullOrWhiteSpace(current))
        {
            chunks.Add(current);
        }

        return chunks;
    }
}
