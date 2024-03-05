using Pixel.Shared.Infrastructure;

var builder = WebApplication
    .CreateSlimBuilder();

builder.Services.AddInfrastructureDependencies(builder.Configuration);
var app = builder.Build();

app.Run();
