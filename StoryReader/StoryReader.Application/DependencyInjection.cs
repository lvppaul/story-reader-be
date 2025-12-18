using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StoryReader.Application.Interfaces;
using StoryReader.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryReader.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
          
            // DI
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IStoryService, StoryService>();
          


            return services;
        }
    }
}
