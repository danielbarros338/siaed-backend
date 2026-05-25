# Tasks: Módulo 1 — Completar sem Refatoração Destrutiva

**Input**: Design documents from `/specs/001-modulo1-estado-atual/`

**Prerequisites**: [plan.md](plan.md) | [spec.md](spec.md) | [research.md](research.md) | [data-model.md](data-model.md) | [contracts/api-contracts.md](contracts/api-contracts.md) | [quickstart.md](quickstart.md)

**Tests**: Sem projeto de testes (fora do escopo desta feature — ver plan.md).

**Organization**: Tarefas agrupadas por user story para permitir implementação e entrega independente. As US1–US4 do spec.md já estão implementadas. As tarefas abaixo cobrem as lacunas restantes.

---

## Format: `[ID] [P?] [Story?] Description`

- **[P]**: Pode ser executada em paralelo (arquivos diferentes, sem dependências incompletas)
- **[Story]**: User story correspondente (US5 a US12 = lacunas L-001 a L-008 + divergências D-001/D-005)
- Caminhos de arquivo sempre relativos à raiz da solution

---

## Mapeamento de User Stories

| Story | Origem | Descrição |
|---|---|---|
| US5 | D-001 | Vincular `User` ao `Teacher` no registro |
| US6 | D-005 | Corrigir acesso cross-professor (LGPD) |
| US7 | L-001 | Endpoints DELETE (soft delete) para as 3 entidades |
| US8 | L-004/L-005 | Perfil do professor autenticado (`GET /teachers/me`) |
| US9 | L-003 | Validadores ausentes para Generate e Update |
| US10 | L-002 | Transições de status Publish/Archive |
| US11 | L-006 | IA auxiliar: Resumo e comunicado para pais |
| US12 | L-007 | Filtros avançados nas queries de listagem |
| US13 | L-008 | Histórico de uso da IA |

---

## Phase 1: Setup (Mudança de domínio — pré-requisito de tudo)

**Purpose**: Adicionar `UserId` à entidade `Teacher` no domínio. Este é o único artefato que não pode ser parallelizado: todas as fases seguintes dependem desta mudança existir no Domain antes de mapear na Infra e usar na Application.

- [X] T001 Adicionar campo `public Guid? UserId { get; private set; }` e sobrecarga de factory `Create(name, email, subject, schoolId, userId)` em `Siaed.Domain/Entities/Teacher.cs`

**Checkpoint**: `dotnet build` deve passar antes de avançar. A partir daqui, T002–T004 podem ser executadas em paralelo.

---

## Phase 2: Foundational — US5 + US6 (Bloqueantes de segurança)

**Purpose**: Completar a ligação User↔Teacher (D-001) e corrigir o acesso cross-professor (D-005/LGPD). **Nenhuma user story de negócio pode ser entregue antes desta fase estar completa.**

**⚠️ CRÍTICO**: D-005 é uma violação LGPD ativa. Esta fase tem prioridade absoluta.

### D-001 — Vincular User ao Teacher [US5]

- [X] T002 [P] [US5] Adicionar mapeamento de `UserId` (nullable FK para `Users.Id`) e `HasIndex(t => t.UserId)` em `Siaed.Infra/Persistence/TeacherConfiguration.cs`
- [X] T003 [P] [US5] Adicionar assinatura `Task<Teacher?> GetByUserIdAsync(Guid userId, CancellationToken ct = default)` em `Siaed.Application/Interfaces/ITeacherRepository.cs`
- [X] T004 [P] [US5] Adicionar campos opcionais `string? Subject` e `string? SchoolId` em `Siaed.Application/Features/Auth/Commands/RegisterCommand.cs`
- [X] T005 [US5] Implementar `GetByUserIdAsync` usando `FirstOrDefaultAsync` em `Siaed.Infra/Repositories/TeacherRepository.cs` (depende de T003)
- [X] T006 [US5] Atualizar `Siaed.Application/Features/Auth/Handlers/RegisterCommandHandler.cs` para criar `Teacher` via `Teacher.Create(name, email, subject, schoolId, user.Id)` quando `role == UserRole.Professor` (depende de T004, T005)
- [X] T007 [US5] Criar migration `AddUserIdAndIndexes` adicionando `Teacher.UserId`, FK opcional para `Users.Id`, indexes em `Teacher.UserId`, `LessonPlan.Status`, `LessonPlan.IsAIGenerated`, `Activity.Status`, `Activity.IsAIGenerated`, `PedagogicalReport.Status` — executar `dotnet ef migrations add AddUserIdAndIndexes --project Siaed.Infra --startup-project Siaed.Api` (depende de T001, T002)

