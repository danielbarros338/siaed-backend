# Quickstart: Como implementar as lacunas do Módulo 1

**Branch**: `001-modulo1-estado-atual` | **Data**: 2025-07-17

> Este guia é voltado para o desenvolvedor (ou agente de IA) que vai executar as tarefas geradas por `/speckit.tasks`. Explica o padrão já estabelecido no projeto e como estender cada peça sem quebrar nada.

---

## Pré-requisitos

```powershell
# Restaurar dependências
dotnet restore

# Verificar build limpo
dotnet build

# Verificar que os testes (quando existirem) passam
# dotnet test  ← ainda sem projeto de testes
```

---

## Padrão de feature (referência)

Cada feature do Módulo 1 segue esta estrutura em `Siaed.Application/Features/<Feature>/`:

```
Features/
  LessonPlans/
    Commands/
      CreateLessonPlanCommand.cs        ← record com campos + IRequest<Result>
      DeleteLessonPlanCommand.cs        ← NOVO: seguir mesmo padrão
    Queries/
      GetLessonPlanByIdQuery.cs         ← record com Id + IRequest<Result<Dto>>
    Handlers/
      CreateLessonPlanCommandHandler.cs ← sealed class IRequestHandler<TCommand, TResult>
      DeleteLessonPlanCommandHandler.cs ← NOVO: seguir mesmo padrão
    Validators/
      CreateLessonPlanCommandValidator.cs ← AbstractValidator<TCommand>
      UpdateLessonPlanCommandValidator.cs ← NOVO: seguir mesmo padrão
    DTOs/
      LessonPlanDto.cs                  ← record com todos os campos de leitura
```

---

## Como implementar um novo Command (exemplo: DeleteLessonPlanCommand)

### 1. Command (Application/Features/LessonPlans/Commands/)

```csharp
using MediatR;
using Siaed.Application.Common;

namespace Siaed.Application.Features.LessonPlans.Commands;

public sealed record DeleteLessonPlanCommand(
    Guid Id,
    Guid RequestingUserId
) : IRequest<Result>;
```

### 2. Validator (Application/Features/LessonPlans/Validators/)

```csharp
using FluentValidation;

namespace Siaed.Application.Features.LessonPlans.Validators;

public sealed class DeleteLessonPlanCommandValidator : AbstractValidator<DeleteLessonPlanCommand>
{
    public DeleteLessonPlanCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.RequestingUserId).NotEmpty();
    }
}
```

### 3. Handler (Application/Features/LessonPlans/Handlers/)

```csharp
using MediatR;
using Siaed.Application.Common;
using Siaed.Application.Features.LessonPlans.Commands;
using Siaed.Application.Interfaces;

namespace Siaed.Application.Features.LessonPlans.Handlers;

public sealed class DeleteLessonPlanCommandHandler
    : IRequestHandler<DeleteLessonPlanCommand, Result>
{
    private readonly ILessonPlanRepository _lessonPlanRepository;
    private readonly ITeacherRepository _teacherRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteLessonPlanCommandHandler(
        ILessonPlanRepository lessonPlanRepository,
        ITeacherRepository teacherRepository,
        IUnitOfWork unitOfWork)
    {
        _lessonPlanRepository = lessonPlanRepository;
        _teacherRepository = teacherRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteLessonPlanCommand request, CancellationToken ct)
    {
        var lessonPlan = await _lessonPlanRepository.GetByIdAsync(request.Id, ct);
        if (lessonPlan is null)
            return Result.Failure("Plano de aula não encontrado.");

        // Ownership check (D-005 fix)
        var teacher = await _teacherRepository.GetByUserIdAsync(request.RequestingUserId, ct);
        if (teacher is null || lessonPlan.TeacherId != teacher.Id)
            return Result.Failure("Plano de aula não encontrado.");

        lessonPlan.Delete();
        _lessonPlanRepository.Update(lessonPlan);
        await _unitOfWork.CommitAsync(ct);

        return Result.Success();
    }
}
```

### 4. Controller (Api/Controllers/LessonPlansController.cs)

```csharp
[HttpDelete("{id:guid}")]
public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
{
    var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    var result = await _mediator.Send(new DeleteLessonPlanCommand(id, userId), ct);
    return result.IsSuccess ? NoContent() : NotFound(new { result.Error });
}
```

---

## Como adicionar ownership check em handler existente (D-005 fix)

