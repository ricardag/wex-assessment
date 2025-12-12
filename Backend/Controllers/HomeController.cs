using Backend.Application.Dtos;
using Backend.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;


public class HomeController : ApiController
    {
    private readonly IAuthService _authService;

    public HomeController(IAuthService authService)
        {
        _authService = authService;
        }
    

    [AllowAnonymous]
    [Route("auth"), HttpPost]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Login([FromBody] LoginRequestDto request)
        {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Valid username and password are defined on .env file for simplicity
        var response = _authService.AuthenticateAsync(request.Username, request.Password);
        if (response == null)
            return Unauthorized(new { message = "Invalid username or password" });

        return Ok(response);
        }
    
    [Route("auth"), HttpPut]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult RefreshToken()
        {
        // Extract username from authenticated user context
        var username = User.Identity?.Name;

        if (string.IsNullOrEmpty(username))
            return Unauthorized(new { message = "Invalid user context" });

        var response = _authService.RefreshToken(username);
        if (response == null)
            return Unauthorized(new { message = "Failed to refresh token" });

        return Ok(response);
        }

    }