# Data Model: Módulo 1 — Completar sem Refatoração Destrutiva

**Branch**: `001-modulo1-estado-atual` | **Data**: 2025-07-17

---

## Visão Geral

Este documento descreve apenas as **mudanças** ao modelo de dados existente. As entidades já implementadas (`User`, `Teacher`, `LessonPlan`, `Activity`, `PedagogicalReport`, `AIRequest`, `AIResponse`) não são redefinidas — apenas as alterações necessárias estão documentadas.

---

## Entidades Existentes (sem alteração)

| Entidade | Status | Observação |
|---|---|---|
| `User` | Sem alteração | Autenticação OK |
| `LessonPlan` | Sem alteração | Domain methods `Publish()`, `Archive()`, `Delete()` já existem |
| `Activity` | Sem alteração | Domain methods `Publish()`, `Archive()`, `Delete()` já existem |
| `PedagogicalReport` | Sem alteração | Domain method `Delete()` já existe |
| `AIRequest` | Sem alteração | Ciclo Pending→Processing→Completed/Failed OK |
| `AIResponse` | Sem alteração | Tokens + custo OK |

---

## Mudanças na Entidade `Teacher`

### Campo adicionado: `UserId`

```csharp
// Siaed.Domain/Entities/Teacher.cs — mudança incremental

public sealed class Teacher : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Subject { get; private set; } = string.Empty;
    public string SchoolId { get; private set; } = string.Empty;

    // NOVO: Referência ao usuário de autenticação
    public Guid? UserId { get; private set; }

    // Factory method EXISTENTE — mantido sem alteração
    public static Teacher Create(string name, string email, string subject, string schoolId)
    { ... }

    // Factory method NOVO — com UserId
    public static Teacher Create(string name, string email, string subject, string schoolId, Guid userId)
    {
        var teacher = Create(name, email, subject, schoolId);
        teacher.UserId = userId;
        return teacher;
    }

    // Métodos existentes Update() e Delete() sem alteração
}
```

### Validações de domínio

| Campo | Regra |
|---|---|
| `UserId` | Nullable (Guid?). Nulo apenas para Teachers existentes antes desta feature. Nunca deve ser nulo para novos registros de `Professor`. |

---

## Mudanças no `RegisterCommand`

O command precisa receber campos adicionais para criar o `Teacher` automaticamente quando `Role == Professor`:

```csharp
// Siaed.Application/Features/Auth/Commands/RegisterCommand.cs — campos adicionados

public sealed record RegisterCommand(
    string Name,
    string Email,
    string Password,
    UserRole Role,
    // NOVOS — obrigatórios para Role == Professor, ignorados para Diretor/Coordenador
    string? Subject,    // Disciplina principal
    string? SchoolId    // Identificador da escola
) : IRequest<Result<AuthResponseDto>>;
```

---

## Interface nova: `ITeacherRepository`

Método adicionado à interface existente:

```csharp
// Siaed.Application/Interfaces/ITeacherRepository.cs — método adicionado

Task<Teacher?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
```

---

## Mudanças nas Queries/Commands (ownership)

Os Commands de escrita e Queries de leitura sensíveis recebem `RequestingUserId` para suporte ao ownership check:

### Queries de leitura (GetById)
```
GetLessonPlanByIdQuery    → adicionar: Guid RequestingUserId
GetActivityByIdQuery      → adicionar: Guid RequestingUserId
GetReportByIdQuery        → adicionar: Guid RequestingUserId
```

### Commands de escrita (Update, novos Delete/Publish/Archive)
```
UpdateLessonPlanCommand   → adicionar: Guid RequestingUserId
UpdateActivityCommand     → adicionar: Guid RequestingUserId
UpdateReportCommand       → adicionar: Guid RequestingUserId
DeleteLessonPlanCommand   → novo: inclui Guid RequestingUserId
DeleteActivityCommand     → novo: inclui Guid RequestingUserId
DeleteReportCommand       → novo: inclui Guid RequestingUserId
PublishLessonPlanCommand  → novo: inclui Guid RequestingUserId
ArchiveLessonPlanCommand  → novo: inclui Guid RequestingUserId
PublishActivityCommand    → novo: inclui Guid RequestingUserId
ArchiveActivityCommand    → novo: inclui Guid RequestingUserId
```

