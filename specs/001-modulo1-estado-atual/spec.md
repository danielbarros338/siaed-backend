# Feature Specification: Módulo 1 — Estado Atual do Assistente IA para Professores

**Feature Branch**: `001-modulo1-estado-atual`

**Created**: 2025-07-17

**Status**: Documentação de Estado Real

---

> **Propósito deste documento**: Registrar fielmente o que está implementado no Módulo 1 do SIAED Backend, identificar lacunas em relação à especificação original, e orientar as próximas etapas sem propor reestruturação arquitetural. A arquitetura (Clean Architecture + DDD + CQRS + MediatR) está correta e não deve ser questionada.

---

## Escopo do Módulo 1

O Módulo 1 é o **Assistente de IA para Professores**. Seu objetivo é reduzir o tempo de trabalho administrativo e pedagógico do professor por meio de:

- Geração automatizada de planos de aula com IA
- Geração automatizada de atividades pedagógicas com IA
- Geração automatizada de relatórios pedagógicos com IA
- Criação manual dos mesmos recursos (quando o professor prefere não usar IA)
- Rastreamento completo de uso da IA (auditoria + controle de custo)
- Autenticação segura de professores, diretores e coordenadores

---

## O que foi implementado

### Autenticação e Autorização

| Funcionalidade | Estado |
|---|---|
| Registro de usuário (`POST /api/v1/auth/register`) | ✅ Implementado |
| Login com retorno de JWT (`POST /api/v1/auth/login`) | ✅ Implementado |
| Roles: `Professor`, `Diretor`, `Coordenador` | ✅ Implementado |
| `[Authorize]` em todos os controllers de recurso | ✅ Implementado |
| Hash de senha (via `IPasswordHasher`) | ✅ Implementado |
| Geração de token JWT (via `IJwtService`) | ✅ Implementado |
| Validação de login/registro com FluentValidation | ✅ Implementado |

### Planos de Aula (`/api/v1/lessonplans`)

| Endpoint | Operação | Estado |
|---|---|---|
| `POST /api/v1/lessonplans` | Criar plano manual | ✅ Implementado |
| `POST /api/v1/lessonplans/generate` | Gerar plano com IA | ✅ Implementado |
| `GET /api/v1/lessonplans/{id}` | Buscar por ID | ✅ Implementado |
| `GET /api/v1/lessonplans?teacherId=&page=&pageSize=` | Listar paginado | ✅ Implementado |
| `PUT /api/v1/lessonplans/{id}` | Atualizar | ✅ Implementado |
| `DELETE /api/v1/lessonplans/{id}` | Excluir (soft delete) | ❌ Não implementado |
| `PATCH /api/v1/lessonplans/{id}/publish` | Publicar plano | ❌ Não implementado |
| `PATCH /api/v1/lessonplans/{id}/archive` | Arquivar plano | ❌ Não implementado |

### Atividades (`/api/v1/activities`)

| Endpoint | Operação | Estado |
|---|---|---|
| `POST /api/v1/activities` | Criar atividade manual | ✅ Implementado |
| `POST /api/v1/activities/generate` | Gerar atividade com IA | ✅ Implementado |
| `GET /api/v1/activities/{id}` | Buscar por ID | ✅ Implementado |
| `GET /api/v1/activities?teacherId=&page=&pageSize=` | Listar paginado | ✅ Implementado |
| `PUT /api/v1/activities/{id}` | Atualizar | ✅ Implementado |
| `DELETE /api/v1/activities/{id}` | Excluir (soft delete) | ❌ Não implementado |
| `PATCH /api/v1/activities/{id}/publish` | Publicar atividade | ❌ Não implementado |
| `PATCH /api/v1/activities/{id}/archive` | Arquivar atividade | ❌ Não implementado |

### Relatórios Pedagógicos (`/api/v1/reports`)

