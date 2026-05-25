using MediatR;
using Siaed.Application.Common;
using Siaed.Application.Features.Activities.Commands;
using Siaed.Application.Interfaces;
using Siaed.Domain.Entities;
using Siaed.Domain.Enums;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Siaed.Application.Features.Activities.Handlers;

public sealed class GenerateActivityCommandHandler
    : IRequestHandler<GenerateActivityCommand, Result<Guid>>
{
    private sealed record ActivityAIContent(
        [property: JsonPropertyName("title")] string Title,
        [property: JsonPropertyName("description")] string Description,
        [property: JsonPropertyName("content")] string Content,
        [property: JsonPropertyName("answerKey")] string AnswerKey,
        [property: JsonPropertyName("simplifiedVersion")] string SimplifiedVersion);
    private readonly IActivityRepository _activityRepo;
    private readonly ILessonPlanRepository _lessonPlanRepo;
    private readonly IAIRequestRepository _aiRequestRepo;
    private readonly IOpenAIService _openAIService;
    private readonly IPromptBuilder _promptBuilder;
    private readonly IUnitOfWork _unitOfWork;

    public GenerateActivityCommandHandler(
        IActivityRepository activityRepo,
        ILessonPlanRepository lessonPlanRepo,
        IAIRequestRepository aiRequestRepo,
        IOpenAIService openAIService,
        IPromptBuilder promptBuilder,
        IUnitOfWork unitOfWork)
    {
        _activityRepo = activityRepo;
        _lessonPlanRepo = lessonPlanRepo;
        _aiRequestRepo = aiRequestRepo;
        _openAIService = openAIService;
        _promptBuilder = promptBuilder;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(GenerateActivityCommand request, CancellationToken ct)
    {
        LessonPlanData? lessonPlanData = null;
        if (request.LessonPlanId.HasValue)
        {
            var lessonPlan = await _lessonPlanRepo.GetByIdAsync(request.LessonPlanId.Value, ct);
            if (lessonPlan is not null)
            {
                lessonPlanData = new LessonPlanData(
                    lessonPlan.Title,
                    lessonPlan.Objectives,
                    lessonPlan.Content,
                    lessonPlan.Methodology,
                    lessonPlan.Resources,
                    lessonPlan.Evaluation);
            }
        }

        var context = new ActivityPromptContext(
            request.Subject,
            request.Grade,
            request.AgeRange,
            request.Type.ToString(),
            request.NumberOfQuestions,
            request.AdditionalInstructions,
            lessonPlanData);

        var prompt = _promptBuilder.BuildActivityPrompt(context);

        var aiRequest = AIRequest.Create(
            request.TeacherId,
            AIRequestType.Activity,
            prompt,
            System.Text.Json.JsonSerializer.Serialize(request),
            "gpt-4o-mini",
            2000);

        await _aiRequestRepo.AddAsync(aiRequest, ct);
        aiRequest.MarkAsProcessing();

        AICompletionResult completionResult;
        try
            {
                completionResult = await _openAIService.CompleteAsync(
                    new AICompletionRequest(
                        prompt,
                        "Você é um assistente pedagógico especializado. Responda sempre em português brasileiro com linguagem clara e pedagógica. Responda APENAS com JSON válido, sem markdown.",
                        2000,
                        "gpt-4o-mini"),
                    ct);
            }
        catch (Exception ex)
        {
            aiRequest.MarkAsFailed(ex.Message);
            await _unitOfWork.CommitAsync(ct);
            return Result.Failure<Guid>("Falha ao gerar atividade com IA. Tente novamente.");
        }

        aiRequest.MarkAsCompleted(completionResult.TotalTokens, completionResult.EstimatedCost);

        var aiResponse = AIResponse.Create(aiRequest.Id, completionResult.Content, completionResult.FinishReason, completionResult.TotalTokens, "gpt-4o-mini");
        await _aiRequestRepo.AddResponseAsync(aiResponse, ct);

        var parsed = ParseAIContent(completionResult.Content, request.Subject, request.Grade);

        var activity = Activity.Create(
            request.TeacherId,
            parsed.Title,
            parsed.Description,
            request.Subject,
            request.Grade,
            request.AgeRange,
            parsed.Content,
            request.Type,
            request.LessonPlanId,
            isAIGenerated: true);

        if (!string.IsNullOrWhiteSpace(parsed.AnswerKey))
            activity.SetAnswerKey(parsed.AnswerKey);

        if (!string.IsNullOrWhiteSpace(parsed.SimplifiedVersion))
            activity.SetSimplifiedVersion(parsed.SimplifiedVersion);

        await _activityRepo.AddAsync(activity, ct);
        await _unitOfWork.CommitAsync(ct);

        return Result.Success(activity.Id);
    }

    private static ActivityAIContent ParseAIContent(string raw, string subject, string grade)
    {
        try
        {
            var json = StripMarkdownCodeBlock(raw);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<ActivityAIContent>(json, options);
            if (result is not null &&
                !string.IsNullOrWhiteSpace(result.Title) &&
                !string.IsNullOrWhiteSpace(result.Content))
            {
                return result;
            }
        }
        catch { /* fallback abaixo */ }

        return new ActivityAIContent(
            Title: $"Atividade — {subject} ({grade})",
            Description: "Consulte o conteúdo gerado.",
            Content: raw,
            AnswerKey: string.Empty,
            SimplifiedVersion: string.Empty);
    }

    private static string StripMarkdownCodeBlock(string text)
    {
        var t = text.Trim();
        if (!t.StartsWith("```")) return t;
        var firstNewLine = t.IndexOf('\n');
        if (firstNewLine >= 0) t = t[(firstNewLine + 1)..];
        if (t.EndsWith("```")) t = t[..^3];
        return t.Trim();
    }
}
