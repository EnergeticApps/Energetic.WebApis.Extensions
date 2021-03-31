using Energetic.Security;
using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection
{
    public class AuthorizationBehaviour<TRequest, TResponse> : IAuthorizationBehaviour<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IAuthorizer<TRequest> _authorizer;

        public AuthorizationBehaviour(IAuthorizer<TRequest> authorizer)
        {
            _authorizer = authorizer ?? throw new ArgumentNullException(nameof(authorizer));
        }


        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            await _authorizer.AuthorizeAsync(request);
            return await next();
        }
    }
}
