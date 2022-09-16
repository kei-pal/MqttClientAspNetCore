using MQTTnet;
using MQTTnet.Client;
using System.Text;

namespace MqttClientAspNetCore.Services
{
    public class MqttClientService : BackgroundService
    {
        ILogger<MqttClientService> _logger;
        IMqttClient _client;
        private MqttClientOptions _clientOptions;
        private MqttClientSubscribeOptions _subscriptionOptions;

        public MqttClientService(ILogger<MqttClientService> logger)
        {
            _logger = logger;
            var factory = new MqttFactory();
            _client = factory.CreateMqttClient();
            _clientOptions = new MqttClientOptionsBuilder()
                            .WithTcpServer("test.mosquitto.org", 1883)
                            .Build();
            _subscriptionOptions = factory.CreateSubscribeOptionsBuilder()
                        .WithTopicFilter(f =>
                        {
                            f.WithTopic("keipalatest/post");
                            f.WithAtLeastOnceQoS();
                        })
                        .Build();
            _client.ApplicationMessageReceivedAsync += HandleMessageAsync;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            await _client.ConnectAsync(_clientOptions, CancellationToken.None);
            _logger.LogInformation("Connected");

            await _client.SubscribeAsync(_subscriptionOptions, CancellationToken.None);
            _logger.LogInformation("Subscribed");
        }
        public virtual async Task StopAsync(CancellationToken cancellationToken)
        {
            await _client.DisconnectAsync();
            await base.StopAsync(cancellationToken);
        }
        async Task HandleMessageAsync(MqttApplicationMessageReceivedEventArgs e)
        {
            var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
            _logger.LogInformation("### RECEIVED APPLICATION MESSAGE ###\n{payload}", payload);
            var applicationMessage = new MqttApplicationMessageBuilder()
                            .WithTopic("keipalatest/resp")
                            .WithPayload("OK")
                            .Build();

            await _client.PublishAsync(applicationMessage, CancellationToken.None);

            _logger.LogInformation("MQTT application message is published.");
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _client.Dispose();
                base.Dispose();
            }
            _client = null;
        }
    }
}