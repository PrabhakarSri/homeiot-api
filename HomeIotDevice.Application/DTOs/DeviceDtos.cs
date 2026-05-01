namespace HomeIotDevice.Application.DTOs;

public record CreateDeviceRequest(string Name, string DeviceType);
public record DeviceCommandRequest(string Command, int? Value);

public record DeviceResponse(
    Guid Id,
    string Name,
    string DeviceType,
    string MqttTopic,
    bool IsOnline,
    bool IsOn,
    int BrightnessLevel,
    int SpeedLevel,
    DateTime? LastSeenAt
);
