using FileService.Core;
using FileService.Infrastructure.Postgres;
using Microsoft.OpenApi.Models;
using Serilog;

namespace FileService.Web.Configuration
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpLogging(o =>
            {
                o.CombineLogs = true;
            });

            services.AddCore();

            services.AddSerilog();

            services.AddEndpointsApiExplorer();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });
            });

            services.AddInfrastructurePostgres(configuration);

            return services;
        }
    }
}