| Endpoint | Operação | Estado |
|---|---|---|
| `POST /api/v1/reports` | Criar relatório manual | ✅ Implementado |
| `POST /api/v1/reports/generate` | Gerar relatório com IA | ✅ Implementado |
| `GET /api/v1/reports/{id}` | Buscar por ID | ✅ Implementado |
| `GET /api/v1/reports?teacherId=&page=&pageSize=` | Listar paginado | ✅ Implementado |
| `PUT /api/v1/reports/{id}` | Atualizar | ✅ Implementado |
| `DELETE /api/v1/reports/{id}` | Excluir (soft delete) | ❌ Não implementado |
| Gerar resumo para relatório | Complementar relatório com resumo IA | ❌ Não implementado |
| Gerar comunicado para pais | Gerar comunicado com base no relatório | ❌ Não implementado |

### Professores (Teacher)

| Funcionalidade | Estado |
|---|---|
| Entidade `Teacher` no domínio | ✅ Implementado |
| Repositório `TeacherRepository` na Infra | ✅ Implementado |
| Interface `ITeacherRepository` na Application | ✅ Implementado |
| CRUD de professores via API | ❌ Sem controller nem features |
| Associação professor-usuário após registro | ❌ Não implementado |

### Infraestrutura de IA

| Componente | Estado |
|---|---|
| `IOpenAIService` (contrato) | ✅ Implementado |
| `OpenAIService` com `gpt-4o-mini` e cálculo de custo | ✅ Implementado |
| `IPromptBuilder` (contrato com 6 métodos) | ✅ Implementado |
| `PromptBuilderService` — prompt para plano de aula | ✅ Implementado |
| `PromptBuilderService` — prompt para atividade | ✅ Implementado |
| `PromptBuilderService` — prompt para relatório | ✅ Implementado |
| `PromptBuilderService` — prompt de sumarização | ✅ Implementado (contrato+impl, sem handler) |
| `PromptBuilderService` — prompt de reformulação de texto | ✅ Implementado (contrato+impl, sem handler) |
| `PromptBuilderService` — prompt de comunicado para pais | ✅ Implementado (contrato+impl, sem handler) |

### Auditoria de IA

| Componente | Estado |
|---|---|
| Entidade `AIRequest` (ciclo: Pending → Processing → Completed/Failed) | ✅ Implementado |
| Entidade `AIResponse` (conteúdo + tokens + modelo) | ✅ Implementado |
| `IAIRequestRepository` + `AIRequestRepository` | ✅ Implementado |
| Registro de AIRequest para cada chamada nos handlers | ✅ Implementado |
| Registro de AIResponse com tokens e custo por chamada | ✅ Implementado |
| Endpoints para consultar histórico de uso de IA | ❌ Não implementado |

### Fundação Técnica

| Componente | Estado |
|---|---|
| Clean Architecture em 4 projetos (Domain / Application / Infra / Api) | ✅ Implementado |
| Result Pattern (`Result<T>`, `Result`) | ✅ Implementado |
| `PagedResult<T>` para paginação | ✅ Implementado |
| `ValidationBehavior` (MediatR pipeline + FluentValidation) | ✅ Implementado |
| `LoggingBehavior` (MediatR pipeline com Serilog) | ✅ Implementado |
| `ExceptionHandlingMiddleware` | ✅ Implementado |
| Soft delete com `DeletedAt` + global query filter no EF Core | ✅ Implementado (verificado em `LessonPlanConfiguration`) |
| `SaveChangesAsync` override atualizando `UpdatedAt` automaticamente | ✅ Implementado |
| Migrations versionadas (MySQL) — todas as 7 tabelas | ✅ Implementado (1 migration) |
| Serilog (console + arquivo rolling) | ✅ Implementado |
| JWT Bearer com roles | ✅ Implementado |

---

## Lacunas identificadas

### L-001 — Ausência de endpoints de exclusão

**Impacto**: Alto
**Descrição**: Os três controllers principais (`LessonPlansController`, `ActivitiesController`, `ReportsController`) não possuem endpoints `DELETE`. As entidades de domínio já possuem o método `Delete()` e o soft delete está configurado no banco, mas não há `DeleteLessonPlanCommand`, `DeleteActivityCommand` nem `DeleteReportCommand` na camada Application.

### L-002 — Ausência de transições de status (Publish/Archive)

**Impacto**: Médio
**Descrição**: `LessonPlan` e `Activity` possuem estados `Draft`, `Published`, `Archived` e os métodos de domínio `Publish()` e `Archive()`. Não existem, porém, commands, handlers ou endpoints para essas transições de estado.

