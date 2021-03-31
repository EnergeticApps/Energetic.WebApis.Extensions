using Microsoft.Extensions.DependencyInjection;
using Energetic.Persistence.Cqrs;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class RepositoriesServiceCollectionExtensions
    {
        private static Type s_readSideRepositoryGenericInterface = typeof(IReadSideRepository<>);
        private static Type s_writeSideRepositoryGenericInterface = typeof(IWriteSideRepository<,>);

        public static IServiceCollection AddAllReadSideRepositories(this IServiceCollection services, params Type[] readSideRepositoryMarkerTypes)
        {
            return services.Scan(action => action
            .FromAssembliesOf(readSideRepositoryMarkerTypes)
            .AddClasses(classes => classes.AssignableTo(s_readSideRepositoryGenericInterface))
            .AsImplementedInterfaces()
            .WithScopedLifetime());
        }

        public static IServiceCollection AddAllWriteSideRepositories(this IServiceCollection services, params Type[] writeSideRepositoryMarkerTypes)
        {
            return services.Scan(action => action
                .FromAssembliesOf(writeSideRepositoryMarkerTypes)
                .AddClasses(classes => classes.AssignableTo(s_writeSideRepositoryGenericInterface))
                .AsImplementedInterfaces()
                .WithScopedLifetime());
        }
    }
}