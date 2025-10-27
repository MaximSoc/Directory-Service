using DirectoryService.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryService.IntegrationTests.Infrastructure
{
    public abstract class DirectoryTestsBase : IClassFixture<DirectoryTestWebFactory>, IAsyncLifetime
    {
        private readonly Func<Task> _resetDatabase;
        protected IServiceProvider Services { get; set; }
        protected DirectoryTestsBase(DirectoryTestWebFactory factory)
        {
            Services = factory.Services;
            _resetDatabase = factory.ResetDatabaseAsync;
        }

        protected async Task<T> ExecuteInDb<T>(Func<DirectoryServiceDbContext, Task<T>> action)
        {
            await using var scope = Services.CreateAsyncScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<DirectoryServiceDbContext>();

            return await action(dbContext);
        }

        protected async Task ExecuteInDb(Func<DirectoryServiceDbContext, Task> action)
        {
            await using var scope = Services.CreateAsyncScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<DirectoryServiceDbContext>();

            await action(dbContext);
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        public async Task InitializeAsync()
        {
            await _resetDatabase();
        }
    }
}