### L-003 — Ausência de validadores para Generate e Update

**Impacto**: Médio
**Descrição**: Faltam validadores FluentValidation para os seguintes commands:
- `GenerateActivityCommandValidator` (existe apenas `CreateActivityCommandValidator`)
- `GenerateReportCommandValidator` (existe apenas `CreateReportCommandValidator`)
- `UpdateLessonPlanCommandValidator`
- `UpdateActivityCommandValidator`
- `UpdateReportCommandValidator`

### L-004 — Teacher sem gestão via API

**Impacto**: Alto
**Descrição**: A entidade `Teacher` existe no domínio com repositório na Infra e interface na Application, mas não há nenhuma feature de Application nem controller para criar, atualizar ou listar professores.

### L-005 — Associação User ↔ Teacher não formalizada

**Impacto**: Alto
**Descrição**: `User` (autenticação) e `Teacher` (pedagógico) são entidades separadas. Após o `RegisterCommand`, um `User` é criado, mas nenhum `Teacher` correspondente é gerado automaticamente. Não há campo `UserId` em `Teacher`.

### L-006 — IA de sumarização, reformulação e comunicado sem handlers nem endpoints

**Impacto**: Médio
**Descrição**: `AIRequestType` define os tipos `Summarization (4)`, `TextReformulation (5)` e `ParentCommunication (6)`. `IPromptBuilder` possui os métodos correspondentes e `PromptBuilderService` os implementa. No entanto, não há features de Application nem endpoints de API para essas funcionalidades.

### L-007 — Sem filtros avançados nas queries de listagem

**Impacto**: Baixo
**Descrição**: As queries `ListLessonPlansQuery`, `ListActivitiesQuery` e `ListReportsQuery` filtram apenas por `teacherId`. Não há filtros por `Status`, `IsAIGenerated`, `Type` (Activity) ou intervalo de datas.

### L-008 — Sem endpoints para histórico de uso de IA

**Impacto**: Baixo
**Descrição**: `AIRequest` e `AIResponse` são persistidos com todos os metadados (tokens, custo, modelo, status), mas não há query, handler ou endpoint para consultar esse histórico.

### L-009 — Global query filter de soft delete não verificado em Activity e Report

**Impacto**: Médio
**Descrição**: Confirmado que `LessonPlanConfiguration` usa `HasQueryFilter(l => l.DeletedAt == null)`. As configurações de `Activity`, `PedagogicalReport` e `Teacher` precisam de confirmação de que o mesmo filtro está aplicado.

### L-010 — Inconsistência no formato de resposta da IA

**Impacto**: Baixo
**Descrição**: O handler de geração de plano de aula solicita resposta JSON estruturada e faz desserialização. Os handlers de atividade e relatório recebem texto em prosa (sem JSON estruturado), armazenando o conteúdo diretamente em `AIResponse`. Isso torna a consulta programática ao conteúdo inconsistente entre entidades.

---

## User Scenarios & Testing

### User Story 1 — Professor gera plano de aula com IA (Priority: P1)

Um professor autenticado informa disciplina, série, faixa etária e duração. O sistema envia para a IA e retorna um plano estruturado com título, objetivos, conteúdo, metodologia, recursos e avaliação.

**Why this priority**: É o fluxo central e diferencial do Módulo 1. Já está implementado de ponta a ponta.

**Independent Test**: Autenticar, `POST /api/v1/lessonplans/generate` com payload válido, verificar retorno `201` com ID do plano criado e confirmar que AIRequest foi persistido como `Completed`.

**Acceptance Scenarios**:

1. **Dado** professor autenticado com token válido, **Quando** envia payload válido para geração de plano, **Então** recebe `201 Created` com `id` do plano gerado.
2. **Dado** payload inválido (campos obrigatórios ausentes), **Quando** envia para geração, **Então** recebe `400 Bad Request` com lista de erros de validação em português.
3. **Dado** falha na API da OpenAI, **Quando** envia para geração, **Então** recebe `400 Bad Request` com mensagem de erro e o AIRequest é persistido como `Failed`.
4. **Dado** plano gerado com sucesso, **Quando** consulta o AIRequest associado no banco, **Então** encontra tokens consumidos, custo estimado e status `Completed`.

