namespace Siaed.Application.Interfaces;

public interface IPromptBuilder
{
    string BuildLessonPlanPrompt(LessonPlanPromptContext context);
    string BuildActivityPrompt(ActivityPromptContext context);
    string BuildReportPrompt(ReportPromptContext context);
    string BuildSummarizationPrompt(string text);
    string BuildTextReformulationPrompt(string text, string targetAgeRange);
    string BuildParentCommunicationPrompt(string reportContent, string studentName);
}

public sealed record LessonPlanPromptContext(
    string Subject,
    string Grade,
    string AgeRange,
    int DurationMinutes,
    string AdditionalInstructions = "");

public sealed record ActivityPromptContext(
    string Subject,
    string Grade,
    string AgeRange,
    string ActivityType,
    int NumberOfQuestions = 10,
    string AdditionalInstructions = "",
    LessonPlanData? LessonPlan = null);

public sealed record LessonPlanData(
    string Title,
    string Objectives,
    string Content,
    string Methodology,
    string Resources,
    string Evaluation);

public sealed record ReportPromptContext(
    Guid StudentId,
    string StudentName = "",
    string StudentNotes = "",
    IReadOnlyList<string>? ActivityPerformances = null,
    IReadOnlyList<string>? PreviousReports = null,
    string AdditionalInstructions = "");
