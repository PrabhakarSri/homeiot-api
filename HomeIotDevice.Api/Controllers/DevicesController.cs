using System.Security.Claims;
using HomeIotDevice.Application.DTOs;
using HomeIotDevice.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeIotDevice.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DevicesController : ControllerBase
{
    private readonly IDeviceService _deviceService;

    public DevicesController(IDeviceService deviceService) => _deviceService = deviceService;

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DeviceResponse>>> GetAll()
    {
        var devices = await _deviceService.GetUserDevicesAsync(GetUserId());
        return Ok(devices);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DeviceResponse>> Get(Guid id)
    {
        try
        {
            var device = await _deviceService.GetDeviceAsync(id, GetUserId());
            return Ok(device);
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpPost]
    public async Task<ActionResult<DeviceResponse>> Create([FromBody] CreateDeviceRequest request)
    {
        var device = await _deviceService.AddDeviceAsync(GetUserId(), request);
        return CreatedAtAction(nameof(Get), new { id = device.Id }, device);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _deviceService.DeleteDeviceAsync(id, GetUserId());
            return NoContent();
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpPost("{id}/command")]
    public async Task<ActionResult<DeviceResponse>> SendCommand(
        Guid id, [FromBody] DeviceCommandRequest request)
    {
        try
        {
            var device = await _deviceService.SendCommandAsync(id, GetUserId(), request);
            return Ok(device);
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }
}
