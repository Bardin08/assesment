namespace PixelStorage.Infrastructure;

public record FileStorageOptions
{
    public const string SectionName = "FileStorage";
    public required string FilePath { get; init; }
    public int BufferSize { get; set; }
}