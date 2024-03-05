using Microsoft.Extensions.Options;
using Pixel.Shared.Infrastructure;
using StackExchange.Redis;

namespace PixelStorage.Infrastructure;

public class RedisConsumer(
    IConnectionMultiplexer redisConnection,
    IOptions<RedisOptions> redisOptions, 
    ITrackerRecordRepository recordRepository)
    : BackgroundService
{
    private readonly RedisOptions _redisConfiguration = redisOptions.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // ReSharper disable once InconsistentlySynchronizedField
        stoppingToken.Register(recordRepository.Dispose);

        var subscriber = redisConnection.GetSubscriber();
        await subscriber.SubscribeAsync(
            _redisConfiguration.TrackerRecordsChannel,
            recordRepository.SaveTrackerRecord);

        while (!stoppingToken.IsCancellationRequested)
        {
            // Keep the service running
            await Task.Delay(1000, stoppingToken);
        }
    }
}