namespace GasPowerAgentCustom;

public sealed record DocumentChunk(
    string Id,
    string Source,
    string Text,
    float[] Embedding);
