using DirectoryService.Application;
using DirectoryService.Application.Database;
using DirectoryService.Infrastructure;
using DirectoryService.Infrastructure.Repositories;
using Microsoft.OpenApi.Models;
using Shared;

var builder = WebApplication.CreateBuilder(args);


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

builder.Services.AddScoped<DirectoryServiceDbContext>(_ => 
new DirectoryServiceDbContext(builder.Configuration.GetConnectionString("DirectoryServiceDb")!));

builder.Services.AddScoped<ILocationsRepository, LocationsRepository>();

builder.Services.AddScoped<CreateLocationHandler>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "v1"));
}

app.MapControllers();

app.Run();
