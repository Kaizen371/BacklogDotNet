using System.Security.Claims;
using BacklogDotNet.DTO;
using BacklogDotNet.Models;
using BacklogDotNet.Services;
using Dapper;
using MySqlConnector;
using LoginRequest = Microsoft.AspNetCore.Identity.Data.LoginRequest;

namespace BacklogDotNet.EndPoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth").WithTags("Auth");
        
        group.MapPost("/login", async (LoginRequest request, TokenService tokenService, UserService userService) =>
        {
            var user = await tokenService.Authenticate(request.Email, request.Password, userService);
            if (user == null)
            {
                return (IResult)TypedResults.Unauthorized();
            }
            return (IResult)TypedResults.Ok(new AuthToken(tokenService.GenerateToken(user)));
        }).AllowAnonymous();

        group.MapGet("/me", (ClaimsPrincipal user) =>
        {
            return new UserProfile("Kai", "campbell", "kaizen");
        }).RequireAuthorization();
        
    } 
    
    
}