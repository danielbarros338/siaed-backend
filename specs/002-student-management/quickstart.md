# Quickstart: Gerenciamento de Alunos

**Fase**: 1 | **Data**: 2026-05-18 | **Feature**: 002-student-management

---

## Pré-requisitos

- .NET 10 SDK instalado
- MySQL 8 rodando localmente (ou via Docker)
- `appsettings.Development.json` com `ConnectionStrings:DefaultConnection` configurada
- JWT configurado em `appsettings.Development.json` (`Jwt:Key`, `Jwt:Issuer`, `Jwt:Audience`)

---

## 1. Criar as Entidades de Domínio

Crie os seguintes arquivos em `Siaed.Domain/`:

```
Entities/Student.cs
Entities/Turma.cs
Enums/StudentStatus.cs
Enums/TurmaStatus.cs
Enums/DocumentType.cs
Events/StudentCreatedEvent.cs
Events/StudentUpdatedEvent.cs
Events/StudentTransferredEvent.cs
Events/StudentDeactivatedEvent.cs
```

> Siga o padrão de `Teacher.cs` — factory `Create(...)`, setters `private set`, métodos de domínio explícitos. Consulte `data-model.md` para os campos e invariantes.

---

## 2. Criar as Configurações EF Core

Em `Siaed.Infra/Persistence/Configurations/`:

```csharp
// StudentConfiguration.cs
public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.FullName).IsRequired().HasMaxLength(200);
        builder.Property(s => s.DocumentId).IsRequired().HasMaxLength(50);
        builder.HasIndex(s => new { s.DocumentId, s.SchoolId }).IsUnique()
               .HasDatabaseName("UX_Students_DocumentId_SchoolId");
        builder.HasQueryFilter(s => s.DeletedAt == null);
        builder.HasOne<Turma>().WithMany().HasForeignKey(s => s.TurmaId);
    }
}
```

> Crie `TurmaConfiguration.cs` de forma similar. Mapeie o many-to-many Teacher↔Turma via:
> ```csharp
> builder.HasMany<Teacher>().WithMany()
>        .UsingEntity("TurmaTeachers");
> ```

---

## 3. Registrar DbSets no AppDbContext

Em `Siaed.Infra/Persistence/AppDbContext.cs`, adicione:

```csharp
public DbSet<Student> Students => Set<Student>();
public DbSet<Turma> Turmas => Set<Turma>();
```

---

## 4. Criar a Migration

```powershell
dotnet ef migrations add AddStudentsAndTurmas `
  --project Siaed.Infra `
  --startup-project Siaed.Api
```

Revise o arquivo de migration gerado para confirmar:
- Índice único `UX_Students_DocumentId_SchoolId`
- Tabela `TurmaTeachers` (chave composta TurmaId + TeacherId)
- Soft delete sem coluna `IsDeleted` (apenas `DeletedAt`)

Aplique ao banco:
```powershell
dotnet ef database update `
  --project Siaed.Infra `
  --startup-project Siaed.Api
```

---

## 5. Criar as Interfaces de Repositório

Em `Siaed.Application/Interfaces/`:

**`IStudentRepository.cs`**:
```csharp
public interface IStudentRepository
{
    Task<Student?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsByDocumentAsync(string documentId, Guid schoolId, CancellationToken ct = default);
    Task<PagedResult<Student>> GetPagedAsync(Guid? turmaId, StudentStatus? status, string? search, int page, int pageSize, CancellationToken ct = default);
    Task AddAsync(Student student, CancellationToken ct = default);
    void Update(Student student);
}
```

**`ITurmaRepository.cs`**:
```csharp
public interface ITurmaRepository
{
    Task<Turma?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> HasActiveStudentsAsync(Guid turmaId, CancellationToken ct = default);
    Task<PagedResult<Turma>> GetPagedAsync(TurmaStatus? status, int? schoolYear, string? search, int page, int pageSize, CancellationToken ct = default);
    Task AddAsync(Turma turma, CancellationToken ct = default);
    void Update(Turma turma);
}
```

---

## 6. Implementar os Repositórios

Em `Siaed.Infra/Repositories/`, crie `StudentRepository.cs` e `TurmaRepository.cs` seguindo o padrão de `TeacherRepository.cs` — injetar `AppDbContext`, usar `FirstOrDefaultAsync`, `AnyAsync`, etc.

---

## 7. Registrar no DI

Em `Siaed.Infra/DependencyInjection/InfraServiceExtensions.cs`, adicione:
```csharp
services.AddScoped<IStudentRepository, StudentRepository>();
services.AddScoped<ITurmaRepository, TurmaRepository>();
```

---

## 8. Implementar Commands e Queries (Application)

Em `Siaed.Application/Features/Students/` e `Features/Turmas/`, crie:

1. **Command/Query** — record com propriedades de entrada, implementando `IRequest<Result<T>>`
2. **Validator** — `AbstractValidator<Command>` com regras FluentValidation
3. **Handler** — `IRequestHandler<Command, Result<T>>`, injetando repositórios e `IUnitOfWork`

