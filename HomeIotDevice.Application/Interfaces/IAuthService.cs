using HomeIotDevice.Application.DTOs;

namespace HomeIotDevice.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> SignupAsync(SignupRequest request);
}
