namespace Task3.Library;

public record Config(int Capacity, int ChunkSize, TimeSpan Timeout, TimeSpan TimeoutChunkSpan);