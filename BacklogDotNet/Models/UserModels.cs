namespace BacklogDotNet.Models;

 public record LoginRequest(string Email, string Password);
 
 public record UserProfile(string FirstName, string LastName, string Username);
 
 public record AuthToken(string AccessToken);
 
 public record User(Guid Id);