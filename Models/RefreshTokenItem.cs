namespace AspNetJWTAuth.Models;

public record RefreshTokenItem(string Token, int UserId, DateTime ExpiresAt);