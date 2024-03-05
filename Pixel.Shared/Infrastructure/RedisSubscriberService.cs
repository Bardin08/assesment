using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Pixel.Shared.Infrastructure;

public class RedisSubscriberService(
    IConnectionMultiplexer redisConnection,
    IOptions<RedisOptions> redisOptions) : BackgroundService
{
    private readonly RedisOptions _redisConfiguration = redisOptions.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var subscriber = redisConnection.GetSubscriber();
        await subscriber.SubscribeAsync(
            _redisConfiguration.TrackerRecordsChannel,
            SaveTrackerRecord);

        while (!stoppingToken.IsCancellationRequested)
        {
            // Keep the service running
            await Task.Delay(1000, stoppingToken);
        }
    }

    private static void SaveTrackerRecord(RedisChannel channel, RedisValue message)
    {
        Console.WriteLine($"Message from {channel}: {message}");
    }
}