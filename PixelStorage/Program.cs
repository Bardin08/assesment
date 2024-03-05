using Pixel.Shared.Infrastructure;
using PixelStorage.Infrastructure;

var builder = WebApplication
    .CreateSlimBuilder();

builder.Services.AddRedisDependencies(builder.Configuration);
builder.Services.AddHostedService<RedisSubscriberService>();

builder.Services.Configure<FileStorageOptions>(
    builder.Configuration.GetSection(FileStorageOptions.SectionName), o =>
    {
        o.ErrorOnUnknownConfiguration = true;
    });

var app = builder.Build();

app.Run();
