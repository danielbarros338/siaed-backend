# Tasks: Gerenciamento de Notas por Atividade

**Input**: Design documents from /specs/003-gerenciar-notas-atividades/

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Preparar estrutura base da feature Grades no padrao da solucao

- [x] T001 Criar estrutura de pastas da feature em Siaed.Application/Features/Grades/Commands, Siaed.Application/Features/Grades/Queries, Siaed.Application/Features/Grades/Handlers, Siaed.Application/Features/Grades/Validators e Siaed.Application/Features/Grades/DTOs
- [x] T002 Criar estrutura de pastas para persistencia de notas em Siaed.Infra/Persistence/Configurations e Siaed.Infra/Repositories
- [x] T003 Criar controller da feature em Siaed.Api/Controllers/GradesController.cs com rotas base api/v1/grades

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Infraestrutura de dominio e persistencia obrigatoria antes de qualquer historia

**CRITICAL**: Nenhuma user story deve iniciar antes da conclusao desta fase

- [x] T004 Criar entidade de dominio Grade em Siaed.Domain/Entities/Grade.cs com ActivityId, StudentId, SchoolClassId, TeacherId, GradeValue, ConventionKey e RowVersion
- [x] T005 [P] Criar eventos de dominio GradeCreatedEvent, GradeUpdatedEvent e GradeDeletedEvent em Siaed.Domain/Events/
- [x] T006 [P] Criar enum GradeStatus em Siaed.Domain/Enums/GradeStatus.cs se necessario para regras de estado do registro de nota
- [x] T007 Criar interface de repositorio IGradeRepository em Siaed.Application/Interfaces/IGradeRepository.cs com operacoes de CRUD e filtros por activityId/schoolClassId/teacherId/gradeValue
- [x] T008 Implementar mapeamento EF da tabela Grades em Siaed.Infra/Persistence/Configurations/GradeConfiguration.cs com indice unico por ActivityId+StudentId+DeletedAt e indices de filtro
- [x] T009 Atualizar contexto de dados em Siaed.Infra/Persistence/AppDbContext.cs adicionando DbSet<Grade> e configuracao da entidade
- [x] T010 Implementar GradeRepository em Siaed.Infra/Repositories/GradeRepository.cs com filtros paginados e suporte a concorrencia otimista
- [x] T011 Atualizar injecao de dependencia em Siaed.Infra/DependencyInjection/InfraServiceExtensions.cs registrando IGradeRepository -> GradeRepository
- [x] T012 Criar migration da tabela independente Grades em Siaed.Infra/Migrations/ com RowVersion e soft delete
- [x] T013 Atualizar entidade Activity para suportar contexto de turma/convensao da atividade em Siaed.Domain/Entities/Activity.cs e refletir no fluxo de criacao/atualizacao em Siaed.Application/Features/Activities/

**Checkpoint**: Fundacao concluida - historias podem iniciar

---

## Phase 3: User Story 1 - Lancar notas da turma na atividade (Priority: P1) MVP

**Goal**: Permitir lancamento de notas (string) por aluno em atividade com autorizacao correta

**Independent Test**: Criar notas para alunos de uma atividade via endpoint de create e visualizar a subsecao da atividade com notas registradas e pendencias

- [x] T014 [US1] Criar comando CreateGradeCommand em Siaed.Application/Features/Grades/Commands/CreateGradeCommand.cs
- [x] T015 [P] [US1] Criar validator CreateGradeCommandValidator em Siaed.Application/Features/Grades/Validators/CreateGradeCommandValidator.cs para validar GradeValue string e ConventionKey
- [x] T016 [US1] Implementar CreateGradeHandler em Siaed.Application/Features/Grades/Handlers/CreateGradeHandler.cs com validacao de atividade editavel e autorizacao professor-da-turma/coordenador
- [x] T017 [US1] Criar DTO de resposta GradeDto em Siaed.Application/Features/Grades/DTOs/GradeDto.cs
- [x] T018 [US1] Implementar endpoint POST /api/v1/grades em Siaed.Api/Controllers/GradesController.cs delegando via IMediator
- [x] T019 [US1] Criar query de subsecao da atividade GetActivityGradesQuery em Siaed.Application/Features/Grades/Queries/GetActivityGradesQuery.cs
- [x] T020 [US1] Implementar handler GetActivityGradesHandler em Siaed.Application/Features/Grades/Handlers/GetActivityGradesHandler.cs preservando historico de alunos que sairam e incluindo novos alunos sem nota
- [x] T021 [US1] Expor endpoint GET /api/v1/activities/{activityId}/grades em Siaed.Api/Controllers/ActivitiesController.cs

