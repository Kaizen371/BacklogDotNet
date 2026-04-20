using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BacklogDotNet.DTO;
using Microsoft.IdentityModel.Tokens;

namespace BacklogDotNet.Services;

public class TokenService(IConfiguration config)
{
    public string GenerateToken(UserEntity user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            config["Jwt:Issuer"], config["Jwt:Audience"], claims,
            expires: DateTime.UtcNow.AddHours(1), signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<UserEntity?> Authenticate(string email, string password, UserService userService)
    {
        var user = await userService.GetByEmail(email);
        if (user != null && user.Password == password) return user;

        return null;
    }
}