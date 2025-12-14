using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StoryReader.Api.Middlewares;
using StoryReader.Application.Interfaces;
using StoryReader.Application.Services;
using StoryReader.Infrastructure.Jwt;
using StoryReader.Infrastructure.Password;
using StoryReader.Persistence.Context;
using StoryReader.Persistence.Repositories;
using System.Text;

namespace StoryReader.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // DI - Repositories
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

            // DI - Infrastructure Services
            builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
            builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

            // DI - Application Services
            builder.Services.AddScoped<IAuthService, AuthService>();

            // Bind JwtOptions + DI
            builder.Services.Configure<JwtOptions>( builder.Configuration.GetSection("Jwt"));

            //Add Authentication + JwtBearer
            var jwtOptions = builder.Configuration
            .GetSection("Jwt")
            .Get<JwtOptions>()!;

            builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,

                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtOptions.Secret)
                    ),

                    ClockSkew = TimeSpan.Zero // không cho lệch giờ
                };
            });

            builder.Services.AddAuthorization();

            // connect db
            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql(
                    builder.Configuration.GetConnectionString("Postgres")
                );
            });



            var app = builder.Build();
       

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseMiddleware<ExceptionMiddleware>();
            app.UseMiddleware<RequestLoggingMiddleware>();

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
