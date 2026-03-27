using FileService.Infrastructure.Postgres;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileService.IntegrationTests.Infrastructure;

public class FileServiceTestBase : IClassFixture<IntegrationTestsWebFactory>
{
    public const string TEST_FILE_NAME = "test-file.mkv";

    protected HttpClient AppHttpClient { get; init; }
    protected HttpClient HttpClient { get; init; }
    protected IServiceProvider Services { get; init; }

    protected FileServiceTestBase(IntegrationTestsWebFactory factory)
    {
        AppHttpClient = factory.CreateClient();
        HttpClient = new HttpClient();
        Services = factory.Services;
    }

    protected async Task ExecuteInDb(Func<FileServiceDbContext, Task> action)
    {
        await using var scope = Services.CreateAsyncScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<FileServiceDbContext>();

        await action(dbContext);
    }
}
