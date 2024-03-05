using StackExchange.Redis;

namespace Pixel.Infrastructure;

public class RedisPublisher(IConnectionMultiplexer redisConnection)
{
    public async Task PublishAsync(string channel, string message)
    {
        var subscriber = redisConnection.GetSubscriber();
        await subscriber.PublishAsync(channel, message);
    }
}