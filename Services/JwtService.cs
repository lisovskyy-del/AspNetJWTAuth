using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AspNetJWTAuth.DTOS;
using AspNetJWTAuth.Models;
using Microsoft.IdentityModel.Tokens;

namespace AspNetJWTAuth.Services;

public class JwtService(IConfiguration configuration)
{
    private readonly List<RefreshTokenItem> _refreshTokens = [];

    public TokenPairResponse GenerateTokenPair(int userId)
    {
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
        var issuer = jwtSettings["Issuer"] ?? "AspNetJWTAuth";
        var audience = jwtSettings["Audience"] ?? "AspNetJWTAuth.Users";
        var expirationMinutes = jwtSettings.GetValue<int>("AccessTokenExpirationMinutes", 2);
        var refreshDays = jwtSettings.GetValue<int>("RefreshTokenExpirationDays", 7);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
        var refreshToken = Guid.NewGuid().ToString("N");

        _refreshTokens.Add(new RefreshTokenItem(
            refreshToken,
            userId,
            DateTime.UtcNow.AddDays(refreshDays)
        ));

        return new TokenPairResponse(accessToken, refreshToken);
    }

    public TokenPairResponse? RotateRefreshToken(string oldRefreshToken)
    {
        var stored = _refreshTokens.FirstOrDefault(t => t.Token == oldRefreshToken);

        if (stored is null || stored.ExpiresAt < DateTime.UtcNow)
            return null;

        _refreshTokens.Remove(stored);
        return GenerateTokenPair(stored.UserId);
    }
}