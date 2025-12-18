using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using StoryReader.Application.Interfaces;
using StoryReader.Infrastructure.Jwt;
using StoryReader.Infrastructure.Password;
using StoryReader.Infrastructure.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryReader.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Redis
            services.AddSingleton<IConnectionMultiplexer>(
                _ => ConnectionMultiplexer.Connect(
                    configuration.GetConnectionString("Redis")!
                )
            );


            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = configuration.GetConnectionString("Redis") ?? "localhost:6379";
            });

            services.AddSingleton<IAuthTokenStore, RedisAuthTokenStore>();
            services.AddScoped<IRedisStoryCache, RedisStoryCache>();
            // JWT / Password / External services khác
            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();



            return services;
        }
    }
}
