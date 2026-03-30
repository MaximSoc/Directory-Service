using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileService.Communication;

public static class FileServiceExtensions
{
    public static IServiceCollection AddFileServiceHttpCommunication(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<FileServiceOptions>(configuration.GetSection(nameof(FileServiceOptions)));

        services.AddHttpClient<IFileCommunicationService, FileHttpClient>((sp, config) =>
        {
            FileServiceOptions fileOptions = sp.GetRequiredService<IOptions<FileServiceOptions>>().Value;

            config.BaseAddress = new Uri(fileOptions.Url);
            config.Timeout = TimeSpan.FromSeconds(fileOptions.TimeoutSeconds);
        });

        return services;
    }
}
