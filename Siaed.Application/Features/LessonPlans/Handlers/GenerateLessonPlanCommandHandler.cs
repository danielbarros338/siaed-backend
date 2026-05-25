using MediatR;
using Siaed.Application.Common;
using Siaed.Application.Features.LessonPlans.Commands;
using Siaed.Application.Interfaces;
using Siaed.Domain.Entities;
using Siaed.Domain.Enums;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Siaed.Application.Features.LessonPlans.Handlers;

public sealed class GenerateLessonPlanCommandHandler
    : IRequestHandler<GenerateLessonPlanCommand, Result<Guid>>
{
    private readonly ILessonPlanRepository _lessonPlanRepo;
    private readonly IAIRequestRepository _aiRequestRepo;
    private readonly IOpenAIService _openAIService;
    private readonly IPromptBuilder _promptBuilder;
    private readonly IUnitOfWork _unitOfWork;

    private sealed record LessonPlanAIContent(
        [property: JsonPropertyName("title")] string Title,
        [property: JsonPropertyName("objectives")] string Objectives,
        [property: JsonPropertyName("content")] string Content,
        [property: JsonPropertyName("methodology")] string Methodology,
        [property: JsonPropertyName("resources")] string Resources,
        [property: JsonPropertyName("evaluation")] string Evaluation);

    public GenerateLessonPlanCommandHandler(
        ILessonPlanRepository lessonPlanRepo,
        IAIRequestRepository aiRequestRepo,
        IOpenAIService openAIService,
        IPromptBuilder promptBuilder,
        IUnitOfWork unitOfWork)
    {
        _lessonPlanRepo = lessonPlanRepo;
        _aiRequestRepo = aiRequestRepo;
        _openAIService = openAIService;
        _promptBuilder = promptBuilder;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(GenerateLessonPlanCommand request, CancellationToken ct)
    {
        var context = new LessonPlanPromptContext(
            request.Subject,
            request.Grade,
            request.AgeRange,
            request.DurationMinutes,
            request.AdditionalInstructions);

        var prompt = _promptBuilder.BuildLessonPlanPrompt(context);

        var aiRequest = AIRequest.Create(
            request.TeacherId,
            AIRequestType.LessonPlan,
            prompt,
            JsonSerializer.Serialize(request),
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
            await _unitOfWork.CommitAsync(CancellationToken.None);
            return Result.Failure<Guid>("Falha ao gerar plano de aula com IA. Tente novamente.");
        }

        aiRequest.MarkAsCompleted(completionResult.TotalTokens, completionResult.EstimatedCost);

        var aiResponse = AIResponse.Create(
            aiRequest.Id,
            completionResult.Content,
            completionResult.FinishReason,
            completionResult.TotalTokens,
            "gpt-4o-mini");
        await _aiRequestRepo.AddResponseAsync(aiResponse, ct);

        var parsed = ParseAIContent(completionResult.Content, request.Subject, request.Grade);

        var lessonPlan = LessonPlan.Create(
            request.TeacherId,
            parsed.Title,
            request.Subject,
            request.Grade,
            request.DurationMinutes,
            parsed.Objectives,
            parsed.Content,
            parsed.Methodology,
            parsed.Resources,
            parsed.Evaluation,
            request.AgeRange,
            isAIGenerated: true);

        await _lessonPlanRepo.AddAsync(lessonPlan, ct);
        await _unitOfWork.CommitAsync(ct);

        return Result.Success(lessonPlan.Id);
    }

    private static LessonPlanAIContent ParseAIContent(string raw, string subject, string grade)
    {
        try
        {
            var json = StripMarkdownCodeBlock(raw);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<LessonPlanAIContent>(json, options);
            if (result is not null &&
                !string.IsNullOrWhiteSpace(result.Title) &&
                !string.IsNullOrWhiteSpace(result.Content))
            {
                return result;
            }
        }
        catch { /* fallback abaixo */ }

        return new LessonPlanAIContent(
            Title: $"Plano de Aula — {subject} ({grade})",
            Objectives: "Consulte o conteúdo gerado.",
            Content: raw,
            Methodology: "Consulte o conteúdo gerado.",
            Resources: "Consulte o conteúdo gerado.",
            Evaluation: "Consulte o conteúdo gerado.");
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

