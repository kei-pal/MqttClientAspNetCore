namespace MqttClientAspNetCore.Options
{
    public class MqttClientOptions
    {
        public const string MqttClient = "MqttClient";
        
        public string Address { get; set; } = String.Empty;
    }
}
