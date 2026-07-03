using Microsoft.AspNetCore.Identity;
using Seasbroker.Infrastructure.Persistence.Entities;
using Seasbroker.Modules.Identity.Application.Constants;
using Seasbroker.Modules.Identity.Application.DTOs;
using Seasbroker.Modules.Identity.Application.Mapping;

namespace Seasbroker.Modules.Identity.Application.Services;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(string email, string password, CancellationToken cancellationToken = default);

    Task<AuthResponse> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default);

    Task LogoutAsync(Guid userId, string refreshToken, CancellationToken cancellationToken = default);

    Task<PocketBaseAuthResponse> LoginPocketBaseAsync(string identity, string password, CancellationToken cancellationToken = default);

    Task<PocketBaseAuthResponse> RefreshPocketBaseAsync(Guid userId, CancellationToken cancellationToken = default);
}

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenService _refreshTokenService;

    public AuthService(
        UserManager<User> userManager,
        IJwtTokenService jwtTokenService,
        IRefreshTokenService refreshTokenService)
    {
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
        _refreshTokenService = refreshTokenService;
    }

    public async Task<AuthResponse> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        if (await _userManager.IsLockedOutAsync(user))
        {
            throw new UnauthorizedAccessException("Account is locked. Try again later.");
        }

        if (!await _userManager.CheckPasswordAsync(user, password))
        {
            await _userManager.AccessFailedAsync(user);
            if (await _userManager.IsLockedOutAsync(user))
            {
                throw new UnauthorizedAccessException("Account is locked. Try again later.");
            }

            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        await _userManager.ResetAccessFailedCountAsync(user);

        return await CreateAuthResponseAsync(user, cancellationToken);
    }

    public async Task<AuthResponse> RefreshAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var storedToken = await _refreshTokenService.GetActiveAsync(refreshToken, cancellationToken);
        if (storedToken is null)
        {
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");
        }

        var user = await _userManager.FindByIdAsync(storedToken.UserId.ToString());
        if (user is null)
        {
            throw new UnauthorizedAccessException("User not found.");
        }

        var newRefreshToken = await _refreshTokenService.CreateAsync(user.Id, cancellationToken);
        await _refreshTokenService.RevokeAsync(storedToken, newRefreshToken.Token, cancellationToken);

        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _jwtTokenService.GenerateAccessToken(user, roles);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken.Token,
            ExpiresIn = _jwtTokenService.GetAccessTokenExpirySeconds(),
            User = UserMapper.ToDto(user, roles.ToList()),
        };
    }

    public async Task LogoutAsync(
        Guid userId,
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var storedToken = await _refreshTokenService.GetActiveAsync(refreshToken, cancellationToken);
        if (storedToken is null || storedToken.UserId != userId)
        {
            return;
        }

        await _refreshTokenService.RevokeAsync(storedToken, cancellationToken: cancellationToken);
    }

    public async Task<PocketBaseAuthResponse> LoginPocketBaseAsync(
        string identity,
        string password,
        CancellationToken cancellationToken = default)
    {
        var auth = await LoginAsync(identity, password, cancellationToken);
        var user = await _userManager.FindByEmailAsync(identity)
            ?? throw new UnauthorizedAccessException("Invalid email or password.");

        if (!await _userManager.IsInRoleAsync(user, SeasbrokerIdentityConstants.SuperuserRole))
        {
            throw new UnauthorizedAccessException("Invalid login credentials.");
        }

        return new PocketBaseAuthResponse
        {
            Token = auth.AccessToken,
            Record = UserMapper.ToPocketBaseRecord(user),
        };
    }

    public async Task<PocketBaseAuthResponse> RefreshPocketBaseAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new UnauthorizedAccessException("User not found.");

        if (!await _userManager.IsInRoleAsync(user, SeasbrokerIdentityConstants.SuperuserRole))
        {
            throw new UnauthorizedAccessException("The request requires valid record authorization token.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _jwtTokenService.GenerateAccessToken(user, roles);

        return new PocketBaseAuthResponse
        {
            Token = accessToken,
            Record = UserMapper.ToPocketBaseRecord(user),
        };
    }

    private async Task<AuthResponse> CreateAuthResponseAsync(User user, CancellationToken cancellationToken)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _jwtTokenService.GenerateAccessToken(user, roles);
        var refreshToken = await _refreshTokenService.CreateAsync(user.Id, cancellationToken);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresIn = _jwtTokenService.GetAccessTokenExpirySeconds(),
            User = UserMapper.ToDto(user, roles.ToList()),
        };
    }
}
