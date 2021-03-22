using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AuthorizationServiceCollectionExtensions
    {
        public static IServiceCollection AddPermissionsAuthorization<TPermissionsAuthorizationPolicyProvider, TPermissionsAuthorizationHandler>
            (this IServiceCollection services)
            where TPermissionsAuthorizationPolicyProvider :class, IAuthorizationPolicyProvider
            where TPermissionsAuthorizationHandler : class, IAuthorizationHandler
        {
            return services
                .AddSingleton<IAuthorizationPolicyProvider, TPermissionsAuthorizationPolicyProvider>()
                .AddSingleton<IAuthorizationHandler, TPermissionsAuthorizationHandler>();
        }
    }
}
