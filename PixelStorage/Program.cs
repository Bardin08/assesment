using Pixel.Shared.Infrastructure;

var builder = WebApplication
    .CreateSlimBuilder();

builder.Services.AddInfrastructureDependencies(builder.Configuration);
builder.Services.AddHostedService<RedisSubscriberService>();

var app = builder.Build();

app.Run();
