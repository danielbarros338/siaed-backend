# Tasks: Gerenciamento de Alunos

**Branch**: `002-student-management` | **Date**: 2026-05-18

**Input**: [plan.md](plan.md) · [spec.md](spec.md) · [data-model.md](data-model.md) · [contracts/api-contracts.md](contracts/api-contracts.md) · [research.md](research.md)

## Format: `[ID] [P?] [Story?] Description`

- **[P]**: Pode rodar em paralelo (arquivos diferentes, sem dependências incompletas)
- **[Story]**: User story correspondente (US1–US6)
- Caminhos exatos de arquivo incluídos em cada tarefa

---

## Phase 1: Setup — Domain Layer

**Purpose**: Criar toda a camada de domínio pura (sem dependências externas) que serve de base para todas as user stories.

- [X] T001 Criar enums `StudentStatus`, `TurmaStatus` e `DocumentType` em `Siaed.Domain/Enums/StudentStatus.cs`, `TurmaStatus.cs` e `DocumentType.cs`
- [X] T002 [P] Criar domain events como records POCO em `Siaed.Domain/Events/` (`StudentCreatedEvent`, `StudentUpdatedEvent`, `StudentTransferredEvent`, `StudentDeactivatedEvent`) — sem implementar `INotification`
- [X] T003 Criar entidade `Turma` com factory `Create(...)`, métodos `Update()`, `Deactivate()`, `Reactivate()`, `AssignTeacher(Guid)` e `RemoveTeacher(Guid)` em `Siaed.Domain/Entities/Turma.cs`
- [X] T004 Criar entidade `Student` com factory `Create(...)`, métodos `Update()`, `Transfer(Guid)`, `Deactivate(StudentStatus)` e `Reactivate(Guid)` e invariantes em `Siaed.Domain/Entities/Student.cs`

**Checkpoint**: Domínio compilando sem dependências externas — pronto para camadas superiores.

---

## Phase 2: Foundational — Infraestrutura + Gerenciamento de Turmas

**Purpose**: Persistência (EF Core), repositórios e CRUD completo de `Turma` — pré-requisito bloqueante para todas as user stories de alunos.

**⚠️ CRÍTICO**: Nenhuma user story de aluno pode ser iniciada antes desta fase estar completa.