### D-005 — Ownership check (LGPD) [US6]

- [X] T008 [P] [US6] Adicionar `Guid RequestingUserId` em `Siaed.Application/Features/LessonPlans/Queries/GetLessonPlanByIdQuery.cs` e ownership check via `ITeacherRepository.GetByUserIdAsync` em `Siaed.Application/Features/LessonPlans/Handlers/GetLessonPlanByIdQueryHandler.cs` (depende de T005)
- [X] T009 [P] [US6] Adicionar `Guid RequestingUserId` em `Siaed.Application/Features/Activities/Queries/GetActivityByIdQuery.cs` e ownership check em `Siaed.Application/Features/Activities/Handlers/GetActivityByIdQueryHandler.cs` (depende de T005)
- [X] T010 [P] [US6] Adicionar `Guid RequestingUserId` em `Siaed.Application/Features/Reports/Queries/GetReportByIdQuery.cs` e ownership check em `Siaed.Application/Features/Reports/Handlers/GetReportByIdQueryHandler.cs` (depende de T005)
- [X] T011 [P] [US6] Adicionar `Guid RequestingUserId` em `Siaed.Application/Features/LessonPlans/Commands/UpdateLessonPlanCommand.cs` e ownership check em `Siaed.Application/Features/LessonPlans/Handlers/UpdateLessonPlanCommandHandler.cs` (depende de T005)
- [X] T012 [P] [US6] Adicionar `Guid RequestingUserId` em `Siaed.Application/Features/Activities/Commands/UpdateActivityCommand.cs` e ownership check em `Siaed.Application/Features/Activities/Handlers/UpdateActivityCommandHandler.cs` (depende de T005)
- [X] T013 [P] [US6] Adicionar `Guid RequestingUserId` em `Siaed.Application/Features/Reports/Commands/UpdateReportCommand.cs` e ownership check em `Siaed.Application/Features/Reports/Handlers/UpdateReportCommandHandler.cs` (depende de T005)
- [X] T014 [US6] Atualizar actions `GetById` e `Update` em `Siaed.Api/Controllers/LessonPlansController.cs`, `Siaed.Api/Controllers/ActivitiesController.cs` e `Siaed.Api/Controllers/ReportsController.cs` para extrair `userId` do JWT (`User.FindFirstValue(ClaimTypes.NameIdentifier)`) e passá-lo nas queries/commands (depende de T008–T013)

**Checkpoint**: Após T014, qualquer professor que tente acessar recurso de outro recebe `404`. Nenhum dado vaza entre professores.

---

## Phase 3: User Story 7 — DELETE endpoints (Priority: P1) — L-001

**Goal**: Professores podem excluir seus próprios planos de aula, atividades e relatórios via soft delete.

**Independent Test**: `DELETE /api/v1/lessonplans/{id}` com token do dono retorna `204`; com token de outro professor retorna `404`; com ID inexistente retorna `404`.

- [X] T015 [P] [US7] Criar `Siaed.Application/Features/LessonPlans/Commands/DeleteLessonPlanCommand.cs` (`record` com `Id` e `RequestingUserId`) e `Siaed.Application/Features/LessonPlans/Handlers/DeleteLessonPlanCommandHandler.cs` com lógica: buscar → ownership check → `lessonPlan.Delete()` → `Update()` → `CommitAsync()` → `Result` (depende de T005)
- [X] T016 [P] [US7] Criar `Siaed.Application/Features/Activities/Commands/DeleteActivityCommand.cs` e `Siaed.Application/Features/Activities/Handlers/DeleteActivityCommandHandler.cs` com mesma lógica de T015 para `Activity` (depende de T005)
- [X] T017 [P] [US7] Criar `Siaed.Application/Features/Reports/Commands/DeleteReportCommand.cs` e `Siaed.Application/Features/Reports/Handlers/DeleteReportCommandHandler.cs` com mesma lógica de T015 para `PedagogicalReport` (depende de T005)
- [X] T018 [US7] Adicionar action `[HttpDelete("{id:guid}")]` em `Siaed.Api/Controllers/LessonPlansController.cs`, `Siaed.Api/Controllers/ActivitiesController.cs` e `Siaed.Api/Controllers/ReportsController.cs` retornando `204 NoContent` ou `NotFound` (depende de T015–T017)

**Checkpoint**: DELETE funcional com soft delete, sem exposição de dados de outros professores.

---

