using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Pixel;
using Pixel.Shared.Contracts;
using Pixel.Shared.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .AddEnvironmentVariables();

builder.Services.AddRedisDependencies(builder.Configuration);
builder.Services.AddTransient<RedisPublisher>();
builder.Services.AddTransient<IpAddressProvider>();

var app = builder.Build();

app.MapGet("/track", async (HttpRequest request,
    [FromServices] IpAddressProvider ipAddressProvider,
    [FromServices] IOptions<RedisOptions> redisOptions,
    [FromServices] RedisPublisher publisher) =>
{
    const string content = "R0lGODdhAQABAIEAAP///wAAAAAAAAAAACwAAAAAAQABAAAIBAABBAQAOw==";
    var redisConfiguration = redisOptions.Value;

    var message = new TrackerRecord(
        DateTimeOffset.UtcNow,
        request.Headers[HeaderNames.UserAgent].FirstOrDefault(),
        request.Headers[HeaderNames.Referer].FirstOrDefault(),
        ipAddressProvider.GetUserIp(request));

    await publisher.PublishAsync(redisConfiguration.TrackerRecordsChannel,
        JsonSerializer.Serialize(message));
    return Results.File(Encoding.UTF8.GetBytes(content), "image/gif");
});

app.Run();