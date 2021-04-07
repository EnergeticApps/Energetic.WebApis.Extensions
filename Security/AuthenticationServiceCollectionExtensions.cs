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
using Microsoft.Extensions.DependencyInjection.Authorization;
using Energetic.WebApis.Extensions;
using Energetic.WebApis;
using System.Net.Http;
using Polly;
using Polly.Extensions.Http;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AuthenticationServiceCollectionExtensions
    {
        public static IServiceCollection AddJwtAuthentication(
            this IServiceCollection services)
        {
            services.AddSingleton<IConfigureOptions<AuthenticationOptions>, ConfigureAuthenticationOptions>();
            services.AddSingleton<IConfigureOptions<JwtBearerOptions>, ConfigureJwtBearerOptions>();

            services.AddAuthentication().AddJwtBearer();

            return services;
        }

        public static IServiceCollection AddHttpClientToGetAuthenticationServerDiscoveryDocument(this IServiceCollection services)
        {
            services.AddHttpClient(NamedHttpClients.AuthenticationServerDiscoveryDocumentClient)
                .AddPolicyHandler(GetRetryPolicy());

            return services;
        }

        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
                // Retry five times after delay  
                .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(1));
        }
    }
}
