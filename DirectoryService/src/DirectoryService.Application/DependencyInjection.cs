using Core.Handlers;
using Core.Validation;
using DirectoryService.Application.Locations;
using FileService.Communication;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryService.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
        {
            var assembly = typeof(DependencyInjection).Assembly;

            services.Scan(scan => scan.FromAssemblies(assembly)
                .AddClasses(classes => classes.Where(type => type.Name.EndsWith("Handler")))
                .AsSelfWithInterfaces()
                .WithScopedLifetime());

            services.AddValidatorsFromAssembly(typeof(CustomValidators).Assembly);

            services.AddValidatorsFromAssemblyContaining<CreateLocationHandler>();

            services.AddFileServiceHttpCommunication(configuration);

            return services;
        }
    }
}
