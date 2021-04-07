namespace Microsoft.Extensions.DependencyInjection
{
    public static class CorsServiceCollectionExtensions
    {
        public static IServiceCollection AddAndConfigureCors(this IServiceCollection services)
        {
            //TODO: Allow anybody to make a call to this API with any HTTP verb. After that, authorization of particular methods and
            //clients will be done by the authentication server
            return services.AddCors(setupAction =>
            setupAction.AddDefaultPolicy(configurePolicy =>
            {
                configurePolicy.AllowAnyOrigin();
                configurePolicy.AllowAnyMethod();
            }));
        }
    }
}