- [X] T005 [P] Criar `TurmaConfiguration` com `HasQueryFilter(t => t.DeletedAt == null)`, many-to-many `TurmaTeachers` e índices em `Siaed.Infra/Persistence/Configurations/TurmaConfiguration.cs`
- [X] T006 [P] Criar `StudentConfiguration` com `HasQueryFilter(s => s.DeletedAt == null)`, FK `TurmaId`, índice único `UX_Students_DocumentId_SchoolId` e demais índices em `Siaed.Infra/Persistence/Configurations/StudentConfiguration.cs`
- [X] T007 Adicionar `DbSet<Student>` e `DbSet<Turma>` ao `Siaed.Infra/Persistence/AppDbContext.cs` (após T005 e T006)
- [X] T008 Criar migration `AddStudentsAndTurmas` via `dotnet ef migrations add AddStudentsAndTurmas --project Siaed.Infra --startup-project Siaed.Api` e validar arquivo gerado em `Siaed.Infra/Migrations/`
- [X] T009 [P] Criar interface `IStudentRepository` com métodos `GetByIdAsync`, `ExistsByDocumentAsync`, `GetPagedAsync`, `AddAsync`, `Update` em `Siaed.Application/Interfaces/IStudentRepository.cs`
- [X] T010 [P] Criar interface `ITurmaRepository` com métodos `GetByIdAsync`, `HasActiveStudentsAsync`, `GetPagedAsync`, `AddAsync`, `Update` em `Siaed.Application/Interfaces/ITurmaRepository.cs`
- [X] T011 Implementar `TurmaRepository : ITurmaRepository` usando `AppDbContext` em `Siaed.Infra/Repositories/TurmaRepository.cs`
- [X] T012 Implementar `StudentRepository : IStudentRepository` usando `AppDbContext` em `Siaed.Infra/Repositories/StudentRepository.cs`
- [X] T013 Registrar `IStudentRepository → StudentRepository` e `ITurmaRepository → TurmaRepository` como `AddScoped` em `Siaed.Infra/DependencyInjection/InfraServiceExtensions.cs`
- [X] T014 [P] Criar `TurmaDto` (com `studentCount` e lista de `teacherIds`) em `Siaed.Application/Features/Turmas/DTOs/TurmaDto.cs`
- [X] T015 [P] Criar `CreateTurmaCommand` e `CreateTurmaCommandValidator` em `Siaed.Application/Features/Turmas/Commands/CreateTurmaCommand.cs` e `Validators/CreateTurmaCommandValidator.cs`
- [X] T016 [P] Criar `UpdateTurmaCommand` e `UpdateTurmaCommandValidator` em `Siaed.Application/Features/Turmas/Commands/UpdateTurmaCommand.cs` e `Validators/UpdateTurmaCommandValidator.cs`
- [X] T017 [P] Criar `DeactivateTurmaCommand` e `DeactivateTurmaCommandValidator` em `Siaed.Application/Features/Turmas/Commands/DeactivateTurmaCommand.cs` e `Validators/DeactivateTurmaCommandValidator.cs`
- [X] T018 [P] Criar `AssignTeacherToTurmaCommand` e `RemoveTeacherFromTurmaCommand` em `Siaed.Application/Features/Turmas/Commands/`
- [X] T019 [P] Criar `GetTurmaByIdQuery` e `GetTurmasPagedQuery` em `Siaed.Application/Features/Turmas/Queries/`
- [X] T020 Criar handlers `CreateTurmaHandler`, `UpdateTurmaHandler`, `DeactivateTurmaHandler` em `Siaed.Application/Features/Turmas/Handlers/` (cada um retorna `Result<TurmaDto>` ou `Result`)
- [X] T021 Criar handlers `AssignTeacherToTurmaHandler`, `RemoveTeacherFromTurmaHandler` em `Siaed.Application/Features/Turmas/Handlers/`
- [X] T022 Criar handlers `GetTurmaByIdHandler` e `GetTurmasPagedHandler` em `Siaed.Application/Features/Turmas/Handlers/`
- [X] T023 Criar `TurmasController` com todos os 6 endpoints definidos em `contracts/api-contracts.md` (`POST`, `PUT`, `PATCH deactivate`, `GET list`, `GET by id`, `POST teachers/{teacherId}`, `DELETE teachers/{teacherId}`) em `Siaed.Api/Controllers/TurmasController.cs`

**Checkpoint**: Turmas totalmente funcionais — é possível criar turmas e atribuir professores, pré-requisito para cadastrar alunos.

---

## Phase 3: US1 — Cadastro de Aluno (Priority: P1) 🎯 MVP

**Goal**: Permitir que Diretores e Coordenadores criem novos alunos vinculados a turmas ativas, com validação de unicidade de documento por escola.

**Independent Test**: `POST /api/v1/students` com dados válidos retorna 201 com aluno Ativo; segundo POST com mesmo CPF retorna 409; Professor recebe 403.

