using System.Security.Claims;
using System.Text;
using BacklogDotNet.EndPoints;
using BacklogDotNet.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
                };
        });


builder.Services.AddAuthorization();

builder.Services.AddSingleton<ITokenService, TokenService>();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.MapUserEndpoints();

app.Run();
