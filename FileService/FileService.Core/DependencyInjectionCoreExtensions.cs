using Core.Validation;
using FluentValidation;
using Framework.Endpoints;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FileService.Core.Features.DeleteFile;
using static FileService.Core.Features.DownloadFileEndPoint;
using static FileService.Core.Features.GenerateDownloadUrl;
using static FileService.Core.Features.GenerateDownloadUrls;
using static FileService.Core.Features.GenerateUploadUrl;
using static FileService.Core.Features.UploadFileEndPoint;

namespace FileService.Core
{
    public static class DependencyInjectionCoreExtensions
    {
        public static IServiceCollection AddCore(this IServiceCollection services)
        {
            var assembly = typeof(DependencyInjectionCoreExtensions).Assembly;

            services.AddEndpoints(assembly);

            services.AddScoped<UploadFileHandler>();

            services.AddScoped<DownloadFileHandler>();

            services.AddScoped<DeleteFileHandler>();

            services.AddScoped<GeneratePresignedUploadUrlHandler>();

            services.AddScoped<GeneratePresignedDownloadUrlHandler>();

            services.AddScoped<GeneratePresignedDownloadUrlsHandler>();

            services.AddValidatorsFromAssembly(typeof(CustomValidators).Assembly);

            services.AddValidatorsFromAssemblyContaining<UploadFileHandler>();

            return services;
        }
    }
}
