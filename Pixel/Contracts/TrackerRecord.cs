namespace Pixel.Contracts;

public record TrackerRecord(
    DateTimeOffset Timestamp,
    string? UserAgent,
    string? Referer,
    string IpAddress);