using Core.Database;
using Core.Shared;
using Core.Validation;
using DirectoryService.Application;
using DirectoryService.Application.Database;
using DirectoryService.Application.Departments;
using DirectoryService.Application.Locations;
using DirectoryService.Application.Positions;
using DirectoryService.Infrastructure;
using DirectoryService.Infrastructure.BackgroundServices;
using DirectoryService.Infrastructure.Database;
using DirectoryService.Infrastructure.Interceptors;
using DirectoryService.Infrastructure.Repositories;
using FluentValidation;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.OpenApi.Models;
using Serilog;
using static CSharpFunctionalExtensions.Result;

namespace DirectoryService.Presentation.Configuration
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllers();

            services.AddHttpLogging(o =>
            {
                o.CombineLogs = true;
            });

            services.AddCors();

            services.AddApplication();

            services.AddInfrastructure(configuration);

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });
            });

            services.AddCache(configuration);

            return services;
        }

        private static IServiceCollection AddCache(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddStackExchangeRedisCache(options =>
            {
                string connection = configuration.GetConnectionString("Redis")
                ?? throw new ArgumentNullException(nameof(connection));

                options.Configuration = connection;
            });

            services.AddHybridCache(options =>
            options.DefaultEntryOptions = new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(5),
                LocalCacheExpiration = TimeSpan.FromMinutes(5),
            });

            return services;
        }
    }
}
