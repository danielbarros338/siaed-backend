using Siaed.Application.Interfaces;
using System.Linq;

namespace Siaed.Infra.OpenAI;

public sealed class PromptBuilderService : IPromptBuilder
{
    public string BuildLessonPlanPrompt(LessonPlanPromptContext context)
    {
        var instrucoes = string.IsNullOrWhiteSpace(context.AdditionalInstructions)
            ? string.Empty
            : $"\nInstruções adicionais do professor: {context.AdditionalInstructions}";

        var minIntro = Math.Max(5, context.DurationMinutes / 6);
        var minDesenv = context.DurationMinutes / 2;
        var minPratica = context.DurationMinutes / 4;
        var minEncerr = Math.Max(5, context.DurationMinutes / 8);

        return $$"""
            Você é um assistente pedagógico especializado. Crie um plano de aula completo para as especificações abaixo.

            ESPECIFICAÇÕES:
            - Disciplina: {{context.Subject}}
            - Série/Turma: {{context.Grade}}
            - Faixa etária dos alunos: {{context.AgeRange}}
            - Duração da aula: {{context.DurationMinutes}} minutos{{instrucoes}}

            INSTRUÇÕES DE RESPOSTA:
            Responda EXCLUSIVAMENTE com um objeto JSON válido, sem markdown, sem blocos de código, sem texto antes ou depois.
            Use apenas aspas duplas. Todos os campos são obrigatórios.

            Formato exato esperado:
            {
              "title": "título objetivo do plano de aula mencionando disciplina e tema principal",
              "objectives": "objetivos de aprendizagem claros e mensuráveis, separados por ponto e vírgula",
              "content": "estrutura detalhada da aula em 4 momentos — Introdução ({{minIntro}} min): contexto e motivação; Desenvolvimento ({{minDesenv}} min): conteúdo programático com exemplos e explicações; Prática ({{minPratica}} min): atividades dos alunos; Encerramento ({{minEncerr}} min): síntese e dúvidas",
              "methodology": "metodologias de ensino utilizadas, adequadas à faixa etária de {{context.AgeRange}}, com descrição de como serão aplicadas",
              "resources": "lista de materiais e recursos didáticos necessários para a aula",
              "evaluation": "estratégias e instrumentos de avaliação com critérios objetivos"
            }
            """;
    }

    public string BuildActivityPrompt(ActivityPromptContext context)
    {
        var instrucoes = string.IsNullOrWhiteSpace(context.AdditionalInstructions)
            ? string.Empty
            : $"\nInstruções adicionais do professor: {context.AdditionalInstructions}";

        var contextoDoPlan = context.LessonPlan is null
            ? string.Empty
            : BuildLessonPlanContextForActivity(context.LessonPlan);

        return $$"""
            Você é um assistente pedagógico especializado. Crie uma atividade pedagógica completa com as especificações abaixo.

            ESPECIFICAÇÕES:
            - Disciplina: {{context.Subject}}
            - Série/Turma: {{context.Grade}}
            - Faixa etária: {{context.AgeRange}}
            - Tipo de atividade: {{context.ActivityType}}
            - Número de questões/exercícios: {{context.NumberOfQuestions}}{{instrucoes}}
            {{contextoDoPlan}}

            INSTRUÇÕES DE RESPOSTA:
            Responda EXCLUSIVAMENTE com um objeto JSON válido, sem markdown, sem blocos de código, sem texto antes ou depois.
            Use apenas aspas duplas. Todos os campos são obrigatórios.

            Formato exato esperado:
            {
              "title": "título objetivo da atividade mencionando disciplina e tema principal",
              "description": "descrição clara do que será feito na atividade, com contexto pedagógico",
              "content": "corpo completo da atividade com {{context.NumberOfQuestions}} questões/exercícios claros, numerados, e adequados à faixa etária de {{context.AgeRange}}, com instruções passo a passo",
              "answerKey": "gabarito detalhado com respostas esperadas, explicações e critérios de correção para cada questão",
              "simplifiedVersion": "versão simplificada da atividade para alunos com dificuldades de aprendizagem, mantendo os objetivos pedagógicos"
            }
            """;
    }

    private static string BuildLessonPlanContextForActivity(LessonPlanData lessonPlan)
    {
        return $$"""

            CONTEXTO DO PLANO DE AULA ASSOCIADO:
            - Título do plano: {{lessonPlan.Title}}
            - Objetivos de aprendizagem: {{lessonPlan.Objectives}}
            - Conteúdo/estrutura: {{lessonPlan.Content}}
            - Metodologia: {{lessonPlan.Methodology}}
            - Recursos disponíveis: {{lessonPlan.Resources}}
            - Critérios de avaliação: {{lessonPlan.Evaluation}}

            INSTRUÇÕES ESPECIAIS:
            A atividade deve estar PERFEITAMENTE ALINHADA com o plano de aula acima.
            Utilize os mesmos objetivos de aprendizagem, metodologia e recursos mencionados no plano.
            A atividade deve ser uma extensão coerente do plano de aula, não uma atividade genérica e isolada.
            """;
    }