- [X] T024 [P] [US1] Criar `StudentDto` (lista — DocumentId mascarado) e `StudentDetailDto` (detalhe — DocumentId completo) em `Siaed.Application/Features/Students/DTOs/StudentDto.cs` e `StudentDetailDto.cs`
- [X] T025 [P] [US1] Criar `CreateStudentCommand` em `Siaed.Application/Features/Students/Commands/CreateStudentCommand.cs`
- [X] T026 [P] [US1] Criar `CreateStudentCommandValidator` com regras FluentValidation em `Siaed.Application/Features/Students/Validators/CreateStudentCommandValidator.cs`
- [X] T027 [P] [US1] Criar `GetStudentByIdQuery` e `GetStudentByIdHandler` (retorna `Result<StudentDetailDto>`) em `Siaed.Application/Features/Students/Queries/` e `Handlers/`
- [X] T028 [US1] Criar `CreateStudentHandler` verificando existência e status da turma, duplicidade de documento via `ExistsByDocumentAsync`, chamando `Student.Create(...)` e emitindo evento em `Siaed.Application/Features/Students/Handlers/CreateStudentHandler.cs`
- [X] T029 [US1] Criar `StudentsController` com `POST /api/v1/students` (roles: Coordenador, Diretor) e `GET /api/v1/students/{id}` (todos os roles) em `Siaed.Api/Controllers/StudentsController.cs`

**Checkpoint**: Cadastro de aluno e consulta por ID funcionando. US1 testável de forma independente.

---

## Phase 4: US2 — Atualização de Dados do Aluno (Priority: P1)

**Goal**: Permitir que Diretores e Coordenadores atualizem dados cadastrais de um aluno existente, com validação de duplicidade de documento ao alterar.

**Independent Test**: `PUT /api/v1/students/{id}` com novo nome retorna 200 com dados atualizados e `updatedAt` modificado; alterar documento para CPF de outro aluno retorna 409.

- [X] T030 [P] [US2] Criar `UpdateStudentCommand` em `Siaed.Application/Features/Students/Commands/UpdateStudentCommand.cs`
- [X] T031 [P] [US2] Criar `UpdateStudentCommandValidator` em `Siaed.Application/Features/Students/Validators/UpdateStudentCommandValidator.cs`
- [X] T032 [US2] Criar `UpdateStudentHandler` que busca o aluno, valida duplicidade se documento foi alterado e chama `student.Update(...)` em `Siaed.Application/Features/Students/Handlers/UpdateStudentHandler.cs`
- [X] T033 [US2] Adicionar endpoint `PUT /api/v1/students/{id}` (roles: Coordenador, Diretor) ao `Siaed.Api/Controllers/StudentsController.cs`

**Checkpoint**: Atualização de dados funcionando. US2 testável de forma independente.

---

## Phase 5: US5 — Listagem e Consulta de Alunos (Priority: P1)

**Goal**: Permitir que qualquer role consulte alunos com paginação e filtros (turmaId, status, search), com DocumentId mascarado na listagem por LGPD.

**Independent Test**: `GET /api/v1/students?turmaId={id}&pageSize=10` retorna apenas alunos da turma com paginação correta e documentos mascarados. Sem filtro, alunos com `DeletedAt != null` não aparecem.

- [X] T034 [P] [US5] Criar `GetStudentsPagedQuery` com parâmetros `turmaId?`, `status?`, `search?`, `page`, `pageSize` em `Siaed.Application/Features/Students/Queries/GetStudentsPagedQuery.cs`
- [X] T035 [US5] Criar `GetStudentsPagedHandler` que retorna `Result<PagedResult<StudentDto>>` com mascaramento de DocumentId (`123***8901`) em `Siaed.Application/Features/Students/Handlers/GetStudentsPagedHandler.cs`
- [X] T036 [US5] Adicionar endpoint `GET /api/v1/students` com query params paginados (todos os roles) ao `Siaed.Api/Controllers/StudentsController.cs`

**Checkpoint**: Listagem paginada com filtros e mascaramento LGPD funcionando. US5 testável de forma independente.

---

## Phase 6: US3 — Transferência de Turma (Priority: P2)

**Goal**: Permitir que Diretores e Coordenadores transfiram alunos Ativos para outra turma ativa, rejeitando tentativas com alunos inativos/evadidos ou turmas inativas.

