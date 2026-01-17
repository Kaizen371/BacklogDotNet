using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BacklogDotNet.Models;
using Microsoft.IdentityModel.Tokens;

namespace BacklogDotNet.Services;

public interface ITokenService {
    string GenerateToken(User user);
    User? Authenticate(string email, string password);
}

public class TokenService(IConfiguration config) : ITokenService {
    public string GenerateToken(User user) {
        var claims = new[] {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        };
        
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        var token = new JwtSecurityToken(
            config["Jwt:Issuer"], config["Jwt:Audience"], claims,
            expires: DateTime.UtcNow.AddHours(1), signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public User? Authenticate(string email, string password) => 
        (email == "admin" && password == "password") 
            ? new User(Guid.NewGuid()) : null;
}