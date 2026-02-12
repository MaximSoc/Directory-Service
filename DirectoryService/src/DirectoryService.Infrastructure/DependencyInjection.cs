using Core.Database;
using Core.Shared;
using DirectoryService.Application.Database;
using DirectoryService.Infrastructure.BackgroundServices;
using DirectoryService.Infrastructure.Database;
using DirectoryService.Infrastructure.Interceptors;
using DirectoryService.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryService.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<DirectoryServiceDbContext>(_ =>
            new DirectoryServiceDbContext(configuration.GetConnectionString("DirectoryServiceDb")!));

            services.AddScoped<IReadDbContext, DirectoryServiceDbContext>(_ =>
            new DirectoryServiceDbContext(configuration.GetConnectionString("DirectoryServiceDb")!));

            services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();

            services.AddScoped<ILocationsRepository, LocationsRepository>();

            services.AddScoped<IDepartmentsRepository, DepartmentRepository>();

            services.AddScoped<IPositionsRepository, PositionRepository>();

            services.AddScoped<ITransactionManager, TransactionManager>();

            services.AddSingleton<SoftDeleteInterceptor>();

            services.AddHostedService<DepartmentCleanerBackgroundService>();

            return services;
        }
    }
}
