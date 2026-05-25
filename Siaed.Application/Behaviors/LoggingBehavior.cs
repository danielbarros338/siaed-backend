using MediatR;
using Microsoft.Extensions.Logging;
using Siaed.Application.Common;

namespace Siaed.Application.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        => _logger = logger;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var requestName = typeof(TRequest).Name;
        _logger.LogInformation("Iniciando {RequestName}", requestName);

        var sw = System.Diagnostics.Stopwatch.StartNew();
        var response = await next();
        sw.Stop();

        if (response is Result result && result.IsFailure)
            _logger.LogWarning("{RequestName} falhou em {ElapsedMs}ms. Erros: {Errors}",
                requestName, sw.ElapsedMilliseconds, string.Join(", ", result.Errors));
        else
            _logger.LogInformation("{RequestName} concluído em {ElapsedMs}ms", requestName, sw.ElapsedMilliseconds);

        return response;
    }
}
