using Microsoft.Extensions.DependencyInjection;
using Energetic.Persistence.Cqrs;
using System;
using Energetic.WebApis.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class RepositoriesServiceCollectionExtensions
    {
        private static Type s_readSideRepositoryGenericInterface = typeof(IReadSideRepository<>);
        private static Type s_writeSideRepositoryGenericInterface = typeof(IWriteSideRepository<,>);

        public static IServiceCollection AddAllReadSideRepositories(this IServiceCollection services, params Type[] assemblyMarkerTypes)
        {
            return services.Scan(action => action
            .FromAssembliesOf(assemblyMarkerTypes)
            .AddClasses(classes => classes.AssignableTo(s_readSideRepositoryGenericInterface))
            .AsImplementedInterfaces()
            .WithScopedLifetime());
        }

        public static IServiceCollection AddAllWriteSideRepositories(this IServiceCollection services, params Type[] assemblyMarkerTypes)
        {
            return services.Scan(action => action
                .FromAssembliesOf(assemblyMarkerTypes)
                .AddClasses(classes => classes.AssignableTo(s_writeSideRepositoryGenericInterface))
                .AsImplementedInterfaces()
                .WithScopedLifetime());
        }
    }
}