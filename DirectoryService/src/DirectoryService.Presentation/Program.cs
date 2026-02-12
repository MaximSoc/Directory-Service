using Core.Database;
using Core.Shared;
using Core.Validation;
using DirectoryService.Application;
using DirectoryService.Application.Database;
using DirectoryService.Application.Departments;
using DirectoryService.Application.Locations;
using DirectoryService.Application.Positions;
using DirectoryService.Infrastructure;
using DirectoryService.Infrastructure.BackgroundServices;
using DirectoryService.Infrastructure.Database;
using DirectoryService.Infrastructure.Interceptors;
using DirectoryService.Infrastructure.Repositories;
using DirectoryService.Presentation.Configuration;
using FluentValidation;
using Framework.Middlewares;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.OpenApi.Models;
using Serilog;
using static CSharpFunctionalExtensions.Result;

try
{
    var builder = WebApplication.CreateBuilder(args);

    Log.Logger = new LoggerConfiguration()
        .WriteTo.Console()
        .WriteTo.Debug()
        .WriteTo.Seq(builder.Configuration.GetConnectionString("Seq")
        ?? throw new ArgumentNullException("Seq"))
        .MinimumLevel.Override("Microsoft.AspNetCore.Hosting", Serilog.Events.LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.AspNetCore.Mvc", Serilog.Events.LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.AspNetCore.Routing", Serilog.Events.LogEventLevel.Warning)
        .CreateLogger();

    builder.Services.AddConfiguration(builder.Configuration);

    var app = builder.Build();

    await using var scope = app.Services.CreateAsyncScope();

    var dbContext = scope.ServiceProvider.GetRequiredService<DirectoryServiceDbContext>();

    app.ConfigureApp();

    app.Run();
}
catch(Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}

finally
{
    Log.CloseAndFlush();
}

namespace DirectoryService.Presentation
{
    public partial class Program;
}
