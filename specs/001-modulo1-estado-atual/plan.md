# Implementation Plan: Módulo 1 — Completar sem Refatoração Destrutiva

**Branch**: `001-modulo1-estado-atual` | **Date**: 2025-07-17 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/001-modulo1-estado-atual/spec.md`

---

## Summary

O Módulo 1 (Assistente de IA para Professores) está aproximadamente 70% implementado. A arquitetura (Clean Architecture + DDD + CQRS + MediatR) está correta e não será alterada. O objetivo é completar as lacunas identificadas no `spec.md` (L-001 a L-010) e corrigir duas divergências arquiteturais críticas (D-001: User↔Teacher desconectados; D-005: violação LGPD por acesso cross-professor).

A abordagem é incremental: nenhuma reestruturação de solution, Auth existente preservada, todas as mudanças adicionam comportamento sem remover o que já funciona.

Pesquisa completa: [research.md](research.md)  
Modelo de dados: [data-model.md](data-model.md)  
Contratos da API: [contracts/api-contracts.md](contracts/api-contracts.md)  
Guia de implementação: [quickstart.md](quickstart.md)

---

## Technical Context

**Language/Version**: C# 13, .NET 10

**Primary Dependencies**: MediatR 12, FluentValidation 11, OpenAI .NET SDK, EF Core 9 + Pomelo.EntityFrameworkCore.MySql, Serilog, JWT Bearer (Microsoft.AspNetCore.Authentication.JwtBearer)

**Storage**: MySQL (EF Core Migrations). 1 migration existente (`20260515192120_AddUser`). Nova migration necessária: `AddUserIdAndIndexes`.

**Testing**: Sem projeto de testes no momento. Fora do escopo desta feature.

**Target Platform**: Servidor Linux/Windows — ASP.NET Core Web API

**Project Type**: Web API service (backend-only)

**Performance Goals**: `<500ms p95` para endpoints IA-backed; `<100ms p95` para CRUD simples

**Constraints**:
- Auth existente preservada (sem alterar contrato de `/api/v1/auth/login`)
- Nenhuma reestruturação da solution (4 projetos existentes mantidos)
- Sem DELETE físico em entidades de domínio
- Dados pessoais de alunos sanitizados antes de envio à IA
- IA responde sempre em português brasileiro

**Scale/Scope**: Módulo 1 inicial — 100–1.000 professores, 10–50 escolas

---

## Constitution Check

*GATE: Avaliado antes e após o design.*

### Princípio I — Clean Architecture + DDD ✅

Todas as mudanças planejadas respeitam as camadas:
- Novos Commands/Queries/Handlers ficam em `Application/Features/`
- `Teacher.UserId` é campo simples sem dependência de framework
- Repositórios e DI ficam na Infra
- Controllers extraem `userId` do JWT (dado de identidade) e delegam ao MediatR

### Princípio II — CQRS com MediatR ✅

- Todos os 11 novos endpoints têm Commands ou Queries correspondentes
- Handlers são os únicos responsáveis por validação de negócio (ownership check)
- Nenhuma regra de negócio em controllers

### Princípio III — Result Pattern ✅

- Todos os novos Handlers retornam `Result` ou `Result<T>`
- Erros de ownership retornam `Result.Failure(...)` — não exceções
- `ExceptionHandlingMiddleware` existente cobre falhas inesperadas

### Princípio IV — Entidades Ricas ✅

- `Teacher.Create(... userId)` é uma sobrecarga do factory method existente
- `LessonPlan.Delete()`, `Publish()`, `Archive()` já existem no domínio — handlers apenas os invocam
- Nenhum modelo anêmico introduzido

### Princípio V — Segurança e Observabilidade ⚠️ → RESOLVIDO pelo plano

**D-005 (LGPD crítico)**: Atualmente qualquer professor autenticado pode ver/editar recursos de outros professores. **Este plano corrige isso** adicionando ownership check em todos os handlers GetById e Update, e em todos os novos handlers.

**D-001**: `User` e `Teacher` não estão conectados. **Este plano corrige isso** adicionando `Teacher.UserId` e criação automática no `RegisterCommandHandler`.

**Avaliação pós-design**: Após implementação, Princípio V estará completo. Sem violações residuais.

---

## Project Structure

### Documentation (this feature)

```text
specs/001-modulo1-estado-atual/
├── plan.md              ← Este arquivo
├── research.md          ← Phase 0: todas as incertezas resolvidas
├── data-model.md        ← Phase 1: mudanças incrementais no modelo
├── quickstart.md        ← Phase 1: guia para implementação
├── contracts/
│   └── api-contracts.md ← Phase 1: novos e modificados endpoints
└── tasks.md             ← Phase 2: gerado por /speckit.tasks (pendente)
```

### Source Code (changes only)

```text
Siaed.Domain/
  Entities/
    Teacher.cs                      ← + UserId field + factory overload