---

## Novos Commands e Queries

### L-001 — DELETE (soft delete)

| Command | Handler | Retorno |
|---|---|---|
| `DeleteLessonPlanCommand(Guid Id, Guid RequestingUserId)` | `DeleteLessonPlanCommandHandler` | `Result` |
| `DeleteActivityCommand(Guid Id, Guid RequestingUserId)` | `DeleteActivityCommandHandler` | `Result` |
| `DeleteReportCommand(Guid Id, Guid RequestingUserId)` | `DeleteReportCommandHandler` | `Result` |

Lógica de todos os handlers de delete:
1. Buscar por ID
2. Se não encontrado → `Result.Failure("Recurso não encontrado.")`
3. Resolver `TeacherId` do usuário logado via `ITeacherRepository.GetByUserIdAsync(RequestingUserId)`
4. Se `recurso.TeacherId != teacher.Id` → `Result.Failure("Recurso não encontrado.")` (não revelar existência)
5. Chamar `entity.Delete()` → `_repository.Update(entity)` → `_unitOfWork.CommitAsync()`

### L-002 — Transições de status

| Command | Handler | Retorno |
|---|---|---|
| `PublishLessonPlanCommand(Guid Id, Guid RequestingUserId)` | `PublishLessonPlanCommandHandler` | `Result` |
| `ArchiveLessonPlanCommand(Guid Id, Guid RequestingUserId)` | `ArchiveLessonPlanCommandHandler` | `Result` |
| `PublishActivityCommand(Guid Id, Guid RequestingUserId)` | `PublishActivityCommandHandler` | `Result` |
| `ArchiveActivityCommand(Guid Id, Guid RequestingUserId)` | `ArchiveActivityCommandHandler` | `Result` |

Lógica dos handlers (mesma estrutura de delete + chamar `entity.Publish()` ou `entity.Archive()`).

Tratamento de transição inválida: `LessonPlan.Publish()` e `Activity.Publish()` já devem lançar `InvalidOperationException` se status incompatível → capturado como `Result.Failure`.

### L-004/L-005 — Teacher Profile

| Query | Handler | Retorno |
|---|---|---|
| `GetMyTeacherProfileQuery(Guid UserId)` | `GetMyTeacherProfileQueryHandler` | `Result<TeacherDto>` |

```csharp
public sealed record TeacherDto(
    Guid Id,
    Guid? UserId,
    string Name,
    string Email,
    string Subject,
    string SchoolId,
    DateTime CreatedAt
);
```

### L-006 — IA auxiliar (Relatório)

| Command | Handler | Retorno |
|---|---|---|
| `SummarizeReportCommand(Guid ReportId, Guid RequestingUserId)` | `SummarizeReportCommandHandler` | `Result<SummarizeReportResponseDto>` |
| `GenerateParentCommunicationCommand(Guid ReportId, Guid RequestingUserId)` | `GenerateParentCommunicationCommandHandler` | `Result<ParentCommunicationResponseDto>` |

### L-008 — Histórico de IA

| Query | Handler | Retorno |
|---|---|---|
| `ListAIRequestsQuery(Guid RequestingUserId, int Page, int PageSize)` | `ListAIRequestsQueryHandler` | `Result<PagedResult<AIRequestDto>>` |

---

## Novos Validadores (L-003)

| Validator | Command |
|---|---|
| `GenerateActivityCommandValidator` | `GenerateActivityCommand` |
| `GenerateReportCommandValidator` | `GenerateReportCommand` |
| `UpdateLessonPlanCommandValidator` | `UpdateLessonPlanCommand` |
| `UpdateActivityCommandValidator` | `UpdateActivityCommand` |
| `UpdateReportCommandValidator` | `UpdateReportCommand` |

---

