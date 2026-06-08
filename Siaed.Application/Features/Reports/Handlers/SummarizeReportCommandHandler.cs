using MediatR;
using Siaed.Application.Common;
using Siaed.Application.Features.Reports.Commands;
using Siaed.Application.Interfaces;
using Siaed.Application.Interfaces.Repositories;
using Siaed.Domain.Entities;
using Siaed.Domain.Enums;

namespace Siaed.Application.Features.Reports.Handlers;

public sealed class SummarizeReportCommandHandler
    : IRequestHandler<SummarizeReportCommand, Result<SummarizeReportResponseDto>>
{
    private readonly IPedagogicalReportRepository _reportRepo;
    private readonly IAIRequestRepository _aiRequestRepo;
    private readonly IOpenAIService _openAIService;
    private readonly IPromptBuilder _promptBuilder;
    private readonly IUnitOfWork _unitOfWork;

    public SummarizeReportCommandHandler(
        IPedagogicalReportRepository reportRepo,
        IAIRequestRepository aiRequestRepo,
        IOpenAIService openAIService,
        IPromptBuilder promptBuilder,
        IUnitOfWork unitOfWork)
    {
        _reportRepo = reportRepo;
        _aiRequestRepo = aiRequestRepo;
        _openAIService = openAIService;
        _promptBuilder = promptBuilder;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<SummarizeReportResponseDto>> Handle(SummarizeReportCommand request, CancellationToken ct)
    {
        var report = await _reportRepo.GetByIdAsync(request.Id, ct);
        if (report is null || report.UserId != request.RequestingUserId)
            return Result.Failure<SummarizeReportResponseDto>("Recurso não encontrado.");

        var prompt = _promptBuilder.BuildSummarizationPrompt(report.Content);

        var aiRequest = AIRequest.Create(
            request.RequestingUserId,
            AIRequestType.Summarization,
            prompt,
            report.Id.ToString(),
            "gpt-4o-mini",
            500);

        await _aiRequestRepo.AddAsync(aiRequest, ct);
        aiRequest.MarkAsProcessing();

        AICompletionResult completionResult;
        try
        {
            completionResult = await _openAIService.CompleteAsync(
                new AICompletionRequest(
                    prompt,
                    "Você é um assistente pedagógico. Gere resumos concisos em português brasileiro.",
                    500,
                    "gpt-4o-mini"),
                ct);
        }
        catch (Exception ex)
        {
            aiRequest.MarkAsFailed(ex.Message);
            _aiRequestRepo.Update(aiRequest);
            await _unitOfWork.CommitAsync(ct);
            return Result.Failure<SummarizeReportResponseDto>("Falha ao gerar resumo com IA. Tente novamente.");
        }

        aiRequest.MarkAsCompleted(completionResult.TotalTokens, completionResult.EstimatedCost);
        _aiRequestRepo.Update(aiRequest);

        var aiResponse = AIResponse.Create(aiRequest.Id, completionResult.Content, completionResult.FinishReason, completionResult.TotalTokens, "gpt-4o-mini");
        await _aiRequestRepo.AddResponseAsync(aiResponse, ct);

        report.SetSummary(completionResult.Content);
        _reportRepo.Update(report);
        await _unitOfWork.CommitAsync(ct);

        return Result.Success(new SummarizeReportResponseDto(report.Id, completionResult.Content, completionResult.TotalTokens, completionResult.EstimatedCost));
    }
}
