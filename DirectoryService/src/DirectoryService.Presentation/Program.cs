using DirectoryService.Application;
using DirectoryService.Application.Database;
using DirectoryService.Application.Departments;
using DirectoryService.Application.Locations;
using DirectoryService.Application.Positions;
using DirectoryService.Application.Shared;
using DirectoryService.Application.Validation;
using DirectoryService.Infrastructure;
using DirectoryService.Infrastructure.Repositories;
using DirectoryService.Presentation.Middlewares;
using FluentValidation;
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
builder.Services.AddOpenApi(options =>
{
    options.AddSchemaTransformer((schema, context, _) =>
    {
        if (context.JsonTypeInfo.Type == typeof(Envelope<Errors>))
        {
            if (schema.Properties.TryGetValue("errors", out var errorsProp))
            {
                errorsProp.Items.Reference = new OpenApiReference
                {
                    Type = ReferenceType.Schema,
                    Id = "Error"
                };
            }
        }

        return Task.CompletedTask;
    });
});

builder.Services.AddHttpLogging(o =>
{
    o.CombineLogs = true;
});

builder.Services.AddSerilog();

builder.Services.AddScoped<DirectoryServiceDbContext>(_ => 
new DirectoryServiceDbContext(builder.Configuration.GetConnectionString("DirectoryServiceDb")!));

builder.Services.AddScoped<ILocationsRepository, LocationsRepository>();

builder.Services.AddScoped<CreateLocationHandler>();

builder.Services.AddScoped<IDepartmentsRepository, DepartmentRepository>();

builder.Services.AddScoped<CreateDepartmentHandler>();

builder.Services.AddScoped<UpdateDepartmentLocationsHandler>();

builder.Services.AddScoped<MoveDepartmentHandler>();

builder.Services.AddScoped<IPositionsRepository, PositionRepository>();

builder.Services.AddScoped<CreatePositionHandler>();

builder.Services.AddScoped<ITransactionManager, TransactionManager>();

builder.Services.AddValidatorsFromAssembly(typeof(CustomValidators).Assembly);

var app = builder.Build();

app.UseExceptionMiddleware();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "v1"));
}

app.UseHttpLogging();

app.UseSerilogRequestLogging();

app.MapControllers();

app.Run();

namespace DirectoryService.Presentation
{
    public partial class Program;
}