Exemplo para `GetLessonPlanByIdQueryHandler`:

### 1. Atualizar Query para receber `RequestingUserId`

```csharp
public sealed record GetLessonPlanByIdQuery(
    Guid Id,
    Guid RequestingUserId  // ← NOVO
) : IRequest<Result<LessonPlanDto>>;
```

### 2. Atualizar Handler

```csharp
// Após buscar o lessonPlan existente:
var teacher = await _teacherRepository.GetByUserIdAsync(request.RequestingUserId, ct);
if (teacher is null || lessonPlan.TeacherId != teacher.Id)
    return Result.Failure<LessonPlanDto>("Plano de aula não encontrado.");
```

### 3. Atualizar Controller

```csharp
[HttpGet("{id:guid}")]
public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
{
    var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    var result = await _mediator.Send(new GetLessonPlanByIdQuery(id, userId), ct);
    return result.IsSuccess ? Ok(result.Value) : NotFound(new { result.Error });
}
```

---

## Como adicionar `UserId` ao Teacher (D-001 fix)

### 1. Modificar entidade `Teacher.cs` (Domain)

```csharp
// Adicionar campo
public Guid? UserId { get; private set; }

// Adicionar sobrecarga de factory method
public static Teacher Create(string name, string email, string subject, string schoolId, Guid userId)
{
    var teacher = Create(name, email, subject, schoolId);
    teacher.UserId = userId;
    return teacher;
}
```

### 2. Atualizar `TeacherConfiguration.cs` (Infra)

```csharp
builder.Property(t => t.UserId).IsRequired(false);
builder.HasIndex(t => t.UserId);
// FK opcional: builder.HasOne<User>().WithOne().HasForeignKey<Teacher>(t => t.UserId).IsRequired(false);
```

### 3. Atualizar `ITeacherRepository.cs` (Application)

```csharp
Task<Teacher?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
```

### 4. Implementar em `TeacherRepository.cs` (Infra)

```csharp
public async Task<Teacher?> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    => await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == userId, ct);
```

### 5. Atualizar `RegisterCommandHandler.cs` (Application)

```csharp
// Após criar e salvar o User:
if (request.Role == UserRole.Professor && !string.IsNullOrWhiteSpace(request.Subject))
{
    var teacher = Teacher.Create(request.Name, request.Email, request.Subject!, request.SchoolId!, user.Id);
    await _teacherRepository.AddAsync(teacher, ct);
}
await _unitOfWork.CommitAsync(ct);
```

### 6. Criar migration

```powershell
dotnet ef migrations add AddUserIdAndIndexes --project Siaed.Infra --startup-project Siaed.Api
dotnet ef database update --project Siaed.Infra --startup-project Siaed.Api
```

---

## Como criar um Validator (L-003 exemplo)

```csharp
// Application/Features/LessonPlans/Validators/UpdateLessonPlanCommandValidator.cs
using FluentValidation;

namespace Siaed.Application.Features.LessonPlans.Validators;

public sealed class UpdateLessonPlanCommandValidator : AbstractValidator<UpdateLessonPlanCommand>
{
    public UpdateLessonPlanCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.RequestingUserId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        // ... demais campos obrigatórios
    }
}
```

> Os validators são registrados automaticamente pelo `ValidationBehavior` — basta criar a classe.

---

## Extraindo `userId` do JWT no controller

Padrão já usado em toda a API:

```csharp
using System.Security.Claims;

// Dentro de qualquer action method:
var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
```

---

## Como verificar o build após cada mudança

```powershell
dotnet build --no-restore 2>&1 | Select-String "error|warning" | Select-Object -First 20
```

---

## Ordem de execução das tarefas (conforme research.md)

```
1. [Segurança-Crítico] D-001: Teacher.UserId + migration
2. [Segurança-Crítico] D-005: Ownership check em handlers existentes
3. [Alta] L-001: DELETE endpoints (3 entidades)
4. [Alta] L-004: TeacherController + GetMyTeacherProfileQuery
5. [Média] L-003: 5 validators ausentes
6. [Média] L-002: Publish/Archive transitions (4 endpoints)
7. [Média] L-006: IA auxiliar (Summarize + ParentCommunication)
8. [Baixa] L-007: Filtros avançados nas queries de listagem
9. [Baixa] L-008: Histórico de IA (GET /ai/requests)
```
