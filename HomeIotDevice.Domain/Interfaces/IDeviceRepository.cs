using HomeIotDevice.Domain.Entities;

namespace HomeIotDevice.Domain.Interfaces;

public interface IDeviceRepository
{
    Task<Device?> GetByIdAsync(Guid id);
    Task<IEnumerable<Device>> GetByUserIdAsync(Guid userId);
    Task<Device> CreateAsync(Device device);
    Task<Device> UpdateAsync(Device device);
    Task DeleteAsync(Guid id);
}