**Checkpoint**: US1 funcional e demonstravel (lancamento e subsecao)

---

## Phase 4: User Story 2 - Consultar notas ja lancadas (Priority: P2)

**Goal**: Consultar notas com filtros por turma, professor e nota em listagem paginada

**Independent Test**: Consultar notas por GET /api/v1/grades usando filtros combinados e validar retorno paginado correto

- [x] T022 [US2] Criar query GetGradeByIdQuery em Siaed.Application/Features/Grades/Queries/GetGradeByIdQuery.cs
- [x] T023 [P] [US2] Criar query ListGradesQuery em Siaed.Application/Features/Grades/Queries/ListGradesQuery.cs com filtros activityId, schoolClassId, teacherId e gradeValue
- [x] T024 [US2] Implementar GetGradeByIdHandler em Siaed.Application/Features/Grades/Handlers/GetGradeByIdHandler.cs
- [x] T025 [US2] Implementar ListGradesHandler em Siaed.Application/Features/Grades/Handlers/ListGradesHandler.cs retornando PagedResult<GradeListItemDto>
- [x] T026 [P] [US2] Criar DTO GradeListItemDto em Siaed.Application/Features/Grades/DTOs/GradeListItemDto.cs
- [x] T027 [US2] Criar validator ListGradesQueryValidator em Siaed.Application/Features/Grades/Validators/ListGradesQueryValidator.cs
- [x] T028 [US2] Expor endpoint GET /api/v1/grades/{id} em Siaed.Api/Controllers/GradesController.cs
- [x] T029 [US2] Expor endpoint GET /api/v1/grades com filtros de turma/professor/nota em Siaed.Api/Controllers/GradesController.cs

**Checkpoint**: US2 funcional e testavel de forma independente

---

## Phase 5: User Story 3 - Atualizar notas antes do fechamento (Priority: P3)

**Goal**: Atualizar e excluir logicamente notas com controle de concorrencia e bloqueio por status da atividade

**Independent Test**: Atualizar nota com RowVersion valido e receber conflito ao reutilizar versao antiga; excluir nota via soft delete sem perda de historico auditavel

- [x] T030 [US3] Criar comando UpdateGradeCommand em Siaed.Application/Features/Grades/Commands/UpdateGradeCommand.cs com campo Version
- [x] T031 [P] [US3] Criar validator UpdateGradeCommandValidator em Siaed.Application/Features/Grades/Validators/UpdateGradeCommandValidator.cs
- [x] T032 [US3] Implementar UpdateGradeHandler em Siaed.Application/Features/Grades/Handlers/UpdateGradeHandler.cs com concorrencia otimista e bloqueio para atividade fechada
- [x] T033 [US3] Expor endpoint PUT /api/v1/grades/{id} em Siaed.Api/Controllers/GradesController.cs retornando conflito 409 quando versao estiver desatualizada
- [x] T034 [US3] Criar comando DeleteGradeCommand em Siaed.Application/Features/Grades/Commands/DeleteGradeCommand.cs para exclusao logica
- [x] T035 [US3] Implementar DeleteGradeHandler em Siaed.Application/Features/Grades/Handlers/DeleteGradeHandler.cs com soft delete e auditoria
- [x] T036 [US3] Expor endpoint DELETE /api/v1/grades/{id} em Siaed.Api/Controllers/GradesController.cs

**Checkpoint**: US3 funcional e sem sobrescrita silenciosa

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Ajustes finais, documentacao e validacao ponta a ponta

- [x] T037 [P] Atualizar contratos finais da feature em specs/003-gerenciar-notas-atividades/contracts/api-contracts.md conforme implementacao final
- [x] T038 [P] Atualizar guia de execucao em specs/003-gerenciar-notas-atividades/quickstart.md com comandos e exemplos de requisicao reais
- [x] T039 Ajustar documentacao de arquitetura para nova entidade Grade em docs/backend-state.md
- [ ] T040 Executar validacao manual do fluxo CRUD + filtros seguindo specs/003-gerenciar-notas-atividades/quickstart.md e registrar resultado em specs/003-gerenciar-notas-atividades/research.md

