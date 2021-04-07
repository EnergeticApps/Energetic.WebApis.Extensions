using System;

namespace Energetic.WebApis.Extensions
{
    public interface IAssemblyMarkerTypes
    {
        Type[] AutoMapperProfiles { get; }
        Type[] MediatorQueriesCommandsAndHandlers { get; }
        Type[] ReadSideRepositories { get; }
        Type[] WriteSideRepositories { get; }
        Type[] FluentValidationValidators { get; }
        Type[] ValueObjects { get; }
        Type[] Authorizers { get; }
    }
}