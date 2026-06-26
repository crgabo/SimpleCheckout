namespace SimpleCheckout.API;

using SimpleCheckout.Application;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        app.MapPost("/auth/login", (LoginRequest request, TokenService service) =>
        {
            var result = service.Login(request);
            return result is null ? Results.Unauthorized() : Results.Ok(result);
        })
        .WithName("Login")
        .WithOpenApi()
        .AllowAnonymous()
        .Produces<LoginResponse>()
        .Produces(StatusCodes.Status401Unauthorized);
    }
}
