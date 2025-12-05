using Framework.Endpoints;
using Framework.Middlewares;
using Serilog;

namespace FileService.Web
{
    public static class AppExtensions
    {
        public static IApplicationBuilder Configure (this WebApplication app)
        {
            app.UseHttpLogging();

            app.UseExceptionMiddleware();

            app.UseRequestCorrelationId();

            app.UseSerilogRequestLogging();

            app.MapOpenApi();

            app.UseSwagger();
            app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "File Service v1"));

            RouteGroupBuilder apiGroup = app.MapGroup("/api").WithOpenApi();
            app.MapEndpoints(apiGroup);

            return app;
        }
    }
}
