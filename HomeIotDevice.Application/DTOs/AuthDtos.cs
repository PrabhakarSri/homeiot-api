namespace HomeIotDevice.Application.DTOs;

public record LoginRequest(string Email, string Password);
public record SignupRequest(string FullName, string Email, string Password);
public record AuthResponse(string Token, string FullName, string Email, DateTime ExpiresAt);
