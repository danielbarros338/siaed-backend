using FluentValidation;
using MediatR;
using Siaed.Application.Common;

namespace Siaed.Application.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        => _validators = validators;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, ct)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count != 0)
        {
            var errors = failures.Select(f => f.ErrorMessage);
            return CreateFailureResult(errors);
        }

        return await next();
    }

    private static TResponse CreateFailureResult(IEnumerable<string> errors)
    {
        if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
        {
            var valueType = typeof(TResponse).GetGenericArguments()[0];
            var method = typeof(Result<>).MakeGenericType(valueType)
                .GetMethod("Failure", [typeof(IEnumerable<string>)])!;
            return (TResponse)method.Invoke(null, [errors])!;
        }
        return (TResponse)(object)Result.Failure(errors);
    }
}
