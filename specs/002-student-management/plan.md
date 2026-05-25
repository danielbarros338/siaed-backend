# Implementation Plan: Gerenciamento de Alunos

**Branch**: `002-student-management` | **Date**: 2026-05-18 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/002-student-management/spec.md`

## Summary

Implementar o módulo de gerenciamento de alunos como entidade pedagógica central do SIAED. O módulo inclui CRUD completo de `Student` (Aluno) e `Turma` (Classe), transferência de turma, inativação (soft delete), listagem paginada com filtros, importação CSV em lote e emissão de eventos de domínio. A implementação segue Clean Architecture + DDD + CQRS via MediatR, com persistência em MySQL via EF Core, validações FluentValidation e autorização JWT Bearer por role (Diretor, Coordenador, Professor). Alunos não são usuários do sistema e não possuem credenciais. Toda operação de ciclo de vida emite um evento de domínio consumível por relatórios, IA e alertas.

## Technical Context

**Language/Version**: C# 13 / .NET 10

**Primary Dependencies**: MediatR 12, FluentValidation 11, Entity Framework Core 9 (Pomelo MySQL), Serilog, CsvHelper 33

**Storage**: MySQL 8 via `AppDbContext` (EF Core)

**Testing**: xUnit (convenção do projeto)

**Target Platform**: Linux/Windows server, ASP.NET Core

**Project Type**: web-service

**Performance Goals**: Queries de lista paginada com filtros em < 200ms p95 para volumes de escola (até 5.000 alunos por instância)

**Constraints**: LGPD — dados pessoais de alunos sanitizados antes de envio à IA; soft delete obrigatório; JWT Bearer Roles; paginação obrigatória em todas as queries de lista

**Scale/Scope**: Escola de médio porte — ~500–5.000 alunos, dezenas de turmas, múltiplos professores

## Constitution Check

*GATE: Passou antes da Phase 0 — Re-verificado após Phase 1.*

| Princípio | Status | Observações |
|-----------|--------|-------------|
| Clean Architecture (camadas) | ✅ PASS | `Student`, `Turma` → Domain; handlers → Application/Features; repositórios → Infra |
| Sem dependências externas no Domain | ✅ PASS | Entidades e enums puros; eventos de domínio como classes POCO |
| CQRS com MediatR | ✅ PASS | Commands: Create, Update, Deactivate, Transfer, ImportCsv; Queries: GetById, GetPaged |
| Result Pattern | ✅ PASS | Todos os handlers retornam `Result` ou `Result<T>` |
| FluentValidation via pipeline | ✅ PASS | `ValidationBehavior` já registrado; Validators em `Features/<Feature>/Validators/` |
| Soft delete (sem DELETE físico) | ✅ PASS | `DeletedAt` na `BaseEntity`; método `MarkAsDeleted()` já disponível |
| JWT Bearer + Roles | ✅ PASS | Roles `Diretor`, `Coordenador`, `Professor` já existem no sistema |
| LGPD | ✅ PASS | Dados do aluno sanitizados no `AIContextManager` antes do envio à IA |
| Entidades ricas (sem modelos anêmicos) | ✅ PASS | Factory methods `Create(...)`, setters `private set`, invariantes nos métodos |
| Sem regra de negócio em Controller/Infra | ✅ PASS | Controllers apenas delegam via `IMediator` |

**Nenhuma violação identificada.**

## Project Structure

### Documentation (this feature)

```text
specs/002-student-management/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
Siaed.Domain/
├── Entities/
│   ├── Student.cs              ← NEW — entidade Aluno (rich domain model)
│   └── Turma.cs                ← NEW — entidade Turma (com lista de TeacherIds)
├── Enums/
│   └── StudentStatus.cs        ← NEW — Ativo, Inativo, Evadido
└── Events/
    ├── StudentCreatedEvent.cs   ← NEW
    ├── StudentUpdatedEvent.cs   ← NEW
    ├── StudentTransferredEvent.cs ← NEW
    └── StudentDeactivatedEvent.cs ← NEW