Siaed.Application/
  Interfaces/
    ITeacherRepository.cs           ← + GetByUserIdAsync()
  Features/
    Auth/
      Commands/
        RegisterCommand.cs          ← + Subject?, SchoolId?
      Handlers/
        RegisterCommandHandler.cs   ← + criar Teacher se Role == Professor
    LessonPlans/
      Commands/
        DeleteLessonPlanCommand.cs  ← NOVO
        PublishLessonPlanCommand.cs ← NOVO
        ArchiveLessonPlanCommand.cs ← NOVO
      Queries/
        GetLessonPlanByIdQuery.cs   ← + RequestingUserId
      Handlers/
        DeleteLessonPlanCommandHandler.cs   ← NOVO
        PublishLessonPlanCommandHandler.cs  ← NOVO
        ArchiveLessonPlanCommandHandler.cs  ← NOVO
        GetLessonPlanByIdQueryHandler.cs    ← + ownership check
        UpdateLessonPlanCommandHandler.cs   ← + ownership check
      Validators/
        UpdateLessonPlanCommandValidator.cs ← NOVO
    Activities/
      [mesma estrutura espelhada do LessonPlans]
    Reports/
      Commands/
        DeleteReportCommand.cs               ← NOVO
        SummarizeReportCommand.cs            ← NOVO
        GenerateParentCommunicationCommand.cs← NOVO
      Handlers/
        [handlers correspondentes]           ← NOVOS
      Validators/
        GenerateReportCommandValidator.cs    ← NOVO
        UpdateReportCommandValidator.cs      ← NOVO
    Teachers/
      Queries/
        GetMyTeacherProfileQuery.cs          ← NOVO
      Handlers/
        GetMyTeacherProfileQueryHandler.cs   ← NOVO
      DTOs/
        TeacherDto.cs                        ← NOVO
    AI/
      Queries/
        ListAIRequestsQuery.cs               ← NOVO
      Handlers/
        ListAIRequestsQueryHandler.cs        ← NOVO
      DTOs/
        AIRequestDto.cs                      ← NOVO

Siaed.Infra/
  Persistence/
    TeacherConfiguration.cs        ← + UserId mapping + FK + index
  Repositories/
    TeacherRepository.cs           ← + GetByUserIdAsync()
  Migrations/
    [timestamp]_AddUserIdAndIndexes.cs ← NOVA

Siaed.Api/
  Controllers/
    LessonPlansController.cs       ← + DELETE, PATCH publish/archive
    ActivitiesController.cs        ← + DELETE, PATCH publish/archive
    ReportsController.cs           ← + DELETE, POST summarize, POST parent-communication
    TeachersController.cs          ← NOVO: GET /me
    AIController.cs                ← NOVO: GET /requests
  [Modificados: extrair userId do JWT em GetById e Update existentes]
```

**Structure Decision**: Solution em 4 projetos existentes. Nenhum projeto novo. Todos os novos artefatos seguem os padrões de pasta já estabelecidos.

---

## Backlog priorizado

| # | ID | Descrição | Prioridade | Dependência |
|---|---|---|---|---|
| 1 | D-001 | Adicionar `Teacher.UserId`, migration, `RegisterCommandHandler` cria Teacher | 🔴 Crítica | — |
| 2 | D-005 | Ownership check em GetById e Update handlers (3 entidades × 2 = 6 handlers) | 🔴 Crítica | D-001 |
| 3 | L-001 | DELETE endpoints para LessonPlan, Activity, Report | 🟠 Alta | D-001 |
| 4 | L-004 | `TeachersController` + `GetMyTeacherProfileQuery` | 🟠 Alta | D-001 |
| 5 | L-003 | 5 validators ausentes (Generate × 2, Update × 3) | 🟡 Média | — |
| 6 | L-002 | Publish/Archive: LessonPlan e Activity (4 endpoints) | 🟡 Média | D-001 |
| 7 | L-006 | IA auxiliar: Summarize e ParentCommunication para Reports | 🟡 Média | — |
| 8 | L-007 | Filtros avançados nas queries de listagem (Status, IsAIGenerated) | 🟢 Baixa | — |
| 9 | L-008 | Histórico de IA: `GET /api/v1/ai/requests` | 🟢 Baixa | — |

> **L-009 encerrada**: Soft delete filters já aplicados em todas as entidades (confirmado via análise do código).  
> **L-010 (formato IA)**: Prioridade decidida como técnica, endereçável após os itens acima.

---

## Complexity Tracking

> Sem violações constitucionais. Nenhuma justificativa necessária.
