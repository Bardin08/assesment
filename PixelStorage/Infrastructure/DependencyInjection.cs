using StackExchange.Redis;

namespace PixelStorage.Infrastructure;

internal static class DependencyInjection
{
    public static void AddInfrastructureDependencies(
        this IServiceCollection services, IConfiguration configuration)
    {
        var redisOptions = new RedisOptions();
        configuration.Bind("Redis", redisOptions);
        if (string.IsNullOrEmpty(redisOptions.ConnectionString))
        {
            throw new ArgumentException("Invalid connection string for Redis");
        }

        services.AddSingleton<IConnectionMultiplexer>(
            ConnectionMultiplexer.Connect(redisOptions.ConnectionString));
        services.AddHostedService<RedisSubscriberService>();
    }
}