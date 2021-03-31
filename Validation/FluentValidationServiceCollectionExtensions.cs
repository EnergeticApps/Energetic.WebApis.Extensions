using Microsoft.Extensions.DependencyInjection;
using FluentValidation.AspNetCore;
using System;
using Energetic.Reflection;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc;
using MediatR;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class FluentValidationServiceCollectionExtensions
    {
        public static IServiceCollection AddMvcWithFluentValidationAndValidatorsFromAssemblies(this IServiceCollection services, params Type[] validatorMarkerTypes)
        {
            var assemblies = validatorMarkerTypes.GetContainingAssemblies();
            services.AddMvc()
                .AddFluentValidation(configuration => configuration.RegisterValidatorsFromAssemblies(assemblies));

            return services.AddTransientGenericValidationBehaviours();
        }

        public static IServiceCollection AddMvcCoreWithFluentValidationAndValidatorsFromAssemblies(this IServiceCollection services, params Type[] validatorMarkerTypes)
        {
            var assemblies = validatorMarkerTypes.GetContainingAssemblies();
            services.AddMvcCore()
                .AddFluentValidation(configuration => configuration.RegisterValidatorsFromAssemblies(assemblies));

            return services.AddTransientGenericValidationBehaviours();
        }

        public static IServiceCollection AddControllersWithFluentValidationAndRegisterValidatorsFromAssemblies(this IServiceCollection services, params Type[] validatorMarkerTypes)
        {
            services.AddTransientGenericValidationBehaviours();

            var assemblies = validatorMarkerTypes.GetContainingAssemblies();

            services.AddControllers()
                .AddFluentValidation(configuration => configuration.RegisterValidatorsFromAssemblies(assemblies));

            return services;
        }


        private static IServiceCollection AddTransientGenericValidationBehaviours(this IServiceCollection services)
        {
            return services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
        }
    }
}
