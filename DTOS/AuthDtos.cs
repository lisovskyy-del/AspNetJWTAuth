namespace AspNetJWTAuth.DTOS;

public record LoginRequest(string Username, string Password);

public record RefreshRequest(string RefreshToken);

public record TokenPairResponse(string AccessToken, string RefreshToken);