using Microsoft.Extensions.DependencyInjection;
using FluentValidation.AspNetCore;
using System;
using Energetic.Reflection;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Energetic.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AuthorizationServiceCollectionExtensions
    {
        private static Type s_authorizerGenericInterface = typeof(IAuthorizer<>);

        public static IServiceCollection AddPermissionsAuthorization<TPermissionsAuthorizationPolicyProvider, TPermissionsAuthorizationHandler>(this IServiceCollection services)
            where TPermissionsAuthorizationPolicyProvider : class, IAuthorizationPolicyProvider
            where TPermissionsAuthorizationHandler : class, IAuthorizationHandler
        {
            return services
                .AddSingleton<IAuthorizationPolicyProvider, TPermissionsAuthorizationPolicyProvider>()
                .AddSingleton<IAuthorizationHandler, TPermissionsAuthorizationHandler>();
        }

        public static IServiceCollection AddAuthorizationAndAuthentication(
            this IServiceCollection services,
            string authority,
            string apiScopeClaimValue)
        {
            services.AddAuthenticationWithJwtBearer(authority);
            return services.AddAuthorization(apiScopeClaimValue);
        }

        private static AuthenticationBuilder AddAuthenticationWithJwtBearer(this IServiceCollection services, string authority)
        {
            return services.AddAuthentication(options =>
           {
               options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
               options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
           })
                .AddJwtBearer("Bearer", options =>
                    {
                        options.RequireHttpsMetadata = false;
                        options.Authority = authority;
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateAudience = false
                        };
                    });
        }

        private static IServiceCollection AddAuthorization(this IServiceCollection services, string apiScopeClaimValue)
        {
            return services.AddAuthorization(options =>
            {
                options.AddPolicy("ApiScope", policy =>
                {
                    policy.RequireAuthenticatedUser();
                policy.RequireClaim("scope", apiScopeClaimValue);
                });
            });
        }

        public static IServiceCollection AddAuthorizationBehavioursWithAuthorizersFromAssemblies(
            this IServiceCollection services, 
            params Type[] authorizerMarkerTypes)
        {
            services.AddTransientGenericAuthorizationBehaviours();

            return services.Scan(action => action
                .FromAssembliesOf(authorizerMarkerTypes)
                .AddClasses(classes => classes.AssignableTo(s_authorizerGenericInterface))
                .AsImplementedInterfaces()
                .WithScopedLifetime());
        }

        private static IServiceCollection AddTransientGenericAuthorizationBehaviours(this IServiceCollection services)
        {
            return services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehaviour<,>));
        }
    }
}
