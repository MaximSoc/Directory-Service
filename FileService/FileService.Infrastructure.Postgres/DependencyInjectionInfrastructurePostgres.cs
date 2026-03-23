using Core.Shared;
using FileService.Core;
using FileService.Core.MediaAssets;
using FileService.Infrastructure.Postgres.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace FileService.Infrastructure.Postgres;

public static class DependencyInjectionInfrastructurePostgres
{
    public static IServiceCollection AddInfrastructurePostgres(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContextPool<FileServiceDbContext>((sp, options) =>
        {
            var connectionString = configuration.GetConnectionString("FileServiceDb");
            IHostEnvironment hostEnvironment = sp.GetRequiredService<IHostEnvironment>();
            ILoggerFactory loggerFactory = sp.GetRequiredService<ILoggerFactory>();

            options.UseNpgsql(connectionString);

            if (hostEnvironment.IsDevelopment())
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }

            options.UseLoggerFactory(loggerFactory);

        });

        services.AddDbContextPool<IReadDbContext, FileServiceDbContext>((sp, options) =>
        {
            var connectionString = configuration.GetConnectionString("FileServiceDb");
            IHostEnvironment hostEnvironment = sp.GetRequiredService<IHostEnvironment>();
            ILoggerFactory loggerFactory = sp.GetRequiredService<ILoggerFactory>();

            options.UseNpgsql(connectionString);

            if (hostEnvironment.IsDevelopment())
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }

            options.UseLoggerFactory(loggerFactory);

        });

        services.AddScoped<IReadDbContext>(sp => sp.GetRequiredService<FileServiceDbContext>());

        services.AddScoped<IMediaRepository, MediaRepository>();

        services.AddScoped<ITransactionManager, TransactionManager>();

        return services;
    }
}
