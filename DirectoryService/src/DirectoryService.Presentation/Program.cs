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

Serilog.Debugging.SelfLog.Enable(msg => Console.WriteLine($"SERILOG ERROR: {msg}"));

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting web application");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.Seq(context.Configuration.GetConnectionString("Seq")
        ?? throw new ArgumentNullException("Seq")));

    builder.Services.AddConfiguration(builder.Configuration);

    var app = builder.Build();

    app.UseSerilogRequestLogging();

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
