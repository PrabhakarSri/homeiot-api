using HomeIotDevice.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;

namespace HomeIotDevice.Infrastructure.Services;

public class MqttService : IMqttService, IAsyncDisposable
{
    private readonly MqttClient _client;
    private readonly ILogger<MqttService> _logger;
    private readonly Dictionary<string, Func<string, string, Task>> _handlers = new();
    private readonly IConfiguration _configuration;

    public MqttService(IConfiguration configuration, ILogger<MqttService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        var factory = new MqttFactory();
        _client = (MqttClient)factory.CreateMqttClient();

        _client.ApplicationMessageReceivedAsync += async e =>
        {
            var topic = e.ApplicationMessage.Topic;
            var payload = System.Text.Encoding.UTF8.GetString(
                e.ApplicationMessage.PayloadSegment);

            if (_handlers.TryGetValue(topic, out var handler))
            {
                try { await handler(topic, payload); }
                catch (Exception ex) { _logger.LogError(ex, "MQTT handler error for {Topic}", topic); }
            }
        };
    }

    public async Task ConnectAsync()
    {
        var broker = _configuration["Mqtt:Broker"] ?? "localhost";
        var port = int.Parse(_configuration["Mqtt:Port"] ?? "1883");

        var options = new MqttClientOptionsBuilder()
            .WithTcpServer(broker, port)
            .WithClientId($"HomeIotApi-{Guid.NewGuid():N}")
            .WithCleanSession()
            .Build();

        try
        {
            await _client.ConnectAsync(options);
            _logger.LogInformation("Connected to MQTT broker at {Broker}:{Port}", broker, port);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "MQTT broker not available. Running in offline mode.");
        }
    }

    public async Task PublishAsync(string topic, string payload)
    {
        if (!_client.IsConnected)
        {
            _logger.LogWarning("MQTT not connected. Command queued for {Topic}", topic);
            return;
        }

        var message = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(payload)
            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
            .WithRetainFlag(true)
            .Build();

        await _client.PublishAsync(message);
        _logger.LogInformation("Published to {Topic}", topic);
    }

    public async Task SubscribeAsync(string topic, Func<string, string, Task> handler)
    {
        _handlers[topic] = handler;

        if (_client.IsConnected)
        {
            await _client.SubscribeAsync(new MqttClientSubscribeOptionsBuilder()
                .WithTopicFilter(topic)
                .Build());
            _logger.LogInformation("Subscribed to {Topic}", topic);
        }
    }

    public async Task UnsubscribeAsync(string topic)
    {
        _handlers.Remove(topic);

        if (_client.IsConnected)
        {
            await _client.UnsubscribeAsync(new MqttClientUnsubscribeOptionsBuilder()
                .WithTopicFilter(topic)
                .Build());
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_client.IsConnected)
            await _client.DisconnectAsync();
        _client.Dispose();
    }
}
