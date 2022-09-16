namespace MqttClientAspNetCore.Options
{
    public class MqttSubscriptionOptions
    {
        public const string MqttSubscription = "MqttSubscription";

        public string Topic { get; set; } = String.Empty;
    }
}
