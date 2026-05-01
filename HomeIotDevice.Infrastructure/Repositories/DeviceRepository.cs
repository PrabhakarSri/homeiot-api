using HomeIotDevice.Domain.Entities;
using HomeIotDevice.Domain.Interfaces;
using HomeIotDevice.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HomeIotDevice.Infrastructure.Repositories;

public class DeviceRepository : IDeviceRepository
{
    private readonly AppDbContext _context;

    public DeviceRepository(AppDbContext context) => _context = context;

    public async Task<Device?> GetByIdAsync(Guid id) =>
        await _context.Devices.FindAsync(id);

    public async Task<IEnumerable<Device>> GetByUserIdAsync(Guid userId) =>
        await _context.Devices.Where(d => d.UserId == userId).ToListAsync();

    public async Task<Device> CreateAsync(Device device)
    {
        _context.Devices.Add(device);
        await _context.SaveChangesAsync();
        return device;
    }

    public async Task<Device> UpdateAsync(Device device)
    {
        _context.Devices.Update(device);
        await _context.SaveChangesAsync();
        return device;
    }

    public async Task DeleteAsync(Guid id)
    {
        var device = await _context.Devices.FindAsync(id);
        if (device != null)
        {
            _context.Devices.Remove(device);
            await _context.SaveChangesAsync();
        }
    }
}