---

### User Story 2 — Professor consulta e edita seus planos de aula (Priority: P1)

Um professor autenticado pode listar todos os seus planos com paginação, buscar um específico por ID e atualizar seu conteúdo.

**Why this priority**: Sem consulta e edição, o conteúdo gerado não tem utilidade prática.

**Independent Test**: Após criar plano, `GET /api/v1/lessonplans?teacherId={id}&page=1&pageSize=10` retorna lista paginada; `GET /api/v1/lessonplans/{id}` retorna o plano; `PUT /api/v1/lessonplans/{id}` retorna `204`.

**Acceptance Scenarios**:

1. **Dado** professor com planos cadastrados, **Quando** lista com `teacherId` válido e paginação, **Então** recebe `PagedResult` com items, totalCount e metadados de página.
2. **Dado** ID válido de plano existente, **Quando** faz GET por ID, **Então** recebe todos os campos do plano incluindo `isAIGenerated` e `status`.
3. **Dado** ID inexistente ou de outro professor, **Quando** faz GET por ID, **Então** recebe `404 Not Found`.
4. **Dado** plano existente, **Quando** atualiza com payload válido, **Então** recebe `204 No Content` e campos alterados persistidos.

---

### User Story 3 — Professor gera atividade pedagógica com IA (Priority: P1)

Um professor autenticado informa disciplina, série, faixa etária e tipo de atividade. O sistema retorna uma atividade completa com conteúdo e gabarito.

**Why this priority**: Segundo fluxo central do módulo, já implementado de ponta a ponta.

**Independent Test**: `POST /api/v1/activities/generate` com payload válido retorna `201` com ID da atividade.

**Acceptance Scenarios**:

1. **Dado** professor autenticado, **Quando** gera atividade com tipo `Quiz`, **Então** recebe `201 Created` com `id` e a atividade é marcada como `IsAIGenerated = true`.
2. **Dado** falha na IA, **Quando** gera atividade, **Então** recebe erro e AIRequest é persistido com status `Failed`.

---

### User Story 4 — Professor gera relatório pedagógico com IA (Priority: P1)

Um professor autenticado informa nome do aluno, série, período e informações de desempenho. A IA gera um relatório completo em linguagem empática e profissional.

**Why this priority**: Terceiro fluxo central do módulo, já implementado de ponta a ponta.

**Independent Test**: `POST /api/v1/reports/generate` com payload válido retorna `201` com ID do relatório.

**Acceptance Scenarios**:

1. **Dado** professor autenticado, **Quando** gera relatório com dados do aluno, **Então** recebe relatório com conteúdo em português e linguagem pedagógica.
2. **Dado** falha na IA, **Quando** gera relatório, **Então** recebe erro e AIRequest é persistido como `Failed`.

---

### User Story 5 — Professor exclui recursos que não quer mais manter (Priority: P2)

Um professor pode excluir planos de aula, atividades e relatórios. A exclusão é lógica (soft delete) e o registro permanece no banco para auditoria.

**Why this priority**: CRUD básico esperado; domínio já suporta (`Delete()`), falta implementação nas camadas superiores (L-001).

**Independent Test**: `DELETE /api/v1/lessonplans/{id}` retorna `204` e o plano não aparece na listagem subsequente.

**Acceptance Scenarios**:

1. **Dado** plano existente do professor, **Quando** professor exclui, **Então** recebe `204` e o plano não aparece na próxima listagem.
2. **Dado** ID inexistente, **Quando** professor tenta excluir, **Então** recebe `404`.
3. **Dado** plano excluído, **Quando** professor busca por ID, **Então** recebe `404`.

---

### User Story 6 — Professor transiciona status de plano de aula ou atividade (Priority: P3)

Um professor pode publicar um rascunho para torná-lo disponível ou arquivá-lo quando não for mais relevante.

**Why this priority**: O domínio já suporta (`Publish()`, `Archive()`), mas os endpoints ainda não existem (L-002).

**Independent Test**: `PATCH /api/v1/lessonplans/{id}/publish` muda status de `Draft` para `Published` e retorna `204`.

**Acceptance Scenarios**:

