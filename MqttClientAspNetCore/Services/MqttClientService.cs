﻿using Microsoft.Extensions.Configuration;
using MQTTnet;
using MQTTnet.Client;
using System.Text;
using MqttClientAspNetCore.Options;

namespace MqttClientAspNetCore.Services
{
    public class MqttClientService : BackgroundService
    {
        private readonly IConfiguration _config;
        ILogger<MqttClientService> _logger;
        IMqttClient _client;
        private MqttClientOptions _clientOptions;
        private MqttClientSubscribeOptions _subscriptionOptions;
        List<MqttClientSubscribeOptions> subscriptionOptions = new List<MqttClientSubscribeOptions>();

        public MqttClientService(ILogger<MqttClientService> logger, IConfiguration config)
        {
            _logger = logger;
            
            this._config = config;
            var clientSettings = _config.GetSection("MySettings:ClientSettings").Get<ClientSettings>();
            var factory = new MqttFactory();
            _client = factory.CreateMqttClient();
            _clientOptions = new MqttClientOptionsBuilder()
                            .WithTcpServer(clientSettings.Address, clientSettings.Port)
                            .Build();
            var deviceSection = _config.GetSection("MySettings:DeviceSettings");
            
            foreach (IConfigurationSection section in deviceSection.GetChildren())
            {
                var topic = section.GetValue<string>("DeviceId");
                _subscriptionOptions = factory.CreateSubscribeOptionsBuilder()
                        .WithTopicFilter(f =>
                        {
                            f.WithTopic(topic);
                            f.WithAtLeastOnceQoS();
                        })
                        .Build();
                subscriptionOptions.Add(_subscriptionOptions);
            };

            _client.ApplicationMessageReceivedAsync += HandleMessageAsync;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            await _client.ConnectAsync(_clientOptions, CancellationToken.None);
            _logger.LogInformation("Connected");
            foreach (MqttClientSubscribeOptions _subscriptionOptions in subscriptionOptions)
            {
                await _client.SubscribeAsync(_subscriptionOptions, CancellationToken.None);
            }
            _logger.LogInformation("Subscribed to topics");
        }
        public virtual async Task StopAsync(CancellationToken cancellationToken)
        {
            await _client.DisconnectAsync();
            await base.StopAsync(cancellationToken);
        }
        async Task HandleMessageAsync(MqttApplicationMessageReceivedEventArgs e)
        {
            var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
            var inTopic = e.ApplicationMessage.Topic;
            string respTopic = inTopic + "/resp";
            _logger.LogInformation("Received:{payload} from {inTopic}", payload, inTopic);
            var applicationMessage = new MqttApplicationMessageBuilder()
                            .WithTopic(respTopic)
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