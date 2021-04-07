using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    /// <summary>
    /// When this filter is used in the <see cref="SwaggerGenServiceCollectionExtensions.AddSwaggerGen"/> method, it stops the Swagger UI from
    /// displaying the default (unversioned) endpoints and only shows the versioned ones. This allows us to permit calls to API endpoints without
    /// specificying a version, while not cluttering up the Swagger UI with duplicate routes.
    /// </summary>
    public class UIShouldHideMethodsOfDefaultApiVersionFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var endpoints = context.ApiDescriptions;

            foreach (var endpoint in endpoints)
            {
                string version = endpoint.GroupName;
                bool isSpecificVersion = endpoint.RelativePath.StartsWith($"api/{version}/");

                if (!isSpecificVersion)
                {
                    var route = "/" + endpoint.RelativePath.TrimEnd('/').ToLowerInvariant();
                    swaggerDoc.Paths.Remove(route);
                }
            }
        }
    }
}