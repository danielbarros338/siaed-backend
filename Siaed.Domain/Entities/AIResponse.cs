namespace Siaed.Domain.Entities;

public sealed class AIResponse : BaseEntity
{
    public Guid AIRequestId { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public string FinishReason { get; private set; } = string.Empty;
    public int TokensUsed { get; private set; }
    public string Model { get; private set; } = string.Empty;

    private AIResponse() { }

    public static AIResponse Create(
        Guid aiRequestId,
        string content,
        string finishReason,
        int tokensUsed,
        string model)
    {
        return new AIResponse
        {
            AIRequestId = aiRequestId,
            Content = content,
            FinishReason = finishReason,
            TokensUsed = tokensUsed,
            Model = model
        };
    }
}
