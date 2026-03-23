using Amazon.S3;
using FileService.Core;
using FileService.Core.FilesStorage;
using FileService.Infrastructure.Postgres;
using FileService.Infrastructure.S3;
using FileService.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Testcontainers.Minio;
using Testcontainers.PostgreSql;
using static CSharpFunctionalExtensions.Result;

namespace FileService.IntegrationTests.Infrastructure;

public class IntegrationTestsWebFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres")
        .WithDatabase("file_service_db_tests")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private readonly MinioContainer _minioContainer = new MinioBuilder()
        .WithImage("minio/minio")
        .WithUsername("minio")
        .WithPassword("minio")
        .Build();

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        await _minioContainer.StartAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddJsonFile(Path.Combine(AppContext.BaseDirectory, "appsettings.Tests.json"), optional: true);
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<FileServiceDbContext>();
            services.RemoveAll<IReadDbContext>();

            services.AddDbContextPool<FileServiceDbContext>((sp, options) =>
            {
                options.UseNpgsql(_dbContainer.GetConnectionString());
            });

            services.AddDbContextPool<IReadDbContext, FileServiceDbContext>((sp, options) =>
            {
                options.UseNpgsql(_dbContainer.GetConnectionString());
            });

            services.RemoveAll<IAmazonS3>();

            services.AddSingleton<IAmazonS3>(sp =>
            {
                S3Options s3Options = sp.GetRequiredService<IOptions<S3Options>>().Value;

                var minioPort = _minioContainer.GetMappedPublicPort(9000);

                var config = new AmazonS3Config
                {
                    ServiceURL = $"http://{_minioContainer.Hostname}:{minioPort}",
                    UseHttp = !s3Options.WithSsl,
                    ForcePathStyle = true,
                };

                return new AmazonS3Client(s3Options.AccessKey, s3Options.SecretKey, config);
            });
        });
    }

    public new async Task DisposeAsync()
    {
        await _minioContainer.StopAsync();
        await _minioContainer.DisposeAsync();

        await _dbContainer.StopAsync();
        await _dbContainer.DisposeAsync();
    }
}
