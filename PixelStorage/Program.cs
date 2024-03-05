using Pixel.Shared.Infrastructure;
using PixelStorage.Infrastructure;

var builder = WebApplication
    .CreateSlimBuilder();

builder.Services.AddRedisDependencies(builder.Configuration);
builder.Services.AddHostedService<RedisConsumer>();

builder.Services.Configure<FileStorageOptions>(
    builder.Configuration.GetSection(FileStorageOptions.SectionName), o =>
    {
        o.ErrorOnUnknownConfiguration = true;
    });
builder.Services.AddSingleton<TrackerRecordRepository>();

var app = builder.Build();

app.Run();
