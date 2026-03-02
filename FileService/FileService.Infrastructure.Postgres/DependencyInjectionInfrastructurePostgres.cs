using FileService.Core.MediaAssets;
using FileService.Infrastructure.Postgres.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileService.Infrastructure.Postgres
{
    public static class DependencyInjectionInfrastructurePostgres
    {
        public static IServiceCollection AddInfrastructurePostgres(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("FileServiceDb");

            services.AddDbContext<FileServiceDbContext>(options =>
            {
                options.UseNpgsql(connectionString);
                options.EnableDetailedErrors();
                options.EnableSensitiveDataLogging();
            });

            services.AddScoped<IMediaRepository, MediaRepository>();

            return services;
        }
    }
}
