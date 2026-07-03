using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Seasbroker.Modules.Identity.Application.DTOs;
using Seasbroker.Modules.Identity.Application.Services;

namespace Seasbroker.Modules.Identity.Controllers;

[ApiController]
[Route("api/collections/_superusers")]
public class SuperusersAuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public SuperusersAuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("auth-with-password")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PocketBaseAuthResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> AuthWithPassword(
        [FromBody] PocketBaseLoginRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null ||
            string.IsNullOrWhiteSpace(request.Identity) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new AuthErrorResponse
            {
                Message = "An error occurred while validating the submitted data.",
                Status = StatusCodes.Status400BadRequest,
            });
        }

        try
        {
            var response = await _authService.LoginPocketBaseAsync(
                request.Identity,
                request.Password,
                cancellationToken);

            return Ok(response);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new AuthErrorResponse
            {
                Message = "Invalid login credentials.",
                Status = StatusCodes.Status400BadRequest,
            });
        }
    }

    [HttpPost("auth-refresh")]
    [Authorize]
    [ProducesResponseType(typeof(PocketBaseAuthResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> AuthRefresh(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new AuthErrorResponse
            {
                Message = "The request requires valid record authorization token.",
                Status = StatusCodes.Status401Unauthorized,
            });
        }

        try
        {
            var response = await _authService.RefreshPocketBaseAsync(userId, cancellationToken);
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
}
