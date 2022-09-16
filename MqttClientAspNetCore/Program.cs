using MqttClientAspNetCore.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHostedService<MqttClientService>();

var app = builder.Build();

// To check if web server is still responsive
app.MapGet("/", () =>
{
    Console.WriteLine("Working");
    return "Hello World";
});


app.Run();
