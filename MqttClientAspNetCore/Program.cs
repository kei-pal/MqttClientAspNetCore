using MqttClientAspNetCore.Services;

var builder = WebApplication.CreateBuilder(args);

var logger = LoggerFactory.Create(config =>
{
    config.AddConsole();
}).CreateLogger("Program");

builder.Services.AddHostedService<MqttClientService>();

var app = builder.Build();

// To check if web server is still responsive
app.MapGet("/", () =>
{
    logger.LogInformation("Get '/' worked");
    return "Hello World";
});


app.Run();
