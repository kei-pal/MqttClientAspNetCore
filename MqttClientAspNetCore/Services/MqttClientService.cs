using MQTTnet;
using MQTTnet.Client;
using System.Text;

namespace MqttClientAspNetCore.Services
{
    public class MqttClientService : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // TODO: Handle application reconnect logic
                await Handle_Received_Application_Message();
            }
        }

        public static async Task Handle_Received_Application_Message()
        {

            var mqttFactory = new MqttFactory();

            using (var mqttClient = mqttFactory.CreateMqttClient())
            {
                var mqttClientOptions = new MqttClientOptionsBuilder()
                    .WithTcpServer("test.mosquitto.org")
                    .Build();

                // Setup message handling before connecting so that queued messages
                // are also handled properly. When there is no event handler attached all
                // received messages get lost.
                mqttClient.ApplicationMessageReceivedAsync += e =>
                {
                    Console.WriteLine("### RECEIVED APPLICATION MESSAGE ###");
                    Console.WriteLine($"+ Payload = {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");

                    var applicationMessage = new MqttApplicationMessageBuilder()
                        .WithTopic("keipalatest/1/resp")
                        .WithPayload("Tag1:OK")
                        .Build();

                    mqttClient.PublishAsync(applicationMessage, CancellationToken.None);

                    Console.WriteLine("MQTT application message is published.");

                    return Task.CompletedTask;
                };

                await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

                var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
                    .WithTopicFilter(f =>
                    {
                        f.WithTopic("keipalatest/1/post");
                        f.WithAtLeastOnceQoS();
                    })
                    .Build();

                await mqttClient.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);

                Console.WriteLine("MQTT client subscribed to topic.");

                // TODO: Remove and use proper logic to keep from restarting
                Console.WriteLine("Press enter to exit.");
                Console.ReadLine();
            }
        }
    }
}
