using Core.Validation;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileService.Core
{
    public static class DependencyInjectionCoreExtensions
    {
        public static IServiceCollection AddCore(this IServiceCollection services)
        {
            var assembly = typeof(DependencyInjectionCoreExtensions).Assembly;

            services.AddValidatorsFromAssembly(typeof(CustomValidators).Assembly);

            return services;
        }
    }
}
