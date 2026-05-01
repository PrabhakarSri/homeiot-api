using System.Text.Json;
using HomeIotDevice.Application.DTOs;
using HomeIotDevice.Application.Interfaces;
using HomeIotDevice.Domain.Entities;
using HomeIotDevice.Domain.Interfaces;

namespace HomeIotDevice.Application.Services;

public class DeviceService : IDeviceService
{
    private readonly IDeviceRepository _deviceRepository;
    private readonly IMqttService _mqttService;

    public DeviceService(IDeviceRepository deviceRepository, IMqttService mqttService)
    {
        _deviceRepository = deviceRepository;
        _mqttService = mqttService;
    }

    public async Task<IEnumerable<DeviceResponse>> GetUserDevicesAsync(Guid userId)
    {
        var devices = await _deviceRepository.GetByUserIdAsync(userId);
        return devices.Select(MapToResponse);
    }

    public async Task<DeviceResponse> GetDeviceAsync(Guid deviceId, Guid userId)
    {
        var device = await _deviceRepository.GetByIdAsync(deviceId)
            ?? throw new KeyNotFoundException("Device not found.");
        if (device.UserId != userId)
            throw new UnauthorizedAccessException("Access denied.");
        return MapToResponse(device);
    }

    public async Task<DeviceResponse> AddDeviceAsync(Guid userId, CreateDeviceRequest request)
    {
        var device = new Device
        {
            Name = request.Name,
            DeviceType = request.DeviceType,
            MqttTopic = $"home/{userId}/{Guid.NewGuid():N}",
            UserId = userId
        };

        var created = await _deviceRepository.CreateAsync(device);

        await _mqttService.SubscribeAsync($"{device.MqttTopic}/status", async (topic, payload) =>
        {
            // Handle device status updates
            var existing = await _deviceRepository.GetByIdAsync(device.Id);
            if (existing != null)
            {
                existing.LastKnownState = payload;
                existing.LastSeenAt = DateTime.UtcNow;
                existing.IsOnline = true;
                await _deviceRepository.UpdateAsync(existing);
            }
        });

        return MapToResponse(created);
    }

    public async Task DeleteDeviceAsync(Guid deviceId, Guid userId)
    {
        var device = await _deviceRepository.GetByIdAsync(deviceId)
            ?? throw new KeyNotFoundException("Device not found.");
        if (device.UserId != userId)
            throw new UnauthorizedAccessException("Access denied.");

        await _mqttService.UnsubscribeAsync($"{device.MqttTopic}/status");
        await _deviceRepository.DeleteAsync(deviceId);
    }

    public async Task<DeviceResponse> SendCommandAsync(Guid deviceId, Guid userId, DeviceCommandRequest request)
    {
        var device = await _deviceRepository.GetByIdAsync(deviceId)
            ?? throw new KeyNotFoundException("Device not found.");
        if (device.UserId != userId)
            throw new UnauthorizedAccessException("Access denied.");

        switch (request.Command.ToLower())
        {
            case "toggle":
                device.IsOn = !device.IsOn;
                break;
            case "turn_on":
                device.IsOn = true;
                break;
            case "turn_off":
                device.IsOn = false;
                break;
            case "set_brightness":
                device.BrightnessLevel = Math.Clamp(request.Value ?? 0, 0, 100);
                break;
            case "set_speed":
                device.SpeedLevel = Math.Clamp(request.Value ?? 0, 0, 5);
                break;
        }

        device.UpdatedAt = DateTime.UtcNow;
        await _deviceRepository.UpdateAsync(device);

        var payload = JsonSerializer.Serialize(new
        {
            command = request.Command,
            value = request.Value,
            isOn = device.IsOn,
            brightness = device.BrightnessLevel,
            speed = device.SpeedLevel
        });

        await _mqttService.PublishAsync($"{device.MqttTopic}/command", payload);

        return MapToResponse(device);
    }

    private static DeviceResponse MapToResponse(Device d) => new(
        d.Id, d.Name, d.DeviceType, d.MqttTopic,
        d.IsOnline, d.IsOn, d.BrightnessLevel, d.SpeedLevel, d.LastSeenAt
    );
}