Siaed.Application/
├── Features/
│   ├── Students/
│   │   ├── Commands/
│   │   │   ├── CreateStudentCommand.cs
│   │   │   ├── UpdateStudentCommand.cs
│   │   │   ├── DeactivateStudentCommand.cs
│   │   │   ├── TransferStudentCommand.cs
│   │   │   └── ImportStudentsCsvCommand.cs
│   │   ├── Queries/
│   │   │   ├── GetStudentByIdQuery.cs
│   │   │   └── GetStudentsPagedQuery.cs
│   │   ├── Handlers/
│   │   │   ├── CreateStudentHandler.cs
│   │   │   ├── UpdateStudentHandler.cs
│   │   │   ├── DeactivateStudentHandler.cs
│   │   │   ├── TransferStudentHandler.cs
│   │   │   ├── ImportStudentsCsvHandler.cs
│   │   │   ├── GetStudentByIdHandler.cs
│   │   │   └── GetStudentsPagedHandler.cs
│   │   ├── Validators/
│   │   │   ├── CreateStudentCommandValidator.cs
│   │   │   ├── UpdateStudentCommandValidator.cs
│   │   │   ├── DeactivateStudentCommandValidator.cs
│   │   │   ├── TransferStudentCommandValidator.cs
│   │   │   └── ImportStudentsCsvCommandValidator.cs
│   │   └── DTOs/
│   │       ├── StudentDto.cs
│   │       ├── StudentDetailDto.cs
│   │       └── CsvImportResultDto.cs
│   └── Turmas/
│       ├── Commands/
│       │   ├── CreateTurmaCommand.cs
│       │   ├── UpdateTurmaCommand.cs
│       │   ├── DeactivateTurmaCommand.cs
│       │   ├── AssignTeacherToTurmaCommand.cs
│       │   └── RemoveTeacherFromTurmaCommand.cs
│       ├── Queries/
│       │   ├── GetTurmaByIdQuery.cs
│       │   └── GetTurmasPagedQuery.cs
│       ├── Handlers/
│       │   ├── CreateTurmaHandler.cs
│       │   ├── UpdateTurmaHandler.cs
│       │   ├── DeactivateTurmaHandler.cs
│       │   ├── AssignTeacherToTurmaHandler.cs
│       │   ├── RemoveTeacherFromTurmaHandler.cs
│       │   ├── GetTurmaByIdHandler.cs
│       │   └── GetTurmasPagedHandler.cs
│       ├── Validators/
│       │   ├── CreateTurmaCommandValidator.cs
│       │   └── UpdateTurmaCommandValidator.cs
│       └── DTOs/
│           └── TurmaDto.cs
└── Interfaces/
    ├── IStudentRepository.cs   ← NEW
    └── ITurmaRepository.cs     ← NEW

Siaed.Infra/
├── Persistence/
│   ├── AppDbContext.cs         ← UPDATED (Students, Turmas DbSets)
│   └── Configurations/
│       ├── StudentConfiguration.cs  ← NEW
│       └── TurmaConfiguration.cs    ← NEW
├── Repositories/
│   ├── StudentRepository.cs    ← NEW
│   └── TurmaRepository.cs      ← NEW
└── Migrations/                 ← NEW migration: AddStudentsAndTurmas

Siaed.Api/
└── Controllers/
    ├── StudentsController.cs   ← NEW
    └── TurmasController.cs     ← NEW
```

**Structure Decision**: Segue o padrão estabelecido pelo Módulo 1. Um projeto de projeto .NET com quatro camadas (Api, Application, Domain, Infra). Features organizadas por agregado (`Students/`, `Turmas/`). Sem nova camada ou projeto necessário.

## Complexity Tracking

> Nenhuma violação identificada. Todos os princípios da constituição foram atendidos.
