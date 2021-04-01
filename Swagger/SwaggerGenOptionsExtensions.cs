using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public static class SwaggerGenOptionsExtensions
    {
        public static void IncludeXmlComments(this SwaggerGenOptions options)
        {
            var entryAssembly = Assembly.GetEntryAssembly();

            if (entryAssembly is { })
            {
                var xmlFile = $"{entryAssembly.GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

                try
                {
                    options.IncludeXmlComments(xmlPath);
                }
                catch (FileNotFoundException ex)
                {
                    throw new InvalidOperationException($"Couldn't find the XML documentation file at {xmlPath}. Ensure that XML documentation " +
                        $"is enabled in the project and that the path is set in the \"XML documentation file\" field of the \"Output\" section " +
                        $"of your project's \"Build\" settings.", ex);
                }
            }
        }
    }
}
