using Microsoft.Extensions.Hosting;
using StackExchange.Redis;

namespace Pixel.Shared.Infrastructure;

public class RedisSubscriberService(IConnectionMultiplexer redisConnection) : BackgroundService
{
    private const string ChannelName = "trackers";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var subscriber = redisConnection.GetSubscriber();
        await subscriber.SubscribeAsync(ChannelName, (channel, message) => {
            Console.WriteLine($"Message from {channel}: {message}");
        });

        while (!stoppingToken.IsCancellationRequested)
        {
            // Keep the service running
            await Task.Delay(1000, stoppingToken);
        }
    }
}