**Exemplo mínimo — CreateStudentCommand**:

```csharp
// Commands/CreateStudentCommand.cs
public record CreateStudentCommand(
    string FullName,
    DocumentType DocumentType,
    string DocumentId,
    DateOnly BirthDate,
    Guid TurmaId,
    DateOnly EnrollmentDate,
    Guid SchoolId,
    string? Notes
) : IRequest<Result<StudentDto>>;

// Validators/CreateStudentCommandValidator.cs
public class CreateStudentCommandValidator : AbstractValidator<CreateStudentCommand>
{
    public CreateStudentCommandValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.DocumentId).NotEmpty().MaximumLength(50);
        RuleFor(x => x.BirthDate).LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow));
        RuleFor(x => x.TurmaId).NotEmpty();
        RuleFor(x => x.EnrollmentDate).LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow));
    }
}

// Handlers/CreateStudentHandler.cs
public class CreateStudentHandler(
    IStudentRepository studentRepo,
    ITurmaRepository turmaRepo,
    IUnitOfWork unitOfWork
) : IRequestHandler<CreateStudentCommand, Result<StudentDto>>
{
    public async Task<Result<StudentDto>> Handle(CreateStudentCommand cmd, CancellationToken ct)
    {
        var turma = await turmaRepo.GetByIdAsync(cmd.TurmaId, ct);
        if (turma is null || turma.Status != TurmaStatus.Ativa)
            return Result<StudentDto>.Failure("A turma informada não existe ou está inativa.");

        if (await studentRepo.ExistsByDocumentAsync(cmd.DocumentId, cmd.SchoolId, ct))
            return Result<StudentDto>.Failure($"Já existe um aluno com o documento '{cmd.DocumentId}' nesta escola.");

        var student = Student.Create(
            cmd.FullName, cmd.DocumentType, cmd.DocumentId,
            cmd.BirthDate, cmd.TurmaId, cmd.EnrollmentDate, cmd.SchoolId, cmd.Notes);

        await studentRepo.AddAsync(student, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Result<StudentDto>.Success(new StudentDto(
            student.Id, student.FullName, student.DocumentType,
            student.DocumentId, student.BirthDate, student.TurmaId,
            turma.Name, student.Status, student.EnrollmentDate));
    }
}
```

---

## 9. Criar o Controller

Em `Siaed.Api/Controllers/StudentsController.cs`:

```csharp
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class StudentsController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Coordenador,Diretor")]
    public async Task<IActionResult> Create([FromBody] CreateStudentCommand cmd, CancellationToken ct)
    {
        var result = await mediator.Send(cmd, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : BadRequest(new { errors = result.Errors });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetStudentByIdQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound(new { errors = result.Errors });
    }

    // ... demais endpoints conforme contracts/api-contracts.md
}
```

---

## 10. Build e Teste

```powershell
# Build completo
dotnet build

# Executar API
dotnet run --project Siaed.Api

# Verificar endpoint (substitua TOKEN pelo JWT obtido via /api/v1/auth/login)
$headers = @{ Authorization = "Bearer $TOKEN" }
Invoke-RestMethod -Uri "http://localhost:5000/api/v1/students" -Headers $headers
```

---

## Sequência de Implementação Recomendada

1. Domain: `StudentStatus.cs`, `TurmaStatus.cs`, `DocumentType.cs`
2. Domain: `Turma.cs`, `Student.cs`
3. Domain: eventos em `Events/`
4. Infra: `TurmaConfiguration.cs`, `StudentConfiguration.cs`
5. Infra: `AppDbContext.cs` (DbSets)
6. Migration: `AddStudentsAndTurmas`
7. Application: `IStudentRepository`, `ITurmaRepository`
8. Infra: `TurmaRepository.cs`, `StudentRepository.cs`
9. DI: registros em `InfraServiceExtensions.cs`
10. Application Features: Turmas (CRUD simples primeiro)
11. Application Features: Students (Create → GetById → GetPaged → Update → Deactivate → Transfer)
12. Application Features: `ImportStudentsCsvCommand` (mais complexo — por último)
13. Api: `TurmasController.cs`, `StudentsController.cs`

---

## Checklist de Verificação Final

- [ ] Soft delete funcionando (alunos deletados não aparecem nas queries)
- [ ] Índice único `UX_Students_DocumentId_SchoolId` presente na migration
- [ ] Tabela `TurmaTeachers` criada corretamente
- [ ] Alunos inativos não podem ser transferidos (validado no domínio)
- [ ] Turmas com alunos ativos não podem ser inativadas (validado no handler)
- [ ] CSV import modo `fail-fast` reverte transação em caso de erro
- [ ] CSV import modo `partial` persiste válidos e retorna lista de erros
- [ ] `documentId` mascarado na listagem (LGPD)
- [ ] Paginação em todas as queries de lista
- [ ] Roles JWT aplicadas em todos os endpoints