1. **Dado** plano no status `Draft`, **Quando** professor publica, **Então** status muda para `Published`.
2. **Dado** plano no status `Published`, **Quando** professor arquiva, **Então** status muda para `Archived`.
3. **Dado** plano no status `Archived`, **Quando** professor tenta publicar novamente, **Então** recebe `400` com mensagem de transição inválida.

---

### Edge Cases

- O que acontece quando a API da OpenAI retorna resposta fora do formato JSON esperado para planos de aula? (Handler usa desserialização com fallback parcial)
- O que acontece quando `teacherId` passado nas queries não tem nenhum registro? (Deve retornar `PagedResult` vazio, não `404`)
- O que acontece quando token JWT expirado acessa endpoint protegido? (`401 Unauthorized` via middleware do ASP.NET Core)
- O que acontece com atividades que referenciam um plano de aula excluído? (`Activity.LessonPlanId` é nullable; sem FK constraint blocking)
- O que acontece se dois requests simultâneos tentam processar o mesmo AIRequest? (Concorrência otimista via EF Core deve rejeitar o segundo)

---

## Requirements

### Functional Requirements — Já Implementados

- **FR-001**: O sistema DEVE autenticar professores, diretores e coordenadores via JWT Bearer com roles
- **FR-002**: O sistema DEVE permitir geração de plano de aula via IA informando disciplina, série, faixa etária e duração
- **FR-003**: O sistema DEVE permitir geração de atividade pedagógica via IA com tipo e configurações básicas
- **FR-004**: O sistema DEVE permitir geração de relatório pedagógico via IA a partir de dados de desempenho do aluno
- **FR-005**: O sistema DEVE permitir criação manual de planos de aula, atividades e relatórios sem uso de IA
- **FR-006**: O sistema DEVE paginar os resultados de listagem de recursos por professor
- **FR-007**: O sistema DEVE registrar cada chamada à IA com modelo, tokens consumidos e custo estimado
- **FR-008**: O sistema DEVE registrar a resposta da IA com conteúdo completo e status de conclusão
- **FR-009**: O sistema DEVE aplicar soft delete em todos os recursos pedagógicos (campo `DeletedAt`)
- **FR-010**: O sistema DEVE validar entradas de commands via FluentValidation antes de executar handlers
- **FR-011**: A IA DEVE responder sempre em português brasileiro com linguagem pedagógica
- **FR-012**: O sistema DEVE rejeitar toda exceção não tratada antes de chegar ao cliente (Result Pattern + middleware)

### Functional Requirements — Pendentes de Implementação

- **FR-013**: O sistema DEVE permitir exclusão (soft delete) de planos, atividades e relatórios via endpoint DELETE (requer L-001)
- **FR-014**: O sistema DEVE permitir transição de status `Draft → Published` e `Published → Archived` para planos e atividades (requer L-002)
- **FR-015**: O sistema DEVE validar via FluentValidation todos os commands `Generate` e `Update` (requer L-003)
- **FR-016**: O sistema DEVE ter API de gestão de professores (criar, buscar, atualizar) (requer L-004)
- **FR-017**: O sistema DEVE criar automaticamente um `Teacher` ao registrar um `User` com role `Professor` (requer L-005)
- **FR-018**: O sistema DEVE permitir sumarização de textos pedagógicos via IA (requer L-006)
- **FR-019**: O sistema DEVE permitir reformulação de texto adequada à faixa etária via IA (requer L-006)
- **FR-020**: O sistema DEVE permitir geração de comunicados para pais com base em relatórios via IA (requer L-006)
- **FR-021**: O sistema DEVE aplicar global query filter de soft delete em `Activity` e `PedagogicalReport` (confirmar L-009)

### Key Entities

