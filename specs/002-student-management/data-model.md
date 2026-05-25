# Data Model: Gerenciamento de Alunos

**Phase**: 1 | **Date**: 2026-05-18 | **Feature**: 002-student-management

---

## Entidades de Domínio

### Student (Aluno)

Herda de `BaseEntity` (`Id`, `CreatedAt`, `UpdatedAt`, `DeletedAt?`).

| Campo | Tipo | Obrigatório | Regras |
|-------|------|-------------|--------|
| `FullName` | `string` | ✅ | Máx. 200 chars |
| `DocumentType` | `DocumentType` (enum) | ✅ | CPF, RegistroEstrangeiro, IdInterno |
| `DocumentId` | `string` | ✅ | Máx. 50 chars; único por escola (índice único no DB) |
| `BirthDate` | `DateOnly` | ✅ | Não pode ser futura; aluno deve ter ≤ 25 anos |
| `TurmaId` | `Guid` | ✅ | FK → Turma; Turma deve estar ativa |
| `Status` | `StudentStatus` (enum) | ✅ | Padrão: `Ativo` na criação |
| `EnrollmentDate` | `DateOnly` | ✅ | Data de matrícula; padrão: data atual |
| `SchoolId` | `Guid` | ✅ | Isolamento multi-tenant por escola |
| `Notes` | `string?` | ❌ | Máx. 2000 chars; observações pedagógicas gerais |

**Factory / Métodos de domínio**:

```csharp
// Criação — invariantes validados no factory method
public static Student Create(
    string fullName,
    DocumentType documentType,
    string documentId,
    DateOnly birthDate,
    Guid turmaId,
    DateOnly enrollmentDate,
    Guid schoolId,
    string? notes = null)

// Atualização de dados cadastrais
public void Update(
    string fullName,
    DateOnly birthDate,
    Guid turmaId,
    string? notes)

// Transferência de turma (regra: não pode transferir aluno inativo/evadido)
public void Transfer(Guid newTurmaId)

// Inativação / Marcação de evasão
public void Deactivate(StudentStatus newStatus)
// Pré-condição: newStatus ∈ {Inativo, Evadido}; não pode reativar via este método

// Reativação (explícita)
public void Reactivate(Guid turmaId)
// Pré-condição: Status atual ≠ Ativo
```

**Invariantes**:
- `FullName` não pode ser vazio ou whitespace
- `DocumentId` não pode ser vazio
- `BirthDate` não pode ser futura
- `Transfer()` lança `InvalidOperationException` se `Status != Ativo`
- `Deactivate()` lança `InvalidOperationException` se `newStatus == Ativo`

---

### Turma (Classe)

Herda de `BaseEntity` (`Id`, `CreatedAt`, `UpdatedAt`, `DeletedAt?`).

| Campo | Tipo | Obrigatório | Regras |
|-------|------|-------------|--------|
| `Name` | `string` | ✅ | Máx. 200 chars (ex: "3º Ano A", "Turma de Matemática Avançada") |
| `Grade` | `string` | ✅ | Máx. 100 chars (ex: "3º Ano EF", "2ª Série EM") |
| `SchoolYear` | `int` | ✅ | Entre 2000 e 2100 |
| `Status` | `TurmaStatus` (enum) | ✅ | Padrão: `Ativa` na criação |
| `SchoolId` | `Guid` | ✅ | Isolamento multi-tenant por escola |
| `TeacherIds` | `IReadOnlyCollection<Guid>` | ❌ | Coleção de IDs de professores; persistida via tabela de junção `TurmaTeachers` |

**Factory / Métodos de domínio**:

```csharp
public static Turma Create(string name, string grade, int schoolYear, Guid schoolId)

public void Update(string name, string grade, int schoolYear)

public void Deactivate()
// Pré-condição: sem alunos ativos vinculados (validado no handler)

public void Reactivate()

public void AssignTeacher(Guid teacherId)
// Pré-condição: teacherId já não está na coleção

public void RemoveTeacher(Guid teacherId)
// Pré-condição: teacherId está na coleção
```

**Invariantes**:
- `Name` e `Grade` não podem ser vazios
- `SchoolYear` deve estar entre 2000 e 2100
- `AssignTeacher` lança exceção se professor já está na turma (sem duplicatas)

---

## Enums de Domínio

### StudentStatus

```csharp
public enum StudentStatus
{
    Ativo = 1,
    Inativo = 2,
    Evadido = 3
}
```

### TurmaStatus

```csharp
public enum TurmaStatus
{
    Ativa = 1,
    Inativa = 2
}
```

### DocumentType

```csharp
public enum DocumentType
{
    Cpf = 1,
    RegistroEstrangeiro = 2,
    IdInterno = 3
}
```

---

## Eventos de Domínio

Classes POCO em `Siaed.Domain/Events/`. **Não implementam `INotification`** (Domain não depende de MediatR). A conversão para `INotification` ocorre na camada Application.

```csharp
public record StudentCreatedEvent(Guid StudentId, Guid TurmaId, Guid SchoolId, DateTime OccurredAt);
public record StudentUpdatedEvent(Guid StudentId, Guid TurmaId, Guid SchoolId, DateTime OccurredAt);
public record StudentTransferredEvent(Guid StudentId, Guid FromTurmaId, Guid ToTurmaId, Guid SchoolId, DateTime OccurredAt);
public record StudentDeactivatedEvent(Guid StudentId, StudentStatus NewStatus, Guid SchoolId, DateTime OccurredAt);
```

