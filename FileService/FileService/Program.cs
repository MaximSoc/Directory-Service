using Core.Database;
using Core.Shared;
using Core.Validation;
using FileService.Web.Configuration;
using FluentValidation;
using Framework.Middlewares;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Transactions;
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

namespace FileService.Web
{
    public partial class Program;
}