- **User**: Identidade de autenticação. Atributos: `Id`, `Name`, `Email`, `PasswordHash`, `Role`, `CreatedAt`
- **Teacher**: Ator pedagógico. Atributos: `Id`, `Name`, `Email`, `Subject`, `SchoolId`. Sem vínculo formal com `User` (lacuna L-005)
- **LessonPlan**: Plano de aula (manual ou IA). Atributos principais: `TeacherId`, `Title`, `Subject`, `Grade`, `AgeRange`, `DurationMinutes`, `Objectives`, `Content`, `Methodology`, `Resources`, `Evaluation`, `IsAIGenerated`, `Status`
- **Activity**: Atividade pedagógica (manual ou IA). Atributos: `TeacherId`, `LessonPlanId?`, `Title`, `Subject`, `Grade`, `AgeRange`, `Content`, `AnswerKey`, `SimplifiedVersion`, `Type`, `IsAIGenerated`, `Status`
- **PedagogicalReport**: Relatório por aluno. Atributos: `TeacherId`, `StudentName`, `Grade`, `Period`, `Content`, `Summary`, `ParentCommunication`, `IsAIGenerated`
- **AIRequest**: Auditoria de chamada à IA. Atributos: `TeacherId`, `RequestType`, `Prompt`, `InputData`, `Model`, `MaxTokens`, `TokensUsed`, `EstimatedCost`, `Status`
- **AIResponse**: Resposta da IA vinculada ao AIRequest. Atributos: `AIRequestId`, `Content`, `FinishReason`, `TotalTokens`, `Model`

---

## Success Criteria

### Critérios Mensuráveis

- **SC-001**: Professor consegue gerar um plano de aula completo em menos de 30 segundos após envio da requisição
- **SC-002**: Professor consegue gerar uma atividade com gabarito em menos de 30 segundos após envio
- **SC-003**: 100% das chamadas à IA resultam em registro de custo e tokens no banco de dados
- **SC-004**: Recursos com `DeletedAt` preenchido não aparecem em nenhuma listagem (soft delete efetivo via global query filter)
- **SC-005**: Todas as entradas inválidas são rejeitadas com mensagens de erro em português antes de chegar aos handlers
- **SC-006**: Nenhuma exceção não tratada chega ao cliente — todos os erros retornam estruturados via Result Pattern

### Critérios Pendentes (vinculados às lacunas)

- **SC-007**: Professor consegue excluir qualquer recurso seu e confirmá-lo ausente na listagem subsequente (requer L-001)
- **SC-008**: Professor consegue transitar plano de `Draft` para `Published` via endpoint dedicado (requer L-002)
- **SC-009**: 100% dos commands `Generate` e `Update` têm validação FluentValidation explícita (requer L-003)

---

## Assumptions

- O banco de dados MySQL está acessível e migrado antes de qualquer execução ou teste
- A chave de API da OpenAI está configurada via `appsettings.json` (seção `OpenAI:ApiKey`) ou variável de ambiente equivalente
- O modelo padrão da IA é `gpt-4o-mini`; a troca de modelo não é configurável por usuário
- A integração com ERP externo não faz parte do Módulo 1; dados de professor e aluno são informados diretamente via API
- A associação entre `User` e `Teacher` é feita manualmente por enquanto (lacuna L-005 a resolver)
- O global query filter de soft delete está confirmado para `LessonPlan`; as demais entidades (`Activity`, `PedagogicalReport`, `Teacher`) precisam de verificação (lacuna L-009)
- Paginação padrão: página 1, tamanho 10; sem limite máximo configurado explicitamente
- A IA não deve emitir diagnósticos médicos, laudos, inventar fatos ou substituir decisões pedagógicas humanas

---

## Próximas Etapas Naturais

Em ordem de prioridade, sem reestruturação arquitetural:

1. **Implementar endpoints DELETE** (L-001): `DeleteLessonPlanCommand`, `DeleteActivityCommand`, `DeleteReportCommand`
2. **Resolver associação User ↔ Teacher** (L-004 + L-005): criar `Teacher` automaticamente ao registrar `User` com role `Professor`
3. **Completar validadores ausentes** (L-003): 5 validators faltantes para commands `Generate` e `Update`
4. **Confirmar global query filters** (L-009): verificar `ActivityConfiguration` e `PedagogicalReportConfiguration`
5. **Implementar Publish/Archive** (L-002): commands e endpoints de transição de status
6. **IA auxiliar** (L-006): features de Application para sumarização, reformulação de texto e comunicados (prompt já está implementado)
7. **Filtros avançados nas listagens** (L-007): parâmetros opcionais de `status`, `isAIGenerated`, `type`
8. **Endpoints de histórico de IA** (L-008): query de `AIRequests` por professor com paginação
