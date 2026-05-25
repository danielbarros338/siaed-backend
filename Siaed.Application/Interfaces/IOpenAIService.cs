namespace Siaed.Application.Interfaces;

public interface IOpenAIService
{
    Task<AICompletionResult> CompleteAsync(AICompletionRequest request, CancellationToken ct = default);
}

public sealed record AICompletionRequest(
    string Prompt,
    string SystemMessage,
    int MaxTokens = 2000,
    string Model = "gpt-4o-mini");

public sealed record AICompletionResult(
    string Content,
    string FinishReason,
    int PromptTokens,
    int CompletionTokens,
    int TotalTokens,
    decimal EstimatedCost);
