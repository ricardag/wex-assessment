using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Backend.Application.Configuration;
using Backend.Application.Dtos;
using Backend.Application.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace Backend.Application.Services;

public class AuthService : IAuthService
    {
    private readonly JwtSettings _jwtSettings;
    private readonly string _authUsername;
    private readonly string _authPassword;
    private const int TokenExpirationSeconds = 300; // 5 minutes

    public AuthService(JwtSettings jwtSettings)
        {
        _jwtSettings = jwtSettings;

        _authUsername = Environment.GetEnvironmentVariable("AUTH_USERNAME")
                        ?? throw new InvalidOperationException("AUTH_USERNAME environment variable is required");

        _authPassword = Environment.GetEnvironmentVariable("AUTH_PASSWORD")
                        ?? throw new InvalidOperationException("AUTH_PASSWORD environment variable is required");
        }

    public LoginResponseDto? AuthenticateAsync(string username, string password)
        {
        // Validate credentials
        if (username != _authUsername || password != _authPassword)
            return null;

        // Calculate expiration time (absolute UTC)
        var expiresAt = DateTime.UtcNow.AddSeconds(TokenExpirationSeconds);

        // Generate JWT token
        var token = GenerateJwtToken(username, expiresAt);

       return new LoginResponseDto
            {
            Token = token,
            TokenType = "Bearer",
            Expires = expiresAt
            };
        }

    public LoginResponseDto? RefreshToken(string username)
        {
        if (string.IsNullOrEmpty(username))
            return null;

        // Calculate new expiration time using RefreshTokenExpirationMinutes
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.RefreshTokenExpirationMinutes);

        // Generate new JWT token
        var newToken = GenerateJwtToken(username, expiresAt);

        return new LoginResponseDto
            {
            Token = newToken,
            TokenType = "Bearer",
            Expires = expiresAt
            };
        }

    private string GenerateJwtToken(string username, DateTime expiresAt)
        {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);

        var tokenDescriptor = new SecurityTokenDescriptor
            {
            Subject = new ClaimsIdentity([
                new Claim(ClaimTypes.Name, username),
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                ]),
            Expires = expiresAt,
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
            };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
        }
    }
