using MQTTnet;
using MQTTnet.Client;
using System.Text;

namespace MqttClientAspNetCore.Services
{
    public class MqttClientService : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            try
            {
                await Handle_Received_Application_Message(cancellationToken);
            }
            catch (OperationCanceledException) { }
        }

        public static async Task Handle_Received_Application_Message(CancellationToken cancellationToken)
        {

            var mqttFactory = new MqttFactory();

            using (var mqttClient = mqttFactory.CreateMqttClient())
            {
                var mqttClientOptions = new MqttClientOptionsBuilder()
                    .WithTcpServer("test.mosquitto.org")
                    .Build();

                // Setup message handling before connecting so that queued messages
                // are also handled properly. 
                mqttClient.ApplicationMessageReceivedAsync += e =>
                {
                    Console.WriteLine("### RECEIVED APPLICATION MESSAGE ###");
                    Console.WriteLine($"+ Payload = {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");

                    // Publish successful message in response
                    var applicationMessage = new MqttApplicationMessageBuilder()
                        .WithTopic("keipalatest/1/resp")
                        .WithPayload("OK")
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

                await Task.Delay(Timeout.Infinite, cancellationToken);
            }
        }
    }
}
