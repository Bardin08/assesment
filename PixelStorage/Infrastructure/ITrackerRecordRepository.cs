using StackExchange.Redis;

namespace PixelStorage.Infrastructure;

public interface ITrackerRecordRepository : IDisposable
{
    void SaveTrackerRecord(RedisChannel _, RedisValue message);
}