## Migration: `AddUserIdAndIndexes`

```sql
-- Coluna nova em Teacher
ALTER TABLE Teachers ADD COLUMN UserId CHAR(36) NULL;
ALTER TABLE Teachers ADD CONSTRAINT FK_Teachers_Users_UserId 
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE SET NULL;
CREATE INDEX IX_Teachers_UserId ON Teachers(UserId);

-- Índices novos para filtros avançados (L-007)
CREATE INDEX IX_LessonPlans_Status ON LessonPlans(Status);
CREATE INDEX IX_LessonPlans_IsAIGenerated ON LessonPlans(IsAIGenerated);
CREATE INDEX IX_Activities_Status ON Activities(Status);
CREATE INDEX IX_Activities_IsAIGenerated ON Activities(IsAIGenerated);
CREATE INDEX IX_PedagogicalReports_IsAIGenerated ON PedagogicalReports(IsAIGenerated);
```

> **Nota**: A migration é gerada via EF Core (`dotnet ef migrations add AddUserIdAndIndexes`). O SQL acima é ilustrativo.

---

## Diagrama de relacionamentos (delta)

```
Users ─────── (UserId) ──────> Teachers
                                   │
              ┌────────────────────┼────────────────────┐
              ▼                    ▼                     ▼
         LessonPlans           Activities         PedagogicalReports
              │                    │                     │
              └────────── AIRequests ──── AIResponses ───┘
```

Nenhuma relação nova de banco além de `Teacher.UserId → User.Id`.

---

## Resumo das mudanças por camada

| Camada | Arquivo | Tipo de mudança |
|---|---|---|
| `Siaed.Domain` | `Teacher.cs` | Adicionar `UserId` + factory overload |
| `Siaed.Application` | `ITeacherRepository.cs` | Adicionar `GetByUserIdAsync` |
| `Siaed.Application` | `RegisterCommand.cs` | Adicionar `Subject?`, `SchoolId?` |
| `Siaed.Application` | `RegisterCommandHandler.cs` | Criar `Teacher` se Role == Professor |
| `Siaed.Application` | `GetLessonPlanByIdQuery.cs` | Adicionar `RequestingUserId` |
| `Siaed.Application` | `GetActivityByIdQuery.cs` | Adicionar `RequestingUserId` |
| `Siaed.Application` | `GetReportByIdQuery.cs` | Adicionar `RequestingUserId` |
| `Siaed.Application` | `UpdateLessonPlanCommand.cs` | Adicionar `RequestingUserId` |
| `Siaed.Application` | `UpdateActivityCommand.cs` | Adicionar `RequestingUserId` |
| `Siaed.Application` | `UpdateReportCommand.cs` | Adicionar `RequestingUserId` |
| `Siaed.Application` | Handlers de GetById e Update | Adicionar ownership check |
| `Siaed.Application` | 11 novos Commands | DELETE, Publish, Archive, Teacher/Me, IA aux, Histórico |
| `Siaed.Application` | 11 novos Handlers | Correspondentes aos Commands acima |
| `Siaed.Application` | 5 novos Validators | L-003 |
| `Siaed.Infra` | `TeacherRepository.cs` | Implementar `GetByUserIdAsync` |
| `Siaed.Infra` | `TeacherConfiguration.cs` | Mapear `UserId` + FK |
| `Siaed.Infra` | `Migrations/` | Nova migration `AddUserIdAndIndexes` |
| `Siaed.Api` | `AuthController.cs` | Atualizar RegisterRequest DTO |
| `Siaed.Api` | `LessonPlansController.cs` | Adicionar DELETE, PATCH publish/archive + extrair userId do JWT |
| `Siaed.Api` | `ActivitiesController.cs` | Adicionar DELETE, PATCH publish/archive + extrair userId do JWT |
| `Siaed.Api` | `ReportsController.cs` | Adicionar DELETE, POST summarize, POST parent-communication + extrair userId do JWT |
| `Siaed.Api` | `TeacherController.cs` | Novo controller com GET /me |
