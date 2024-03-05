using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Pixel.Shared.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructureDependencies(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RedisOptions>(
            configuration.GetSection(RedisOptions.SectionName), o =>
            {
                o.ErrorOnUnknownConfiguration = true;
            });

        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var redisOptions = sp.GetRequiredService<IOptions<RedisOptions>>();
            var redisConfiguration = redisOptions.Value;
            if (string.IsNullOrEmpty(redisConfiguration.ConnectionString))
            {
                throw new ArgumentException("Invalid Redis connection string");
            }

            return ConnectionMultiplexer.Connect(redisConfiguration.ConnectionString);
        });
    }
}