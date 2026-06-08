using MediatR;
using Siaed.Application.Common;
using Siaed.Application.Features.Reports.Commands;
using Siaed.Application.Interfaces;
using Siaed.Application.Interfaces.Repositories;
using Siaed.Domain.Entities;
using Siaed.Domain.Enums;

namespace Siaed.Application.Features.Reports.Handlers;

public sealed class GenerateParentCommunicationCommandHandler
    : IRequestHandler<GenerateParentCommunicationCommand, Result<ParentCommunicationResponseDto>>
{
    private readonly IPedagogicalReportRepository _reportRepo;
    private readonly IStudentRepository _studentRepository;
    private readonly IAIRequestRepository _aiRequestRepo;
    private readonly IOpenAIService _openAIService;
    private readonly IPromptBuilder _promptBuilder;
    private readonly IUnitOfWork _unitOfWork;

    public GenerateParentCommunicationCommandHandler(
        IPedagogicalReportRepository reportRepo,
        IStudentRepository studentRepository,
        IAIRequestRepository aiRequestRepo,
        IOpenAIService openAIService,
        IPromptBuilder promptBuilder,
        IUnitOfWork unitOfWork)
    {
        _reportRepo = reportRepo;
        _studentRepository = studentRepository;
        _aiRequestRepo = aiRequestRepo;
        _openAIService = openAIService;
        _promptBuilder = promptBuilder;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ParentCommunicationResponseDto>> Handle(GenerateParentCommunicationCommand request, CancellationToken ct)
    {
        var report = await _reportRepo.GetByIdAsync(request.Id, ct);
        if (report is null || report.UserId != request.RequestingUserId)
            return Result.Failure<ParentCommunicationResponseDto>("Recurso não encontrado.");

        var student = await _studentRepository.GetByIdAsync(report.StudentId, ct);
        if (student is null)
            return Result.Failure<ParentCommunicationResponseDto>("Aluno não encontrado para este relatório.");

        var prompt = _promptBuilder.BuildParentCommunicationPrompt(report.Content, student.FullName);

        var aiRequest = AIRequest.Create(
            request.RequestingUserId,
            AIRequestType.ParentCommunication,
            prompt,
            report.Id.ToString(),
            "gpt-4o-mini",
            800);

        await _aiRequestRepo.AddAsync(aiRequest, ct);
        aiRequest.MarkAsProcessing();

        AICompletionResult completionResult;
        try
        {
            completionResult = await _openAIService.CompleteAsync(
                new AICompletionRequest(
                    prompt,
                    "Você é um assistente pedagógico. Gere comunicados para pais em português brasileiro com linguagem acessível, empática e respeitosa.",
                    800,
                    "gpt-4o-mini"),
                ct);
        }
        catch (Exception ex)
        {
            aiRequest.MarkAsFailed(ex.Message);
            _aiRequestRepo.Update(aiRequest);
            await _unitOfWork.CommitAsync(ct);
            return Result.Failure<ParentCommunicationResponseDto>("Falha ao gerar comunicado com IA. Tente novamente.");
        }

        aiRequest.MarkAsCompleted(completionResult.TotalTokens, completionResult.EstimatedCost);
        _aiRequestRepo.Update(aiRequest);

        var aiResponse = AIResponse.Create(aiRequest.Id, completionResult.Content, completionResult.FinishReason, completionResult.TotalTokens, "gpt-4o-mini");
        await _aiRequestRepo.AddResponseAsync(aiResponse, ct);

        report.SetParentCommunication(completionResult.Content);
        _reportRepo.Update(report);
        await _unitOfWork.CommitAsync(ct);

        return Result.Success(new ParentCommunicationResponseDto(report.Id, completionResult.Content, completionResult.TotalTokens, completionResult.EstimatedCost));
    }
}
