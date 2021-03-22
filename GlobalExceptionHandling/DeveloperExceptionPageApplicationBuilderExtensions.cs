using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Microsoft.AspNetCore.Builder
{
    // I excluded this because (I THINK) in an API we wouldn't need it. There should be no pages (apart from Swagger UI) as everything will be status code
    // responses with JSON. And errors will be handled with Hellang's ProblemDetails middleware so MAYBE we can discard this
    public static class DeveloperExceptionPageApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseDeveloperExceptionPageInDevEnvironment(this IApplicationBuilder app, IWebHostEnvironment environment)
        {
            if (environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            return app;
        }
    }
}
