using Core.Database;
using Dapper;
using DirectoryService.Application.Database;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryService.Infrastructure.Database
{
    public class DbConnectionFactory : IDbConnectionFactory, IDisposable, IAsyncDisposable
    {
        private readonly NpgsqlDataSource _dataSource;
        private readonly ILogger<DbConnectionFactory> _logger;

        public DbConnectionFactory(IConfiguration configuration, ILogger<DbConnectionFactory> logger)
        {
            var dataSourceBuilder = new NpgsqlDataSourceBuilder(configuration.GetConnectionString("DirectoryServiceDb"));
            dataSourceBuilder.UseLoggerFactory(CreateLoggerFactory());
            _dataSource = dataSourceBuilder.Build();
            _logger = logger;
        }

        public async Task<IDbConnection> CreateConnectionAsync(CancellationToken cancellationToken)
        {
            try
            {
                var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
                await connection.ExecuteAsync(@"SET search_path TO public;");

                return connection;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not connect to database");
                throw;
            }
        }

        private static ILoggerFactory CreateLoggerFactory()
        => LoggerFactory.Create(builder => builder.AddConsole());

        public void Dispose() => _dataSource?.Dispose();

        public async ValueTask DisposeAsync() => await _dataSource.DisposeAsync();
    }
}