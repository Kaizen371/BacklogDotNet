using System.Security.Claims;
using System.Text;
using BacklogDotNet.DTO;
using BacklogDotNet.EndPoints;
using BacklogDotNet.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.IdentityModel.Tokens;
using MySqlConnector;
using Dapper;



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
builder.Services.AddMySqlDataSource(builder.Configuration.GetConnectionString("Default"));

builder.Services.AddAuthorization();

builder.Services.AddScoped<UserService>();

builder.Services.AddSingleton<TokenService>();

builder.Services.AddCors(options =>
{
        options.AddPolicy("AllowRider",
                policy =>
                {
                        policy.WithOrigins("http://localhost:63342") // Your HTML's origin
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                });
});



var app = builder.Build();

app.UseCors("AllowRider");
app.UseAuthentication();
app.UseAuthorization();
app.MapUserEndpoints();

app.Run();
