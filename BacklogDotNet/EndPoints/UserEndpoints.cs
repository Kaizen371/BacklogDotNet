using System.Security.Claims;
using BacklogDotNet.DTO;
using BacklogDotNet.Models;
using BacklogDotNet.Services;
using Dapper;
using MySqlConnector;


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

        group.MapGet("/me", async (ClaimsPrincipal principal, UserService userService) =>
        {
            string userID = GetUserID(principal);
            

            if (userID == null)
            {
                return (IResult)TypedResults.Unauthorized();
            } 

            var userEntity = await userService.GetUser(userID);
            if (userEntity == null)
            {
                return (IResult)TypedResults.NotFound();
            }
            var userModel = new UserProfile(userEntity.FirstName, userEntity.LastName, userEntity.Username);
            
            return (IResult)TypedResults.Ok(userModel);

        }).RequireAuthorization();
        
    } 
    public static string? GetUserID(ClaimsPrincipal principal)
    {
        string userID = null;
            
        foreach (Claim claim in principal.Claims)
        {
            if (claim.Type == ClaimTypes.NameIdentifier)
            {
                userID = claim.Value;
            }
        }
        return userID;

    }
    
}

