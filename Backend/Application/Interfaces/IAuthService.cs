using Backend.Application.Dtos;

namespace Backend.Application.Interfaces;

public interface IAuthService
    {
    LoginResponseDto? AuthenticateAsync(string username, string password);
    LoginResponseDto? RefreshToken(string username);
    }
