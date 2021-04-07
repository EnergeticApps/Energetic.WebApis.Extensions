using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    internal class UIShouldDisplayControllerActionMethodsInLowercase : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var paths = swaggerDoc.Paths.ToDictionary(entry => LowercaseEverythingButParameters(entry.Key),
                entry => entry.Value);

            swaggerDoc.Paths = new OpenApiPaths();

            foreach (var (key, value) in paths)
            {
                swaggerDoc.Paths.Add(key, value);
            }
        }

        private static string LowercaseEverythingButParameters(string key) => 
            string.Join('/', key.Split('/')
                .Select(x => x.Contains("{") ? x : x.ToLower()));
    }
}