namespace Pixel.Shared.Infrastructure;

public record RedisOptions
{
    public const string SectionName = "Redis";
    public required string ConnectionString { get; init; }
    public required string TrackerRecordsChannel { get; init; }
}