## Phase 4: User Story 8 — Perfil do professor autenticado (Priority: P1) — L-004/L-005

**Goal**: Professor autenticado obtém seu próprio perfil Teacher via `GET /api/v1/teachers/me` sem precisar conhecer seu TeacherId.

**Independent Test**: `GET /api/v1/teachers/me` com token válido de Professor retorna `200` com `id`, `name`, `email`, `subject`, `schoolId`.

- [X] T019 [US8] Criar `Siaed.Application/Features/Teachers/DTOs/TeacherDto.cs` (record com `Id`, `UserId?`, `Name`, `Email`, `Subject`, `SchoolId`, `CreatedAt`), `Siaed.Application/Features/Teachers/Queries/GetMyTeacherProfileQuery.cs` (`record` com `UserId`) e `Siaed.Application/Features/Teachers/Handlers/GetMyTeacherProfileQueryHandler.cs` usando `ITeacherRepository.GetByUserIdAsync` → mapear para `TeacherDto` → `Result<TeacherDto>` (depende de T005)
- [X] T020 [US8] Criar `Siaed.Api/Controllers/TeachersController.cs` com action `[HttpGet("me")]` que extrai `userId` do JWT e envia `GetMyTeacherProfileQuery` — retorna `200 OK` ou `404 NotFound` (depende de T019)

**Checkpoint**: `GET /api/v1/teachers/me` retorna perfil completo do professor.

---

## Phase 5: User Story 9 — Validadores ausentes (Priority: P2) — L-003

**Goal**: Todos os commands de geração e atualização têm validação FluentValidation ativa via `ValidationBehavior`.

**Independent Test**: `POST /api/v1/activities/generate` com body vazio retorna `400 Bad Request` com lista de erros de validação.

- [X] T021 [P] [US9] Criar `Siaed.Application/Features/Activities/Validators/GenerateActivityCommandValidator.cs` validando todos os campos obrigatórios de `GenerateActivityCommand` (subject, gradeLevel, ageRange, activityType)
- [X] T022 [P] [US9] Criar `Siaed.Application/Features/Reports/Validators/GenerateReportCommandValidator.cs` validando todos os campos obrigatórios de `GenerateReportCommand` (studentName, gradeLevel, period, performanceData)
- [X] T023 [P] [US9] Criar `Siaed.Application/Features/LessonPlans/Validators/UpdateLessonPlanCommandValidator.cs` validando `Id` (NotEmpty), `RequestingUserId` (NotEmpty) e campos de conteúdo obrigatórios
- [X] T024 [P] [US9] Criar `Siaed.Application/Features/Activities/Validators/UpdateActivityCommandValidator.cs` com mesma estrutura de T023 para `UpdateActivityCommand`
- [X] T025 [P] [US9] Criar `Siaed.Application/Features/Reports/Validators/UpdateReportCommandValidator.cs` com mesma estrutura de T023 para `UpdateReportCommand`

**Checkpoint**: Requests inválidos são rejeitados com `400` antes de chegar nos handlers.

---

## Phase 6: User Story 10 — Transições de status Publish/Archive (Priority: P2) — L-002

**Goal**: Professores podem publicar e arquivar planos de aula e atividades via PATCH.

**Independent Test**: `PATCH /api/v1/lessonplans/{id}/publish` com token do dono retorna `204`; tentar publicar plano já arquivado retorna `400`.

- [X] T026 [P] [US10] Criar `Siaed.Application/Features/LessonPlans/Commands/PublishLessonPlanCommand.cs` e `Siaed.Application/Features/LessonPlans/Handlers/PublishLessonPlanCommandHandler.cs` com lógica: buscar → ownership check → `lessonPlan.Publish()` em try/catch `InvalidOperationException` → `Update()` → `CommitAsync()` → `Result`
- [X] T027 [P] [US10] Criar `Siaed.Application/Features/LessonPlans/Commands/ArchiveLessonPlanCommand.cs` e `Siaed.Application/Features/LessonPlans/Handlers/ArchiveLessonPlanCommandHandler.cs` com mesma estrutura usando `lessonPlan.Archive()`
- [X] T028 [P] [US10] Criar `Siaed.Application/Features/Activities/Commands/PublishActivityCommand.cs` e `Siaed.Application/Features/Activities/Handlers/PublishActivityCommandHandler.cs` (espelha T026 para `Activity`)
- [X] T029 [P] [US10] Criar `Siaed.Application/Features/Activities/Commands/ArchiveActivityCommand.cs` e `Siaed.Application/Features/Activities/Handlers/ArchiveActivityCommandHandler.cs` (espelha T027 para `Activity`)
- [X] T030 [US10] Adicionar actions `[HttpPatch("{id:guid}/publish")]` e `[HttpPatch("{id:guid}/archive")]` em `Siaed.Api/Controllers/LessonPlansController.cs` e `Siaed.Api/Controllers/ActivitiesController.cs` retornando `204 NoContent` ou `400 BadRequest` (depende de T026–T029)

