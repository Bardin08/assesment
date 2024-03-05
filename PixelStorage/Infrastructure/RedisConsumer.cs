using Microsoft.Extensions.Options;
using Pixel.Shared.Infrastructure;
using StackExchange.Redis;

namespace PixelStorage.Infrastructure;

public class RedisConsumer(
    IConnectionMultiplexer redisConnection,
    IOptions<RedisOptions> redisOptions, 
    TrackerRecordRepository recordRepository,
    ILogger<RedisConsumer> logger)
    : BackgroundService
{
    private readonly RedisOptions _redisConfiguration = redisOptions.Value;
    private int exceptionsCount = 0;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
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
        catch (Exception e)
        {
            exceptionsCount++;
            logger.LogError(e, "Exception occured in Redis Consumer");

            if (exceptionsCount > 10)
            {
                throw;
            }
        }
    }
}