    public string BuildReportPrompt(ReportPromptContext context)
    {
        var instrucoes = string.IsNullOrWhiteSpace(context.AdditionalInstructions)
            ? string.Empty
            : $"\n- Instruções adicionais do professor: {context.AdditionalInstructions}";

        var studentName = string.IsNullOrWhiteSpace(context.StudentName)
            ? "Não informado"
            : context.StudentName;

        var studentNotes = string.IsNullOrWhiteSpace(context.StudentNotes)
            ? "Sem observações cadastrais."
            : context.StudentNotes;

        var activityPerformances = context.ActivityPerformances is { Count: > 0 }
            ? string.Join("\n", context.ActivityPerformances.Select((item, i) => $"{i + 1}. {item}"))
            : "Nenhuma atividade com nota disponível.";

        var previousReports = context.PreviousReports is { Count: > 0 }
            ? string.Join("\n", context.PreviousReports.Select((r, i) => $"{i + 1}. {r}"))
            : "Nenhum relatório anterior disponível.";

        return $$"""
            Você é um assistente pedagógico especializado. Gere um relatório pedagógico com viés comportamental para o aluno informado.

            DADOS DISPONÍVEIS (FONTE ÚNICA):
            - Identificador do aluno: {{context.StudentId}}
            - Nome do aluno: {{studentName}}
            - Observações cadastrais do aluno: {{studentNotes}}{{instrucoes}}

            ATIVIDADES E NOTAS DO ALUNO:
            {{activityPerformances}}

            RESUMOS DE RELATÓRIOS ANTERIORES DO MESMO ALUNO:
            {{previousReports}}

            REGRAS DE CONFIABILIDADE (OBRIGATÓRIAS):
            - Use SOMENTE informações presentes em "DADOS DISPONÍVEIS", "ATIVIDADES E NOTAS DO ALUNO" e "RESUMOS DE RELATÓRIOS ANTERIORES DO MESMO ALUNO".
            - NÃO invente fatos, datas, comportamentos, contextos, falas, intervenções ou resultados.
            - Se não houver evidência suficiente para algum ponto, escreva explicitamente "Informação insuficiente".
            - Não emita diagnósticos ou laudos médicos.
            - Não use linguagem absoluta quando não houver base (evite "sempre", "nunca" sem evidência).

            INSTRUÇÕES DE RESPOSTA:
            Responda EXCLUSIVAMENTE com um objeto JSON válido, sem markdown, sem blocos de código, sem texto antes ou depois.
            Use apenas aspas duplas. Todos os campos são obrigatórios.

            Formato exato esperado:
            {
              "content": "relatório pedagógico completo, em linguagem clara, empática e profissional, contendo apenas inferências sustentadas pelos dados fornecidos",
              "summary": "resumo objetivo do relatório em 4 a 6 frases, sem adicionar novas informações",
              "parentCommunication": "comunicação acolhedora aos responsáveis, com linguagem simples, baseada estritamente nos dados disponíveis e indicando quando houver informação insuficiente"
            }
            """;
    }

    public string BuildSummarizationPrompt(string text)
    {
        return $"""
            Faça um resumo pedagógico claro e objetivo do seguinte texto:

            {text}

            O resumo deve:
            - Ser conciso e objetivo
            - Preservar as informações mais importantes
            - Usar linguagem clara e acessível
            - Estar em português brasileiro
            """;
    }

    public string BuildTextReformulationPrompt(string text, string targetAgeRange)
    {
        return $"""
            Reformule o seguinte texto para adequá-lo à faixa etária: {targetAgeRange}

            Texto original:
            {text}

            A reformulação deve:
            - Adaptar o vocabulário para a faixa etária indicada
            - Manter o conteúdo e as informações essenciais
            - Tornar o texto mais acessível e motivador para a faixa etária
            - Estar em português brasileiro
            """;
    }

    public string BuildParentCommunicationPrompt(string reportContent, string studentName)
    {
        return $"""
            Com base no seguinte relatório pedagógico do aluno {studentName}, crie uma comunicação clara e acolhedora para os responsáveis:

            Relatório:
            {reportContent}

            A comunicação deve:
            - Usar linguagem simples e acessível (não técnica)
            - Ser empática e construtiva
            - Destacar pontos positivos
            - Apresentar sugestões práticas para apoio em casa
            - Ter tom profissional mas acolhedor
            - Estar em português brasileiro

            IMPORTANTE: Não inclua diagnósticos ou laudos médicos.
            """;
    }
}
