namespace HomeIotDevice.Application.Interfaces;

public interface IMqttService
{
    Task PublishAsync(string topic, string payload);
    Task SubscribeAsync(string topic, Func<string, string, Task> handler);
    Task UnsubscribeAsync(string topic);
}
