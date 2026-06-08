using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;
using Siaed.Application.Interfaces.Repositories;

namespace Siaed.Infra.OpenAI;

public sealed class OpenAIService : IOpenAIService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<OpenAIService> _logger;

    public OpenAIService(IConfiguration configuration, ILogger<OpenAIService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<AICompletionResult> CompleteAsync(AICompletionRequest request, CancellationToken ct = default)
    {
        var apiKey = _configuration["OpenAI:ApiKey"]
            ?? throw new InvalidOperationException("OpenAI API Key não configurada.");

        var client = new ChatClient(request.Model, apiKey);

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(request.SystemMessage),
            new UserChatMessage(request.Prompt)
        };

        var options = new ChatCompletionOptions
        {
            MaxOutputTokenCount = request.MaxTokens
        };

        _logger.LogInformation("Enviando requisição para OpenAI. Modelo: {Model}, MaxTokens: {MaxTokens}",
            request.Model, request.MaxTokens);

        var response = await client.CompleteChatAsync(messages, options, ct);
        var completion = response.Value;

        var content = completion.Content[0].Text;
        var finishReason = completion.FinishReason.ToString() ?? "stop";
        var promptTokens = completion.Usage.InputTokenCount;
        var completionTokens = completion.Usage.OutputTokenCount;
        var totalTokens = completion.Usage.TotalTokenCount;

        var estimatedCost = CalculateCost(request.Model, promptTokens, completionTokens);

        _logger.LogInformation(
            "Resposta OpenAI recebida. Tokens usados: {TotalTokens}, Custo estimado: {Cost:C6}",
            totalTokens, estimatedCost);

        return new AICompletionResult(content, finishReason, promptTokens, completionTokens, totalTokens, estimatedCost);
    }

    private static decimal CalculateCost(string model, int promptTokens, int completionTokens)
    {
        return model switch
        {
            "gpt-4o" => (promptTokens * 0.000005m) + (completionTokens * 0.000015m),
            "gpt-4o-mini" => (promptTokens * 0.00000015m) + (completionTokens * 0.0000006m),
            _ => 0m
        };
    }
}
