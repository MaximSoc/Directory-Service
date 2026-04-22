using Framework.Endpoints;
using Framework.Middlewares;
using Serilog;

namespace FileService.Web.Configuration;

public static class AppExtensions
{
    public static IApplicationBuilder ConfigureApp (this WebApplication app)
    {
        app.UseCors(builder =>
        {
            builder.WithOrigins(
                "http://localhost:3000",
                "http://localhost",
                "http://frontend:3000")
            .AllowCredentials()
            .AllowAnyHeader()
            .AllowAnyMethod();
        });

        app.UseHttpLogging();

        app.UseExceptionMiddleware();

        app.UseRequestCorrelationId();

        app.UseSerilogRequestLogging();

        app.MapOpenApi();

        app.UseSwagger();
        app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "File Service v1"));

        app.MapEndpoints();

        return app;
    }
}
