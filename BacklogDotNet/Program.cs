using System.Security.Claims;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDataProtection();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AuthService>();

var app = builder.Build();

app.Use((ctx, next) =>
{
    var idp = ctx.RequestServices.GetService<IDataProtectionProvider>();
    var proctector = idp.CreateProtector("auth-cookie");
    
    var authCookie = ctx.Request.Headers.Cookie.FirstOrDefault(x => x.StartsWith("auth="));
    var ProtectedPayload = authCookie.Split("=").Last();
    var payload = proctector.Unprotect(ProtectedPayload);
    var parts = payload.Split(":");
    var key = parts[0];
    var value = parts[1];
    
    
    var claims = new List<Claim>();
    claims.Add(new Claim(key, value));
    var identity = new ClaimsIdentity(claims);
    ctx.User = new ClaimsPrincipal(identity);
    
    return next();
});

app.MapGet("/username", (HttpContext ctx) =>
{
    return ctx.User.FindFirst("usr").Value;
});
app.MapGet("/login", (AuthService auth) =>
{
    auth.SignIn();
    return "login complete";
});

app.Run();

public class AuthService
{
    private readonly IDataProtectionProvider _idp;
    private readonly IHttpContextAccessor _accessor;

    public AuthService(IDataProtectionProvider idp, IHttpContextAccessor accessor)
    {
        _idp = idp;
        _accessor = accessor;
    }

    public void SignIn()
    {
        var proctector = _idp.CreateProtector("auth-cookie");
        _accessor.HttpContext.Response.Headers["set-cookie"] = $"auth={proctector.Protect("usr:kai")}";
    }
}