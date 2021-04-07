using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    internal class AuthorizeOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var authAttributes = GetAuthorizationAttributes(context);

            if (authAttributes.Any())
            {
                AddAuthenticationAndAuthorizationResponses(operation);

                var oauth2SecurityScheme = MakeOAuth2SecurityScheme();
                UseOAuth2SecurityScheme(operation, oauth2SecurityScheme);
            }
        }

        private static void UseOAuth2SecurityScheme(OpenApiOperation operation, OpenApiSecurityScheme oauth2SecurityScheme)
        {
            operation.Security = new List<OpenApiSecurityRequirement>
                {
                    new OpenApiSecurityRequirement()
                    {
                        [oauth2SecurityScheme] = new[] { "OAuth2" }
                    }
                };
        }

        private static OpenApiSecurityScheme MakeOAuth2SecurityScheme()
        {
            return new OpenApiSecurityScheme()
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "OAuth2" },
            };
        }

        private static void AddAuthenticationAndAuthorizationResponses(OpenApiOperation operation)
        {
            operation.Responses.Add(StatusCodes.Status401Unauthorized.ToString(), new OpenApiResponse { Description = nameof(HttpStatusCode.Unauthorized) });
            operation.Responses.Add(StatusCodes.Status403Forbidden.ToString(), new OpenApiResponse { Description = nameof(HttpStatusCode.Forbidden) });
        }

        private static IEnumerable<AuthorizeAttribute> GetAuthorizationAttributes(OperationFilterContext context)
        {
            var methodInfo = context.MethodInfo;
            var classInfo = methodInfo.DeclaringType;

            if (classInfo is null)
                throw new InvalidOperationException($"The declaring type information could not be ascertained for method {methodInfo.Name}.");

            var authAttributes = classInfo.GetCustomAttributes(true)
                            .Union(context.MethodInfo.GetCustomAttributes(true))
                            .OfType<AuthorizeAttribute>();

            return authAttributes;
        }
    }
}