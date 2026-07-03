using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Seasbroker.Modules.Identity.Application.DTOs;
using Seasbroker.Modules.Identity.Application.Services;

namespace Seasbroker.Modules.Identity.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new AuthErrorResponse
            {
                Message = "Email and password are required.",
                Status = StatusCodes.Status400BadRequest,
            });
        }

        try
        {
            var response = await _authService.LoginAsync(request.Email, request.Password, cancellationToken);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new AuthErrorResponse
            {
                Message = ex.Message,
                Status = StatusCodes.Status401Unauthorized,
            });
        }
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return BadRequest(new AuthErrorResponse
            {
                Message = "Refresh token is required.",
                Status = StatusCodes.Status400BadRequest,
            });
        }

        try
        {
            var response = await _authService.RefreshAsync(request.RefreshToken, cancellationToken);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new AuthErrorResponse
            {
                Message = ex.Message,
                Status = StatusCodes.Status401Unauthorized,
            });
        }
    }

    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(
        [FromBody] LogoutRequest request,
        CancellationToken cancellationToken)
    {
        if (request is not null && !string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if (userIdClaim is not null && Guid.TryParse(userIdClaim, out var userId))
            {
                await _authService.LogoutAsync(userId, request.RefreshToken, cancellationToken);
            }
        }

        return NoContent();
    }
}
