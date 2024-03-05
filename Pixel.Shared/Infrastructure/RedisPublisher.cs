using StackExchange.Redis;

namespace Pixel.Shared.Infrastructure;

public class RedisPublisher(IConnectionMultiplexer redisConnection)
{
    public async Task PublishAsync(string channel, string message)
    {
        // no error handling, because if something wrong with messages publish I'd like this fail-fast
        var subscriber = redisConnection.GetSubscriber();
        await subscriber.PublishAsync(channel, message);
    }
}