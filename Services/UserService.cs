using AspNetJWTAuth.Models;

namespace AspNetJWTAuth.Services;

public class UserService
{
    private readonly List<User> _users =
    [
        new(1, "admin", BCrypt.Net.BCrypt.HashPassword("admin123")),
        new(2, "user", BCrypt.Net.BCrypt.HashPassword("user123"))
    ];

    public User? Authenticate(string username, string password)
    {
        var user = _users.FirstOrDefault(u => u.Username == username);
        if (user is null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return null;

        return user;
    }
}