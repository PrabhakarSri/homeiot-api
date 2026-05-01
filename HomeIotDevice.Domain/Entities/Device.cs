namespace HomeIotDevice.Domain.Entities;

public class Device
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string DeviceType { get; set; } = string.Empty; // Light, Fan, Thermostat, etc.
    public string MqttTopic { get; set; } = string.Empty;
    public bool IsOnline { get; set; }
    public bool IsOn { get; set; }
    public int BrightnessLevel { get; set; } // 0-100 for lights
    public int SpeedLevel { get; set; } // 0-5 for fans
    public string LastKnownState { get; set; } = "{}";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastSeenAt { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
}