---

## Mapeamento de Banco de Dados (EF Core)

### Tabela: `Students`

| Coluna | Tipo MySQL | Constraints |
|--------|-----------|-------------|
| `Id` | `CHAR(36)` | PK |
| `FullName` | `VARCHAR(200)` | NOT NULL |
| `DocumentType` | `TINYINT` | NOT NULL |
| `DocumentId` | `VARCHAR(50)` | NOT NULL |
| `BirthDate` | `DATE` | NOT NULL |
| `TurmaId` | `CHAR(36)` | NOT NULL, FK → Turmas(Id) |
| `Status` | `TINYINT` | NOT NULL |
| `EnrollmentDate` | `DATE` | NOT NULL |
| `SchoolId` | `CHAR(36)` | NOT NULL |
| `Notes` | `TEXT` | NULL |
| `CreatedAt` | `DATETIME(6)` | NOT NULL |
| `UpdatedAt` | `DATETIME(6)` | NOT NULL |
| `DeletedAt` | `DATETIME(6)` | NULL |

**Índices**:
- `IX_Students_TurmaId` — FK
- `IX_Students_SchoolId` — filtro multi-tenant
- `IX_Students_Status` — filtro de listagem
- `UX_Students_DocumentId_SchoolId` — único: mesmo documento não pode ser cadastrado duas vezes na mesma escola

**Query Filter**: `HasQueryFilter(s => s.DeletedAt == null)`

---

### Tabela: `Turmas`

| Coluna | Tipo MySQL | Constraints |
|--------|-----------|-------------|
| `Id` | `CHAR(36)` | PK |
| `Name` | `VARCHAR(200)` | NOT NULL |
| `Grade` | `VARCHAR(100)` | NOT NULL |
| `SchoolYear` | `INT` | NOT NULL |
| `Status` | `TINYINT` | NOT NULL |
| `SchoolId` | `CHAR(36)` | NOT NULL |
| `CreatedAt` | `DATETIME(6)` | NOT NULL |
| `UpdatedAt` | `DATETIME(6)` | NOT NULL |
| `DeletedAt` | `DATETIME(6)` | NULL |

**Índices**:
- `IX_Turmas_SchoolId` — filtro multi-tenant
- `IX_Turmas_Status` — filtro de listagem

**Query Filter**: `HasQueryFilter(t => t.DeletedAt == null)`

---

### Tabela: `TurmaTeachers` (junção many-to-many)

| Coluna | Tipo MySQL | Constraints |
|--------|-----------|-------------|
| `TurmaId` | `CHAR(36)` | PK (composta), FK → Turmas(Id) |
| `TeacherId` | `CHAR(36)` | PK (composta), FK → Teachers(Id) |

> Sem campos adicionais — EF Core implicit join table é suficiente.

---

## Relacionamentos

```
Student  *──1  Turma
Turma    *──*  Teacher  (via TurmaTeachers)
```

- Um aluno pertence a exatamente uma turma ativa
- Uma turma pode ter muitos alunos
- Uma turma pode ter muitos professores
- Um professor pode estar em muitas turmas

---

## Regras de Estado (Transições)

### StudentStatus

```
Ativo ──→ Inativo   (Deactivate com Inativo)
Ativo ──→ Evadido   (Deactivate com Evadido)
Inativo ──→ Ativo   (Reactivate)
Evadido ──→ Ativo   (Reactivate — reingresso na escola)
Inativo ──→ Evadido (não permitido — apenas inativação a partir de Ativo)
```

### TurmaStatus

```
Ativa ──→ Inativa  (Deactivate — somente se não houver alunos Ativos)
Inativa ──→ Ativa  (Reactivate)
```

---

## DTOs de Resposta

### StudentDto (listagem)

```csharp
public record StudentDto(
    Guid Id,
    string FullName,
    DocumentType DocumentType,
    string DocumentId,
    DateOnly BirthDate,
    Guid TurmaId,
    string TurmaName,
    StudentStatus Status,
    DateOnly EnrollmentDate
);
```

### StudentDetailDto (detalhe)

```csharp
public record StudentDetailDto(
    Guid Id,
    string FullName,
    DocumentType DocumentType,
    string DocumentId,
    DateOnly BirthDate,
    Guid TurmaId,
    string TurmaName,
    string TurmaGrade,
    int TurmaSchoolYear,
    StudentStatus Status,
    DateOnly EnrollmentDate,
    string? Notes,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
```

### TurmaDto

```csharp
public record TurmaDto(
    Guid Id,
    string Name,
    string Grade,
    int SchoolYear,
    TurmaStatus Status,
    int StudentCount,
    IReadOnlyList<TeacherSummaryDto> Teachers
);
```

### CsvImportResultDto

```csharp
public record CsvImportResultDto(
    int TotalRows,
    int SuccessCount,
    int FailureCount,
    IReadOnlyList<CsvImportRowError> Errors
);

public record CsvImportRowError(int RowNumber, string DocumentId, string Reason);
```
