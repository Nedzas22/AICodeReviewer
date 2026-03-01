using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using CodeLens.Application.Common.Interfaces;
using CodeLens.Infrastructure.Settings;

namespace CodeLens.Infrastructure.Services;

/// <summary>
/// Generates and validates signed JWT access tokens using HMAC-SHA256.
/// </summary>
internal sealed class JwtService : IJwtService
{
    private readonly JwtSettings _settings;

    /// <summary>Initialises the service with JWT configuration.</summary>
    public JwtService(IOptions<JwtSettings> options) => _settings = options.Value;

    /// <inheritdoc />
    public (string Token, DateTime ExpiresAt) GenerateToken(string userId, string email, string displayName)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiresAt = DateTime.UtcNow.AddHours(_settings.ExpirationHours);

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                new Claim(JwtRegisteredClaimNames.Sub, userId),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim("display_name", displayName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat,
                    DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                    ClaimValueTypes.Integer64)
            ]),
            Issuer = _settings.Issuer,
            Audience = _settings.Audience,
            Expires = expiresAt,
            SigningCredentials = credentials
        };

        var handler = new JwtSecurityTokenHandler();
        return (handler.WriteToken(handler.CreateToken(descriptor)), expiresAt);
    }

    /// <inheritdoc />
    public (bool IsValid, string? UserId) ValidateToken(string token)
    {
        try
        {
            var key = Encoding.UTF8.GetBytes(_settings.SecretKey);
            var handler = new JwtSecurityTokenHandler();

            handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _settings.Issuer,
                ValidateAudience = true,
                ValidAudience = _settings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out var validatedToken);

            var jwt = (JwtSecurityToken)validatedToken;
            var userId = jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value;

            return (true, userId);
        }
        catch
        {
            return (false, null);
        }
    }
}