**Checkpoint**: Planos e atividades percorrem o ciclo de vida Draft → Published → Archived via API.

---

## Phase 7: User Story 11 — IA auxiliar para relatórios (Priority: P2) — L-006

**Goal**: Professor pode gerar resumo executivo e comunicado para pais a partir de um relatório pedagógico já existente.

**Independent Test**: `POST /api/v1/reports/{id}/summarize` com token do dono retorna `200` com `summary` gerado pela IA e `AIRequest` persistido como `Completed`.

- [X] T031 [P] [US11] Criar `Siaed.Application/Features/Reports/DTOs/SummarizeReportResponseDto.cs` (record: `ReportId`, `Summary`, `TokensUsed`, `EstimatedCost`), `Siaed.Application/Features/Reports/Commands/SummarizeReportCommand.cs` e `Siaed.Application/Features/Reports/Handlers/SummarizeReportCommandHandler.cs` com lógica: buscar relatório → ownership check → `IPromptBuilder.BuildSummarizationPrompt()` → `IOpenAIService` → salvar AIRequest/AIResponse → retornar `Result<SummarizeReportResponseDto>` (depende de T005)
- [X] T032 [P] [US11] Criar `Siaed.Application/Features/Reports/DTOs/ParentCommunicationResponseDto.cs` (record: `ReportId`, `ParentCommunication`, `TokensUsed`, `EstimatedCost`), `Siaed.Application/Features/Reports/Commands/GenerateParentCommunicationCommand.cs` e `Siaed.Application/Features/Reports/Handlers/GenerateParentCommunicationCommandHandler.cs` usando `IPromptBuilder.BuildParentCommunicationPrompt()` (depende de T005)
- [X] T033 [US11] Adicionar actions `[HttpPost("{id:guid}/summarize")]` e `[HttpPost("{id:guid}/parent-communication")]` em `Siaed.Api/Controllers/ReportsController.cs` retornando `200 OK` ou `400 BadRequest` (depende de T031–T032)

**Checkpoint**: Professores podem enriquecer relatórios com resumos e comunicados gerados pela IA.

---

## Phase 8: User Story 12 — Filtros avançados nas listagens (Priority: P3) — L-007

**Goal**: Endpoints de listagem aceitam filtros opcionais `status` e `isAIGenerated` além do `teacherId` existente.

**Independent Test**: `GET /api/v1/lessonplans?teacherId={id}&status=Published` retorna apenas planos publicados do professor.

- [X] T034 [P] [US12] Adicionar parâmetros opcionais `string? Status` e `bool? IsAIGenerated` em `Siaed.Application/Features/LessonPlans/Queries/ListLessonPlansQuery.cs` e aplicar filtros condicionais (`Where(x => ...)`) em `Siaed.Application/Features/LessonPlans/Handlers/ListLessonPlansQueryHandler.cs` e no action do `Siaed.Api/Controllers/LessonPlansController.cs`
- [X] T035 [P] [US12] Adicionar mesmos parâmetros em `Siaed.Application/Features/Activities/Queries/ListActivitiesQuery.cs` e `Siaed.Application/Features/Activities/Handlers/ListActivitiesQueryHandler.cs` e `Siaed.Api/Controllers/ActivitiesController.cs`
- [X] T036 [P] [US12] Adicionar mesmos parâmetros em `Siaed.Application/Features/Reports/Queries/ListReportsQuery.cs` e `Siaed.Application/Features/Reports/Handlers/ListReportsQueryHandler.cs` e `Siaed.Api/Controllers/ReportsController.cs`

**Checkpoint**: Listagens retornam subconjuntos filtrados, aproveitando os índices criados em T007.

---

## Phase 9: User Story 13 — Histórico de uso da IA (Priority: P3) — L-008

**Goal**: Professor consulta seu histórico de requisições à IA com paginação.

**Independent Test**: `GET /api/v1/ai/requests?page=1&pageSize=10` retorna `PagedResult<AIRequestDto>` com requests do professor autenticado.

