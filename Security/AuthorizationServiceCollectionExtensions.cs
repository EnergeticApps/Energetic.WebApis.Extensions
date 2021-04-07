using Energetic.Security;
using MediatR;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AuthorizationServiceCollectionExtensions
    {
        private static Type s_authorizerGenericInterface = typeof(IAuthorizer<>);

        public static IServiceCollection AddAuthorizationBehavioursWithAuthorizersFromAssemblies(
            this IServiceCollection services,
            params Type[] authorizerMarkerTypes)
        {
            services.AddTransientGenericAuthorizationBehaviours();
            return services.AddAuthorizersFromAssemblies(authorizerMarkerTypes);
        }

        private static IServiceCollection AddTransientGenericAuthorizationBehaviours(this IServiceCollection services)
        {
            return services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehaviour<,>));
        }

        private static IServiceCollection AddAuthorizersFromAssemblies(this IServiceCollection services, params Type[] authorizerMarkerTypes)
        {
            return services.Scan(action => action
                .FromAssembliesOf(authorizerMarkerTypes)
                .AddClasses(classes => classes.AssignableTo(s_authorizerGenericInterface))
                .AsImplementedInterfaces()
                .WithScopedLifetime());
        }
    }
}