**Independent Test**: `PATCH /api/v1/students/{id}/transfer` com turma ativa retorna 200 com novo TurmaId; aluno Evadido retorna 422; turma de destino inativa retorna 422.

- [X] T037 [P] [US3] Criar `TransferStudentCommand` e `TransferStudentCommandValidator` em `Siaed.Application/Features/Students/Commands/` e `Validators/`
- [X] T038 [US3] Criar `TransferStudentHandler` que valida status do aluno e status da turma destino, chama `student.Transfer(newTurmaId)` e emite `StudentTransferredEvent` em `Siaed.Application/Features/Students/Handlers/TransferStudentHandler.cs`
- [X] T039 [US3] Adicionar endpoint `PATCH /api/v1/students/{id}/transfer` (roles: Coordenador, Diretor) ao `Siaed.Api/Controllers/StudentsController.cs`

**Checkpoint**: Transferência de turma funcionando com todas as validações de domínio. US3 testável de forma independente.

---

## Phase 7: US4 — Inativação de Aluno (Priority: P2)

**Goal**: Permitir que Diretores e Coordenadores inativem ou marquem como evadidos alunos existentes, preservando histórico via soft delete.

**Independent Test**: `PATCH /api/v1/students/{id}/deactivate` com `newStatus: 3` retorna 200 com status Evadido e `deletedAt` preenchido; aluno não aparece em listagem padrão mas aparece com filtro `status=3`.

- [X] T040 [P] [US4] Criar `DeactivateStudentCommand` e `DeactivateStudentCommandValidator` (valida `newStatus ∈ {2, 3}`) em `Siaed.Application/Features/Students/Commands/` e `Validators/`
- [X] T041 [US4] Criar `DeactivateStudentHandler` que chama `student.Deactivate(newStatus)` e `student.MarkAsDeleted()` para soft delete em `Siaed.Application/Features/Students/Handlers/DeactivateStudentHandler.cs`
- [X] T042 [US4] Adicionar endpoint `PATCH /api/v1/students/{id}/deactivate` (roles: Coordenador, Diretor) ao `Siaed.Api/Controllers/StudentsController.cs`

**Checkpoint**: Inativação e soft delete funcionando. Alunos inativos/evadidos filtrados corretamente. US4 testável de forma independente.

---

## Phase 8: US6 — Importação de Alunos via CSV (Priority: P3)

**Goal**: Permitir importação em lote de alunos via CSV com dois modos: `fail-fast` (reverte tudo ao primeiro erro) e `partial` (persiste válidos, retorna erros das linhas inválidas).

**Independent Test**: `POST /api/v1/students/import` com CSV de 5 alunos válidos + 1 inválido em modo `partial` persiste 5 e retorna erro na linha inválida; modo `fail-fast` com mesma entrada não persiste nenhum e retorna o primeiro erro.

- [X] T043 [P] [US6] Adicionar pacote `CsvHelper` versão 33 ao `Siaed.Application/Siaed.Application.csproj`
- [X] T044 [P] [US6] Criar `CsvImportResultDto` e `CsvImportRowError` em `Siaed.Application/Features/Students/DTOs/CsvImportResultDto.cs`
- [X] T045 [P] [US6] Criar `StudentCsvRow` (mapeamento de colunas CSV) e `StudentCsvRowMap : ClassMap<StudentCsvRow>` em `Siaed.Application/Features/Students/DTOs/StudentCsvRow.cs`
- [X] T046 [US6] Criar `ImportStudentsCsvCommand` (com `Stream CsvContent`, `string ImportMode`, `Guid TurmaId`, `Guid SchoolId`) e `ImportStudentsCsvCommandValidator` (valida limite de 500 linhas, modo válido) em `Siaed.Application/Features/Students/Commands/` e `Validators/`
- [X] T047 [US6] Criar `ImportStudentsCsvHandler` implementando lógica de `fail-fast` (transação única, rollback no 1º erro) e `partial` (iterar todas as linhas, coletar erros) em `Siaed.Application/Features/Students/Handlers/ImportStudentsCsvHandler.cs`
- [X] T048 [US6] Adicionar endpoint `POST /api/v1/students/import` (multipart/form-data, roles: Coordenador, Diretor) ao `Siaed.Api/Controllers/StudentsController.cs`

