using System.Security.Claims;
using BacklogDotNet.Models;
using BacklogDotNet.Services;
using LoginRequest = Microsoft.AspNetCore.Identity.Data.LoginRequest;

namespace BacklogDotNet.EndPoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth").WithTags("Auth");
        group.MapPost("/login", (LoginRequest request, ITokenService tokenService) =>
        {
            var user = tokenService.Authenticate(request.Email, request.Password);
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