using MqttClientAspNetCore.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHostedService<MqttClientService>();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
