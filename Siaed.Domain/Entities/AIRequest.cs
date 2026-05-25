using Siaed.Domain.Enums;

namespace Siaed.Domain.Entities;

public sealed class AIRequest : BaseEntity
{
    public Guid TeacherId { get; private set; }
    public AIRequestType RequestType { get; private set; }
    public string Prompt { get; private set; } = string.Empty;
    public string InputData { get; private set; } = string.Empty;
    public string Model { get; private set; } = string.Empty;
    public int MaxTokens { get; private set; }
    public int TokensUsed { get; private set; }
    public decimal EstimatedCost { get; private set; }
    public AIRequestStatus Status { get; private set; } = AIRequestStatus.Pending;
    public string? ErrorMessage { get; private set; }

    private AIRequest() { }

    public static AIRequest Create(
        Guid teacherId,
        AIRequestType requestType,
        string prompt,
        string inputData,
        string model,
        int maxTokens)
    {
        return new AIRequest
        {
            TeacherId = teacherId,
            RequestType = requestType,
            Prompt = prompt,
            InputData = inputData,
            Model = model,
            MaxTokens = maxTokens,
            Status = AIRequestStatus.Pending
        };
    }

    public void MarkAsProcessing()
    {
        Status = AIRequestStatus.Processing;
        MarkAsUpdated();
    }

    public void MarkAsCompleted(int tokensUsed, decimal estimatedCost)
    {
        Status = AIRequestStatus.Completed;
        TokensUsed = tokensUsed;
        EstimatedCost = estimatedCost;
        MarkAsUpdated();
    }

    public void MarkAsFailed(string errorMessage)
    {
        Status = AIRequestStatus.Failed;
        ErrorMessage = errorMessage;
        MarkAsUpdated();
    }
}
