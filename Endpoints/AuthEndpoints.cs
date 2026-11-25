using AspNetJWTAuth.DTOS;
using AspNetJWTAuth.Services;

namespace AspNetJWTAuth.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("")
            .WithTags("Auth");

        group.MapPost("/login", Login);
        group.MapPost("/refresh", Refresh);
    }

    private static IResult Login(LoginRequest request, UserService userService, JwtService jwtService)
    {
        var user = userService.Authenticate(request.Username, request.Password);
        if (user is null)
            return Results.Unauthorized();

        var tokens = jwtService.GenerateTokenPair(user.Id);
        return Results.Ok(tokens);
    }

    private static IResult Refresh(RefreshRequest request, JwtService jwtService)
    {
        var newTokens = jwtService.RotateRefreshToken(request.RefreshToken);
        if (newTokens is null)
            return Results.Unauthorized();

        return Results.Ok(newTokens);
    }
}