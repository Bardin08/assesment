using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Pixel.Shared.Contracts;
using Pixel.Shared.Infrastructure;
using StackExchange.Redis;

namespace PixelStorage.Infrastructure;

public class RedisSubscriberService(
    IConnectionMultiplexer redisConnection,
    IOptions<RedisOptions> redisOptions,
    IOptions<FileStorageOptions> fileStorageOptions)
    : BackgroundService
{
    private readonly RedisOptions _redisConfiguration = redisOptions.Value;
    private readonly StreamWriter _streamWriter = GetStreamWriter(fileStorageOptions);
    private readonly object _lockObject = new();
    private int _wroteRecords;

    private static StreamWriter GetStreamWriter(IOptions<FileStorageOptions> fileStorageOptions)
    {
        const int oneKb = 1024;

        ArgumentNullException.ThrowIfNull(fileStorageOptions.Value);
        ArgumentNullException.ThrowIfNull(fileStorageOptions.Value.FilePath);

        var fullPath = Path.GetFullPath(fileStorageOptions.Value.FilePath);

        var directory = Path.GetDirectoryName(fullPath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory!);
        }

        var bufferSize = fileStorageOptions.Value.BufferSize < oneKb
            ? oneKb
            : fileStorageOptions.Value.BufferSize;

        return new StreamWriter(fullPath, append: true,
            Encoding.UTF8, bufferSize);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // ReSharper disable once InconsistentlySynchronizedField
        stoppingToken.Register(() => _streamWriter.Dispose());

        var subscriber = redisConnection.GetSubscriber();
        await subscriber.SubscribeAsync(
            _redisConfiguration.TrackerRecordsChannel,
            async (c, m) => await SaveTrackerRecordAsync(c, m));

        while (!stoppingToken.IsCancellationRequested)
        {
            // Keep the service running
            await Task.Delay(1000, stoppingToken);
        }
    }

    private async Task SaveTrackerRecordAsync(RedisChannel _, RedisValue message)
    {
        if (!message.HasValue)
        {
            return;
        }

        var record = JsonSerializer.Deserialize<TrackerRecord>(message!);
        if (record is null)
        {
            return;
        }
        else if (string.IsNullOrEmpty(record.IpAddress))
        {
            // no need to save records without IP_Address,
            // according to the requirements it's the only mandatory field. 
            return;
        }

        var recordStr = string.Join("|",
            [
                record.Timestamp.ToString("O"),
                record.Referer ?? "null",
                record.UserAgent ?? "null",
                record.IpAddress
            ]
        );

        lock (_lockObject)
        {
            _streamWriter.WriteLine(recordStr);
            Interlocked.Add(ref _wroteRecords, 1);

            if (_wroteRecords > 1000)
            {
                _streamWriter.Flush();
                Interlocked.Exchange(ref _wroteRecords, 0);
            }
        }
    }

    public override void Dispose()
    {
        base.Dispose();
        _streamWriter?.Dispose();
    }
}