﻿using DirectoryService.Infrastructure;
using DirectoryService.Presentation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Npgsql;
using Respawn;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Testcontainers.PostgreSql;
using Program = DirectoryService.Presentation.Program;

namespace DirectoryService.IntegrationTests.Infrastructure
{
    public class DirectoryTestWebFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
            .WithImage("postgres")
            .WithDatabase("directory_service_db")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        private Respawner _respawner = null!;
        private DbConnection _dbConnection = null!;
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<DirectoryServiceDbContext>();

                services.AddScoped(_ =>
                    new DirectoryServiceDbContext(_dbContainer.GetConnectionString()));
            });
        }

        public async Task InitializeAsync()
        {
            await _dbContainer.StartAsync();

            using var scope = Services.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<DirectoryServiceDbContext>();

            await dbContext.Database.EnsureDeletedAsync();
            await dbContext.Database.EnsureCreatedAsync();

            _dbConnection = new NpgsqlConnection(_dbContainer.GetConnectionString());
            await _dbConnection.OpenAsync();

            await InitializeRespawner();
        }

        public new async Task DisposeAsync()
        {
            await _dbContainer.StopAsync();
            await _dbContainer.DisposeAsync();

            await _dbConnection.CloseAsync();
            await _dbConnection.DisposeAsync();
        }

        public async Task ResetDatabaseAsync()
        {
            await _respawner.ResetAsync(_dbConnection);
        }

        private async Task InitializeRespawner()
        {
            _respawner = await Respawner.CreateAsync(
                _dbConnection,
                new RespawnerOptions
                {
                    DbAdapter = DbAdapter.Postgres,
                    SchemasToInclude = ["public"]
                });
        }
    }
}
