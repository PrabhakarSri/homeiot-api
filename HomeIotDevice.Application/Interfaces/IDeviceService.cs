using HomeIotDevice.Application.DTOs;

namespace HomeIotDevice.Application.Interfaces;

public interface IDeviceService
{
    Task<IEnumerable<DeviceResponse>> GetUserDevicesAsync(Guid userId);
    Task<DeviceResponse> GetDeviceAsync(Guid deviceId, Guid userId);
    Task<DeviceResponse> AddDeviceAsync(Guid userId, CreateDeviceRequest request);
    Task DeleteDeviceAsync(Guid deviceId, Guid userId);
    Task<DeviceResponse> SendCommandAsync(Guid deviceId, Guid userId, DeviceCommandRequest request);
}