- [X] T037 [US13] Criar `Siaed.Application/Features/AI/DTOs/AIRequestDto.cs` (record: `Id`, `Type`, `Status`, `Model`, `PromptTokens`, `CompletionTokens`, `EstimatedCost`, `CreatedAt`), `Siaed.Application/Features/AI/Queries/ListAIRequestsQuery.cs` (`record` com `RequestingUserId`, `Page`, `PageSize`) e `Siaed.Application/Features/AI/Handlers/ListAIRequestsQueryHandler.cs` usando `IAIRequestRepository` para buscar paginado por `TeacherId` (depende de T005)
- [X] T038 [US13] Criar `Siaed.Api/Controllers/AIController.cs` com action `[HttpGet("requests")]` que extrai `userId` do JWT e envia `ListAIRequestsQuery` — retorna `200 OK` com `PagedResult<AIRequestDto>` (depende de T037)

**Checkpoint**: Professor visualiza custo e uso da IA ao longo do tempo.

---

## Final Phase: Polish & Cross-Cutting

- [X] T039 Executar `dotnet build` na solution inteira e corrigir todos os erros de compilação resultantes das alterações de assinatura (ex: `GetLessonPlanByIdQuery` agora exige `RequestingUserId` em todos os call sites)

---

## Dependencies

```
T001
├── T002 (TeacherConfiguration)
│   └── T007 (migration)
├── T003 (ITeacherRepository interface)
│   └── T005 (TeacherRepository impl)
│       ├── T006 (RegisterCommandHandler)
│       ├── T008 (GetLessonPlanById ownership)
│       ├── T009 (GetActivityById ownership)
│       ├── T010 (GetReportById ownership)
│       ├── T011 (UpdateLessonPlan ownership)
│       ├── T012 (UpdateActivity ownership)
│       ├── T013 (UpdateReport ownership)
│       │   └── T014 (controllers: GetById + Update com userId)
│       ├── T015 (DeleteLessonPlan handler)
│       ├── T016 (DeleteActivity handler)
│       ├── T017 (DeleteReport handler)
│       │   └── T018 (controllers: DELETE actions)
│       ├── T019 (GetMyTeacherProfile handler)
│       │   └── T020 (TeachersController)
│       ├── T031 (SummarizeReport handler)
│       ├── T032 (GenerateParentCommunication handler)
│       │   └── T033 (ReportsController: IA actions)
│       └── T037 (ListAIRequests handler)
│           └── T038 (AIController)
└── T004 (RegisterCommand campos)
    └── T006 (RegisterCommandHandler)

Independentes (sem deps externas):
T021–T025 (validators)
T026–T029 (Publish/Archive handlers) → T030 (controllers)
T034–T036 (list filters)
T039 (build verification — deve ser último)
```

---

## Execução em paralelo por story

### Após T001 + T005 (unlocker):
```
Paralelo A: T008, T009, T010, T011, T012, T013
Paralelo B: T015, T016, T017
Paralelo C: T026, T027, T028, T029
Paralelo D: T031, T032
Paralelo E: T034, T035, T036
Paralelo F: T021, T022, T023, T024, T025
```

### Sequencial (após paralelos):
```
T014 (após A) → T018 (após B) → T030 (após C) → T033 (após D) → T039
T019 → T020
T037 → T038
```

---

## Implementation Strategy

**MVP (entregar primeiro)**: Phases 1–3 = T001–T018

Após MVP:
- D-001 e D-005 corrigidos (LGPD compliant)
- DELETE funcional para as 3 entidades
- Professor pode ver/editar/excluir apenas seus próprios recursos

**Incremento 2**: Phases 4–6 = T019–T030 (Teacher profile + validators + Publish/Archive)

**Incremento 3**: Phases 7–9 = T031–T038 (IA auxiliar + filtros + histórico)

**Verificação final**: T039 (build limpo)

---

## Summary

| Métrica | Valor |
|---|---|
| Total de tarefas | 39 |
| Phase 1 (Setup) | 1 |
| Phase 2 (Foundational D-001+D-005) | 13 |
| Phase 3 (US7 — DELETE) | 4 |
| Phase 4 (US8 — Teacher profile) | 2 |
| Phase 5 (US9 — Validators) | 5 |
| Phase 6 (US10 — Publish/Archive) | 5 |
| Phase 7 (US11 — IA auxiliar) | 3 |
| Phase 8 (US12 — Filtros) | 3 |
| Phase 9 (US13 — AI history) | 2 |
| Final Polish | 1 |
| Tarefas paralelizáveis | 23 |
| MVP (T001–T018) | 18 tarefas |
