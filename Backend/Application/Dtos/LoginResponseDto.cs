namespace Backend.Application.Dtos;

public class LoginResponseDto
    {
    public string Token { get; init; } = string.Empty;
    public string TokenType { get; init; } = "Bearer";
    public DateTime Expires { get; init; }
    }
