using Microsoft.Extensions.DependencyInjection;
using FluentValidation.AspNetCore;
using System;
using Energetic.Reflection;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class FluentValidationServiceCollectionExtensions
    {
        public static IServiceCollection AddMvcWithFluentValidationAndValidatorsFromAssemblies(this IServiceCollection services, params Type[] validatorMarkerTypes)
        {
            var assemblies = validatorMarkerTypes.GetContainingAssemblies();
            services.AddMvc()
                .AddFluentValidation(configuration => configuration.RegisterValidatorsFromAssemblies(assemblies));

            return services.AddTransientGenericValidators();
        }

        private static IServiceCollection AddTransientGenericValidators(this IServiceCollection services)
        {
            return services.AddTransient(typeof(IValidationBehaviour<,>), typeof(ValidationBehaviour<,>));
        }

        public static IServiceCollection AddMvcCoreWithFluentValidationAndValidatorsFromAssemblies(this IServiceCollection services, params Type[] validatorMarkerTypes)
        {
            var assemblies = validatorMarkerTypes.GetContainingAssemblies();
            services.AddMvcCore()
                .AddFluentValidation(configuration => configuration.RegisterValidatorsFromAssemblies(assemblies));

            return services.AddTransientGenericValidators();
        }

        public static IServiceCollection AddFluentValidationAndRegisterValidatorsFromAssemblies(this IServiceCollection services, params Type[] validatorMarkerTypes)
        {
            var assemblies = validatorMarkerTypes.GetContainingAssemblies();
            services.AddControllers()
                .AddFluentValidation(configuration => configuration.RegisterValidatorsFromAssemblies(assemblies));

            return services.AddTransientGenericValidators();
        }
    }
}
