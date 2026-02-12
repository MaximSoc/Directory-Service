using DirectoryService.Infrastructure;
using Framework.Middlewares;
using Serilog;

namespace DirectoryService.Presentation.Configuration
{
    public static class AppExtensions
    {
        public static IApplicationBuilder ConfigureApp(this WebApplication app)
        { 
            app.UseExceptionMiddleware();

            app.UseRouting();

            app.UseCors(builder =>
            {
                builder.WithOrigins("http://localhost:3000")
                .AllowCredentials()
                .AllowAnyHeader()
                .AllowAnyMethod();
            });

            app.UseHttpLogging();

            app.UseSerilogRequestLogging();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1"));
            }

            app.MapControllers();

            return app;
        }
    }
}
