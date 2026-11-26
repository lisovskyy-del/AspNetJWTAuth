using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AspNetJWTAuth.DTOS;
using AspNetJWTAuth.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AspNetJWTAuth.Services;

public class JwtService(IOptions<JwtSettings> options)
{
    private readonly JwtSettings _jwtSettings = options.Value;
    private readonly List<RefreshTokenItem> _refreshTokens = [];

    public TokenPairResponse GenerateTokenPair(int userId)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            signingCredentials: credentials
        );

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
        var refreshToken = Guid.NewGuid().ToString("N");

        _refreshTokens.Add(new RefreshTokenItem(
            refreshToken,
            userId,
            DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays)
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