using DirectoryService.Application.Departments;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryService.Infrastructure.BackgroundServices
{
    public class DepartmentCleanerBackgroundService : BackgroundService
    {
        private readonly ILogger<DepartmentCleanerBackgroundService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _configuration;
        public DepartmentCleanerBackgroundService(
            ILogger<DepartmentCleanerBackgroundService> logger,
            IServiceScopeFactory scopeFactory,
            IConfiguration configuration)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _configuration = configuration;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("DepartmentCleanerBackgroundService is starting");

            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope();

                var handler = scope.ServiceProvider.GetRequiredService<DeleteInactiveDepartmentsHandler>();

                try
                {
                    await handler.Handle(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while deleting inactive departments");
                }

                var interval = GetIntervalFromConfig();

                _logger.LogInformation($"DepartmentCleanerBackgroundService waiting {interval} before next run");

                await Task.Delay(interval, stoppingToken);
            }
        }

        private TimeSpan GetIntervalFromConfig()
        {
            var hours = _configuration.GetValue<int>("DepartmentCleaner:RunIntervalHours");
            return TimeSpan.FromHours(hours);
        }
    }
}
