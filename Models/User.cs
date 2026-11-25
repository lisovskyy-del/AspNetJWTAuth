namespace AspNetJWTAuth.Models;

public record User(int Id, string Username, string PasswordHash);