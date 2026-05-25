# Implementation Plan: Gerenciamento de Notas por Atividade

**Branch**: `003-manage-student-grades` | **Date**: 2026-05-25 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/003-gerenciar-notas-atividades/spec.md`

## Summary

Implementar gerenciamento de notas por atividade com subseção dedicada no contexto de atividade, incluindo CRUD completo de notas, filtros por turma/professor/valor de nota e persistência em tabela independente `Grades`. A nota será armazenada como string para suportar convenções numéricas e literais (ex.: `8,5`, `MB`, `A`, `R`) conforme regra configurada da escola por atividade. A solução seguirá Clean Architecture + DDD + CQRS com MediatR, Result Pattern, validação por FluentValidation, auditoria e concorrência otimista por versão.

## Technical Context

**Language/Version**: C# 13 / .NET 10

**Primary Dependencies**: MediatR 12, FluentValidation 11, Entity Framework Core 9 (Pomelo MySQL), Serilog

**Storage**: MySQL 8 via EF Core (`AppDbContext`), nova tabela independente `Grades`

**Testing**: xUnit (unit + integration + contracts)

**Target Platform**: ASP.NET Core Web API em Linux/Windows server

**Project Type**: web-service

**Performance Goals**: listagem de notas por atividade com filtros em < 200ms p95 para turmas de até 40 alunos; operações de escrita com resposta < 300ms p95

**Constraints**: LGPD; autorização por roles e vínculo de turma; soft delete; concorrência otimista por versão; nota em string com validação por convenção da atividade

**Scale/Scope**: escolas com 500 a 5.000 alunos, dezenas de turmas, milhares de lançamentos de nota por período letivo

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### Pre-Phase 0

| Princípio | Status | Observações |
|-----------|--------|-------------|
| Clean Architecture (camadas) | PASS | API apenas delega via MediatR; regra de negócio em Application/Domain |
| Domain sem frameworks | PASS | Entidade de domínio `Grade` sem dependência de EF/MediatR |
| CQRS com MediatR | PASS | CRUD modelado com Commands/Queries separados |
| Result Pattern | PASS | Todos os handlers retornam `Result` ou `Result<T>` |
| Soft delete obrigatório | PASS | `Grade` herda `BaseEntity` e usa `DeletedAt` |
| Segurança e LGPD | PASS | Controle por perfil + vínculo professor-turma; sem exposição de dados desnecessários |
| Sem regra em Controller/Infra | PASS | Controllers finos; Infra apenas persistência e integrações |

### Post-Phase 1 Re-check

| Princípio | Status | Observações |
|-----------|--------|-------------|
| Clean Architecture (camadas) | PASS | Estrutura final mantém Domain/Application/Infra/API desacoplados |
| CQRS + Result Pattern | PASS | Contratos de command/query e respostas padronizadas documentadas |
| Segurança e autorização | PASS | Filtro por professor e controle de permissão no escopo da atividade |
| Soft delete e auditoria | PASS | Exclusão lógica e rastreabilidade previstas em modelo e contratos |

**Resultado do gate**: PASS, sem violações que exijam justificativa de complexidade.

## Project Structure

### Documentation (this feature)

```text
specs/003-gerenciar-notas-atividades/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── api-contracts.md
└── tasks.md
```

### Source Code (repository root)

```text
Siaed.Domain/
├── Entities/
│   └── Grade.cs                              <- NEW (tabela independente de notas)
├── Enums/
│   └── GradeStatus.cs                        <- NEW (ativo/inativo, se aplicável)
└── Events/
    ├── GradeCreatedEvent.cs                  <- NEW
    ├── GradeUpdatedEvent.cs                  <- NEW
    └── GradeDeletedEvent.cs                  <- NEW (soft delete)

Siaed.Application/
├── Features/
│   └── Grades/
│       ├── Commands/
│       │   ├── CreateGradeCommand.cs
│       │   ├── UpdateGradeCommand.cs
│       │   └── DeleteGradeCommand.cs
│       ├── Queries/
│       │   ├── GetGradeByIdQuery.cs
│       │   └── ListGradesQuery.cs
│       ├── Handlers/
│       │   ├── CreateGradeHandler.cs
│       │   ├── UpdateGradeHandler.cs
│       │   ├── DeleteGradeHandler.cs
│       │   ├── GetGradeByIdHandler.cs
│       │   └── ListGradesHandler.cs
│       ├── Validators/
│       │   ├── CreateGradeCommandValidator.cs
│       │   ├── UpdateGradeCommandValidator.cs
│       │   └── ListGradesQueryValidator.cs
│       └── DTOs/
│           ├── GradeDto.cs
│           └── GradeListItemDto.cs
└── Interfaces/
    └── IGradeRepository.cs                   <- NEW

Siaed.Infra/
├── Persistence/
│   ├── AppDbContext.cs                       <- UPDATED (DbSet<Grade>)
│   └── Configurations/
│       └── GradeConfiguration.cs             <- NEW
├── Repositories/
│   └── GradeRepository.cs                    <- NEW
└── Migrations/
    └── <timestamp>_AddGradesTable.cs         <- NEW

Siaed.Api/
└── Controllers/
    └── GradesController.cs                   <- NEW (CRUD + filtros)
```

**Structure Decision**: manter o padrão existente de módulos por feature e adicionar a feature `Grades` transversalmente nas quatro camadas. Embora exista o campo textual `Grade` na entidade `Activity` (ano/série), a nova entidade `Grade` representa nota do aluno e será isolada em tabela própria para evitar acoplamento e permitir CRUD independente.

## Complexity Tracking

Nenhuma violação constitucional identificada.
