using FluentValidation;
using MediatR;

namespace GoVoylo.Application.Common.Behaviors;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    // Inject all validators registered for this specific request type
    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // 1. If there are no validation rules defined for this request, skip and move to the next behavior
        if (!_validators.Any())
        {
            return await next();
        }

        // 2. Create the validation context from the incoming request data
        var context = new ValidationContext<TRequest>(request);

        // 3. Execute all validators asynchronously
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken))
        );

        // 4. Collect all failure errors
        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        // 5. If any validation rules failed, short-circuit and throw an exception
        if (failures.Any())
        {
            throw new ValidationException(failures); 
            // This exception will be caught globally by your API middleware later!
        }

        // 6. If everything is valid, pass control down the pipeline to the next behavior or handler
        return await next();
    }
}
