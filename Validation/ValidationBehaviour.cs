using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection
{
    public class ValidationBehaviour<TRequest, TResponse> : IValidationBehaviour<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> validators;

        public ValidationBehaviour(IEnumerable<IValidator<TRequest>> validators)
        {
            this.validators = validators;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            var context = new ValidationContext<TRequest>(request);

            var failures = this.validators.Select(validator => validator.Validate(context))
                .SelectMany(validator => validator.Errors)
                .Where(error => error is { });

            if (!failures.Any())
            {
                // TODO: We should try to do this without throwing exceptions because validation errors are a pretty normal part of life.
                // It was non-trivial when I tried.

                throw new ValidationException(failures);
                //return new Invalid(failures.ToDictionary(failure => failure.PropertyName, failure => failure.ErrorMessage));
            }

            return await next();
        }
    }
}
