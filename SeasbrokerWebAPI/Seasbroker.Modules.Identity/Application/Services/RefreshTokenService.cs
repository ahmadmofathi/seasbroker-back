using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Seasbroker.Infrastructure.Persistence;
using Seasbroker.Infrastructure.Persistence.Entities;
using Seasbroker.Modules.Identity.Infrastructure.Options;

namespace Seasbroker.Modules.Identity.Application.Services;

public interface IRefreshTokenService
{
    Task<RefreshToken> CreateAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<RefreshToken?> GetActiveAsync(string token, CancellationToken cancellationToken = default);

    Task RevokeAsync(RefreshToken token, string? replacedByToken = null, CancellationToken cancellationToken = default);

    Task RevokeAllForUserAsync(Guid userId, CancellationToken cancellationToken = default);
}

public class RefreshTokenService : IRefreshTokenService
{
    private readonly SeasbrokerDbContext _dbContext;
    private readonly JwtOptions _options;

    public RefreshTokenService(SeasbrokerDbContext dbContext, IOptions<JwtOptions> options)
    {
        _dbContext = dbContext;
        _options = options.Value;
    }

    public async Task<RefreshToken> CreateAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var refreshToken = new RefreshToken
        {
            UserId = userId,
            Token = GenerateToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(_options.RefreshTokenExpiryDays),
            CreatedAt = DateTime.UtcNow,
        };

        _dbContext.RefreshTokens.Add(refreshToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return refreshToken;
    }

    public async Task<RefreshToken?> GetActiveAsync(string token, CancellationToken cancellationToken = default)
    {
        var refreshToken = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == token, cancellationToken);

        if (refreshToken is null || !refreshToken.IsActive)
        {
            return null;
        }

        return refreshToken;
    }

    public async Task RevokeAsync(
        RefreshToken token,
        string? replacedByToken = null,
        CancellationToken cancellationToken = default)
    {
        token.RevokedAt = DateTime.UtcNow;
        token.ReplacedByToken = replacedByToken;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RevokeAllForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var tokens = await _dbContext.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var token in tokens)
        {
            token.RevokedAt = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static string GenerateToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }
}