**Checkpoint**: Importação CSV em lote funcionando nos dois modos. US6 testável de forma independente.

---

## Phase 9: Polish & Cross-Cutting Concerns

- [X] T049 Aplicar migration ao banco: `dotnet ef database update --project Siaed.Infra --startup-project Siaed.Api`
- [X] T050 [P] Verificar que soft delete funciona corretamente em todas as queries (alunos com `DeletedAt != null` não retornam sem filtro explícito)
- [X] T051 [P] Verificar mascaramento de `DocumentId` na listagem (`GET /api/v1/students`) — LGPD: ex. `123***8901`
- [X] T052 [P] Verificar paginação em todas as queries de lista (`GET /api/v1/students` e `GET /api/v1/turmas`) com `PagedResult<T>` correto
- [X] T053 Executar `dotnet build` e garantir zero erros de compilação

---

## Dependency Graph

```
Phase 1 (Domain)
    └── Phase 2 (Infra + Turmas)  ← BLOQUEANTE
            ├── Phase 3 (US1 Cadastro) [P1]
            │       ├── Phase 4 (US2 Atualização) [P1]  ← depende de US1
            │       ├── Phase 5 (US5 Listagem)    [P1]  ← depende do Foundation
            │       ├── Phase 6 (US3 Transferência)[P2] ← depende de US1
            │       └── Phase 7 (US4 Inativação)  [P2]  ← depende de US1
            └── Phase 8 (US6 CSV Import)           [P3] ← depende de US1
                        └── Phase 9 (Polish)
```

**User stories sem dependências entre si** (após Foundation):
- US3, US4, US5, US6 podem ser desenvolvidas em paralelo após US1 estar completa

---

## Parallel Execution Examples

### Sprint Completo (1 desenvolvedor)

```
T001 → T002, T003 → T004
T005, T006 → T007 → T008
T009, T010 → T011, T012 → T013
T014, T015, T016, T017, T018, T019 → T020, T021, T022 → T023
T024, T025, T026, T027 → T028 → T029
T030, T031 → T032 → T033
T034 → T035 → T036
T037 → T038 → T039
T040 → T041 → T042
T043, T044, T045 → T046 → T047 → T048
T049 → T050, T051, T052 → T053
```

### MVP Mínimo (apenas US1 — cadastrar e consultar aluno)

```
T001 → T003, T004
T005, T006 → T007 → T008
T009, T010 → T011, T012 → T013
T014, T015 → T020 → T023  (apenas CreateTurma para ter turmas de teste)
T024, T025, T026, T027 → T028 → T029
T049 → T053
```

---

## Implementation Strategy

1. **MVP** (Phases 1–3): Entregar US1 — cadastro de aluno funcional com turmas
2. **Incremento 1** (Phase 4–5): Completar as 3 user stories P1 (US1+US2+US5)
3. **Incremento 2** (Phases 6–7): Entregar US3 e US4 (P2) — transferência e inativação
4. **Incremento 3** (Phase 8): Entregar US6 (P3) — importação CSV

**Total de tarefas**: 53  
**Tarefas por fase**:
- Phase 1 (Domain): 4 tarefas
- Phase 2 (Foundation + Turmas): 19 tarefas
- Phase 3 (US1): 6 tarefas
- Phase 4 (US2): 4 tarefas
- Phase 5 (US5): 3 tarefas
- Phase 6 (US3): 3 tarefas
- Phase 7 (US4): 3 tarefas
- Phase 8 (US6): 6 tarefas
- Phase 9 (Polish): 5 tarefas
