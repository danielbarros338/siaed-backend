using MediatR;
using Siaed.Application.Common;
using Siaed.Application.Features.Reports.Commands;
using Siaed.Application.Interfaces;
using Siaed.Application.Interfaces.Repositories;
using Siaed.Domain.Entities;
using Siaed.Domain.Enums;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Siaed.Application.Features.Reports.Handlers;

public sealed class GenerateReportCommandHandler
    : IRequestHandler<GenerateReportCommand, Result<Guid>>
{
    private sealed record ReportAIContent(
        [property: JsonPropertyName("content")] string Content,
        [property: JsonPropertyName("summary")] string Summary,
        [property: JsonPropertyName("parentCommunication")] string ParentCommunication);

    private readonly IPedagogicalReportRepository _reportRepo;
    private readonly IStudentRepository _studentRepo;
    private readonly IGradeRepository _gradeRepo;
    private readonly IActivityRepository _activityRepo;
    private readonly IAIRequestRepository _aiRequestRepo;
    private readonly IOpenAIService _openAIService;
    private readonly IPromptBuilder _promptBuilder;
    private readonly IUnitOfWork _unitOfWork;

    public GenerateReportCommandHandler(
        IPedagogicalReportRepository reportRepo,
        IStudentRepository studentRepo,
        IGradeRepository gradeRepo,
        IActivityRepository activityRepo,
        IAIRequestRepository aiRequestRepo,
        IOpenAIService openAIService,
        IPromptBuilder promptBuilder,
        IUnitOfWork unitOfWork)
    {
        _reportRepo = reportRepo;
        _studentRepo = studentRepo;
        _gradeRepo = gradeRepo;
        _activityRepo = activityRepo;
        _aiRequestRepo = aiRequestRepo;
        _openAIService = openAIService;
        _promptBuilder = promptBuilder;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(GenerateReportCommand request, CancellationToken ct)
    {
        var student = await _studentRepo.GetByIdAsync(request.StudentId, ct);
        if (student is null)
            return Result.Failure<Guid>("Aluno não encontrado para geração do relatório.");

        var (teacherReports, _) = await _reportRepo.GetByUserIdAsync(request.UserId, 1, 100, null, ct);
        var previousReports = teacherReports
            .Where(r => r.StudentId == request.StudentId)
            .OrderByDescending(r => r.CreatedAt)
            .Take(request.HistoricalReportCount)
            .Select(r => $"[{r.CreatedAt:yyyy-MM-dd}] Resumo: {TruncateForPrompt(r.Summary, 300)}")
            .ToList();

        var grades = await _gradeRepo.ListByStudentIdAsync(request.StudentId, ct);
        var activities = await _activityRepo.GetByIdsAsync(grades.Select(g => g.ActivityId), ct);
        var activityTitles = activities.ToDictionary(activity => activity.Id, activity => activity.Title);
        var activityPerformances = grades
            .Select(grade =>
            {
                var activityTitle = activityTitles.GetValueOrDefault(grade.ActivityId, "Atividade sem título identificado");
                return $"Atividade: {activityTitle} | Nota do aluno: {grade.GradeValue}";
            })
            .ToList();

        var context = new ReportPromptContext(
            request.StudentId,
            student.FullName,
            student.Notes ?? string.Empty,
            activityPerformances,
            previousReports,
            request.AdditionalInstructions);

        var prompt = _promptBuilder.BuildReportPrompt(context);

        var aiRequest = AIRequest.Create(
            request.UserId,
            AIRequestType.Report,
            prompt,
            System.Text.Json.JsonSerializer.Serialize(request),
            "gpt-4o-mini",
            1500);

        await _aiRequestRepo.AddAsync(aiRequest, ct);
        aiRequest.MarkAsProcessing();

        AICompletionResult completionResult;
        try
        {
            completionResult = await _openAIService.CompleteAsync(
                new AICompletionRequest(
                    prompt,
                    "Você é um assistente pedagógico especializado. Responda sempre em português brasileiro com linguagem clara e empática. Responda APENAS com JSON válido, sem markdown.",
                    1500,
                    "gpt-4o-mini"),
                ct);
        }
        catch (Exception ex)
        {
            aiRequest.MarkAsFailed(ex.Message);
            await _unitOfWork.CommitAsync(ct);
            return Result.Failure<Guid>("Falha ao gerar relatório com IA. Tente novamente.");
        }

        aiRequest.MarkAsCompleted(completionResult.TotalTokens, completionResult.EstimatedCost);

        var aiResponse = AIResponse.Create(aiRequest.Id, completionResult.Content, completionResult.FinishReason, completionResult.TotalTokens, "gpt-4o-mini");
        await _aiRequestRepo.AddResponseAsync(aiResponse, ct);

        var parsed = ParseAIContent(completionResult.Content);

        var report = PedagogicalReport.Create(
            request.UserId,
            request.StudentId,
            parsed.Content,
            parsed.Summary,
            parsed.ParentCommunication,
            isAIGenerated: true);

        await _reportRepo.AddAsync(report, ct);
        await _unitOfWork.CommitAsync(ct);

        return Result.Success(report.Id);
    }

    private static ReportAIContent ParseAIContent(string raw)
    {
        try
        {
            var json = StripMarkdownCodeBlock(raw);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<ReportAIContent>(json, options);

            if (result is not null &&
                !string.IsNullOrWhiteSpace(result.Content) &&
                !string.IsNullOrWhiteSpace(result.Summary) &&
                !string.IsNullOrWhiteSpace(result.ParentCommunication))
            {
                return result;
            }
        }
        catch { }

        return new ReportAIContent(
            Content: raw,
            Summary: "Resumo não disponível na resposta estruturada da IA.",
            ParentCommunication: "Comunicado aos responsáveis não disponível na resposta estruturada da IA.");
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

    private static string TruncateForPrompt(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "Informação insuficiente.";

        var cleaned = value.Replace("\r", " ").Replace("\n", " ").Trim();
        if (cleaned.Length <= maxLength)
            return cleaned;

        return cleaned[..maxLength] + "...";
    }
}
