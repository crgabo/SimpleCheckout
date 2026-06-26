namespace SimpleCheckout.Application;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

public record LoginRequest(string Username, string Password);
public record LoginResponse(string Token);

public class TokenService(IConfiguration configuration)
{
    private const string FakeUsername = "admin";
    private const string FakePassword = "admin";

    public LoginResponse? Login(LoginRequest request)
    {
        if (request.Username != FakeUsername || request.Password != FakePassword)
            return null;

        var key     = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]!));
        var creds   = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token   = new JwtSecurityToken(
            issuer:            configuration["Jwt:Issuer"],
            audience:          configuration["Jwt:Audience"],
            claims:            [new Claim(ClaimTypes.Name, request.Username)],
            expires:           DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);

        return new LoginResponse(new JwtSecurityTokenHandler().WriteToken(token));
    }
}
