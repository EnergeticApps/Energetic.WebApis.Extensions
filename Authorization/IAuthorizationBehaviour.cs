using MediatR;

namespace Microsoft.Extensions.DependencyInjection
{
    public interface IAuthorizationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    { }
}