---

## Dependencies & Execution Order

### Phase Dependencies

- Phase 1: sem dependencias
- Phase 2: depende da Phase 1 e bloqueia todas as historias
- Phase 3 a Phase 5: dependem da conclusao da Phase 2
- Phase 6: depende da conclusao das historias que forem para entrega

### User Story Dependencies

- US1 (P1): inicia apos Phase 2, sem dependencia de outras historias
- US2 (P2): inicia apos Phase 2, independente de US1
- US3 (P3): inicia apos Phase 2, independente de US1/US2 para valor minimo

### Within Each User Story

- Commands/Queries antes de Handlers
- Handlers antes de endpoints
- Validators e DTOs no inicio da historia quando aplicavel
- Story so finaliza apos checkpoint da propria fase

### Parallel Opportunities

- Phase 2: T005 e T006 podem rodar em paralelo; T008 e T010 podem ser divididas entre pessoas diferentes apos T004/T007
- US1: T015 e T017 em paralelo; T019 pode iniciar em paralelo com T018
- US2: T023 e T026 em paralelo; T024 e T025 em paralelo apos criacao de queries
- US3: T031 e T034 em paralelo
- Phase 6: T037 e T038 em paralelo

---

## Parallel Example: User Story 1

- T015 [US1] Criar validator CreateGradeCommandValidator em Siaed.Application/Features/Grades/Validators/CreateGradeCommandValidator.cs
- T017 [US1] Criar DTO de resposta GradeDto em Siaed.Application/Features/Grades/DTOs/GradeDto.cs

Depois da finalizacao destes itens:

- T018 [US1] Implementar endpoint POST /api/v1/grades em Siaed.Api/Controllers/GradesController.cs
- T019 [US1] Criar query de subsecao da atividade GetActivityGradesQuery em Siaed.Application/Features/Grades/Queries/GetActivityGradesQuery.cs

---

## Parallel Example: User Story 2

- T023 [US2] Criar query ListGradesQuery em Siaed.Application/Features/Grades/Queries/ListGradesQuery.cs com filtros activityId, schoolClassId, teacherId e gradeValue
- T026 [US2] Criar DTO GradeListItemDto em Siaed.Application/Features/Grades/DTOs/GradeListItemDto.cs

Depois da finalizacao destes itens:

- T025 [US2] Implementar ListGradesHandler em Siaed.Application/Features/Grades/Handlers/ListGradesHandler.cs retornando PagedResult<GradeListItemDto>
- T029 [US2] Expor endpoint GET /api/v1/grades com filtros de turma/professor/nota em Siaed.Api/Controllers/GradesController.cs

---

## Parallel Example: User Story 3

- T031 [US3] Criar validator UpdateGradeCommandValidator em Siaed.Application/Features/Grades/Validators/UpdateGradeCommandValidator.cs
- T034 [US3] Criar comando DeleteGradeCommand em Siaed.Application/Features/Grades/Commands/DeleteGradeCommand.cs para exclusao logica

Depois da finalizacao destes itens:

- T032 [US3] Implementar UpdateGradeHandler em Siaed.Application/Features/Grades/Handlers/UpdateGradeHandler.cs com concorrencia otimista e bloqueio para atividade fechada
- T035 [US3] Implementar DeleteGradeHandler em Siaed.Application/Features/Grades/Handlers/DeleteGradeHandler.cs com soft delete e auditoria

---

## Implementation Strategy

### MVP First (US1)

1. Concluir Phase 1 e Phase 2
2. Entregar Phase 3 (US1)
3. Validar fluxo de lancamento + subsecao por atividade
4. Liberar MVP

### Incremental Delivery

1. MVP (US1)
2. Incremento de consulta com filtros (US2)
3. Incremento de atualizacao e exclusao com concorrencia (US3)
4. Finalizacao com Phase 6

### Parallel Team Strategy

1. Time A: dominio + persistencia (Phase 2)
2. Time B: endpoints e handlers de US1
3. Time C: consultas e filtros de US2
4. Time D: concorrencia e delete logico de US3

---

## Notes

- Todas as tasks seguem formato checklist obrigatorio com ID sequencial
- Marcacao [P] indica potencial de execucao paralela
- Marcacao [USx] aparece somente em fases de user story
- Cada historia possui criterio de teste independente no cabecalho da fase
