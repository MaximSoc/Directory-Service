using DirectoryService.Application;
using DirectoryService.Application.Database;
using DirectoryService.Application.Departments;
using DirectoryService.Application.Locations;
using DirectoryService.Application.Positions;
using DirectoryService.Application.Shared;
using DirectoryService.Application.Validation;
using DirectoryService.Infrastructure;
using DirectoryService.Infrastructure.BackgroundServices;
using DirectoryService.Infrastructure.Database;
using DirectoryService.Infrastructure.Interceptors;
using DirectoryService.Infrastructure.Repositories;
using DirectoryService.Presentation.Middlewares;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using Shared;

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

builder.Services.AddControllers();

builder.Services.AddHttpLogging(o =>
{
    o.CombineLogs = true;
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddSerilog();

builder.Services.AddScoped<DirectoryServiceDbContext>(_ => 
new DirectoryServiceDbContext(builder.Configuration.GetConnectionString("DirectoryServiceDb")!));

builder.Services.AddScoped<IReadDbContext, DirectoryServiceDbContext>(_ =>
new DirectoryServiceDbContext(builder.Configuration.GetConnectionString("DirectoryServiceDb")!));

builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();

builder.Services.AddScoped<ILocationsRepository, LocationsRepository>();

builder.Services.AddScoped<CreateLocationHandler>();

builder.Services.AddScoped<GetLocationsByDepartmentHandler>();

builder.Services.AddScoped<IDepartmentsRepository, DepartmentRepository>();

builder.Services.AddScoped<CreateDepartmentHandler>();

builder.Services.AddScoped<UpdateDepartmentLocationsHandler>();

builder.Services.AddScoped<MoveDepartmentHandler>();

builder.Services.AddScoped<GetDepartmentsWithTopPositionsHandler>();

builder.Services.AddScoped<GetParentWithChildrensHandler>();

builder.Services.AddScoped<GetChildrenByParentHandler>();

builder.Services.AddScoped<DeleteDepartmentHandler>();

builder.Services.AddScoped<DeleteInactiveDepartmentsHandler>();

builder.Services.AddScoped<IPositionsRepository, PositionRepository>();

builder.Services.AddScoped<CreatePositionHandler>();

builder.Services.AddScoped<ITransactionManager, TransactionManager>();

builder.Services.AddSingleton<SoftDeleteInterceptor>();

builder.Services.AddValidatorsFromAssembly(typeof(CustomValidators).Assembly);

builder.Services.AddHostedService<DepartmentCleanerBackgroundService>();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });
    //c.AddServer(new OpenApiServer
    //{
    //    Url = "http://localhost:8080"
    //});
});

var app = builder.Build();

await using var scope = app.Services.CreateAsyncScope();

var dbContext = scope.ServiceProvider.GetRequiredService<DirectoryServiceDbContext>();

await dbContext.Database.MigrateAsync();

app.UseExceptionMiddleware();

app.UseRouting();

app.UseCors();

app.UseHttpLogging();

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1"));
}

app.MapControllers();

app.Run();

namespace DirectoryService.Presentation
{
    public partial class Program;
}
