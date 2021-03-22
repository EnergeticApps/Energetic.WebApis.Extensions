using MediatR;

namespace Microsoft.Extensions.DependencyInjection
{
    public interface IValidationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    { }
}
