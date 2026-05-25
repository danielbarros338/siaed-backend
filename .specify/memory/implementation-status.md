# SIAED Backend — Status de Implementação

**Referência**: `specs/001-modulo1-estado-atual/spec.md`
**Última verificação**: 2026-05-18
**Completude estimada do Módulo 1**: ~70%

---

## Módulo 1 — Assistente IA para Professores

### Implementado ✅

| Área | Componentes |
|------|-------------|
| **Auth** | `RegisterCommand` + `LoginCommand`, JWT Bearer, hash de senha via `IPasswordHasher`, Roles (`Professor`, `Diretor`, `Coordenador`), `[Authorize]` em todos os controllers de recurso |
| **LessonPlans** | Create (manual), Generate (IA), GetById, List (paginado por teacherId), Update |
| **Activities** | Create (manual), Generate (IA), GetById, List (paginado por teacherId), Update |
| **Reports** | Create (manual), Generate (IA), GetById, List (paginado por teacherId), Update |
| **IA — Infra** | `OpenAIService` com cálculo de custo por modelo; `PromptBuilderService` com 6 prompts implementados |
| **Auditoria IA** | `AIRequest`/`AIResponse` persistidos com tokens, custo estimado e ciclo de status por chamada |
| **Fundação técnica** | `Result<T>`, `PagedResult<T>`, `ValidationBehavior`, `LoggingBehavior`, `ExceptionHandlingMiddleware` |
| **Banco de dados** | 1 migration com 7 tabelas, soft delete com `global query filter` (confirmado em LessonPlan), `UpdatedAt` auto-atualizado via override de `SaveChangesAsync` |

> **Nota — Auth fora de spec**: A feature de autenticação foi implementada fora do feature branch `001-modulo1-estado-atual`. Não está mapeada em nenhum arquivo de spec. Funciona corretamente; apenas carece de documentação formal em spec dedicada.

---

## Lacunas Conhecidas

### L-001 — DELETE endpoints ausentes (Impacto: Alto)
Os controllers `LessonPlansController`, `ActivitiesController` e `ReportsController` não possuem endpoints `DELETE`.
As entidades de domínio já têm o método `Delete()` e o soft delete está configurado no banco.
**Falta**: `DeleteLessonPlanCommand`, `DeleteActivityCommand`, `DeleteReportCommand` + handlers + endpoints.

### L-002 — Transições de status Publish/Archive ausentes (Impacto: Médio)
`LessonPlan` e `Activity` têm estados `Draft`, `Published`, `Archived` e métodos `Publish()`/`Archive()` no domínio.
**Falta**: commands, handlers e endpoints `PATCH /{id}/publish` e `PATCH /{id}/archive`.

### L-003 — Validadores Generate e Update ausentes (Impacto: Médio)
Commands que chegam ao handler sem validação FluentValidation:
- `GenerateActivityCommandValidator` (existe apenas `CreateActivityCommandValidator`)
- `GenerateReportCommandValidator` (existe apenas `CreateReportCommandValidator`)
- `UpdateLessonPlanCommandValidator`
- `UpdateActivityCommandValidator`
- `UpdateReportCommandValidator`

### L-004 — Teacher sem gestão via API (Impacto: Alto)
`Teacher` existe no domínio com `ITeacherRepository` e `TeacherRepository`, mas não há features de Application nem controller para criar, listar ou atualizar professores.

### L-005 — User ↔ Teacher não associados (Impacto: Alto)
Após `RegisterCommand`, apenas `User` é criado. Não há `Teacher` correspondente criado automaticamente.
Não existe campo `UserId` em `Teacher`.
**Consequência**: professores autenticados não têm entidade pedagógica vinculada — `teacherId` nos planos/atividades/relatórios não pode ser derivado automaticamente do token JWT.

### L-006 — IA auxiliar sem handlers nem endpoints (Impacto: Médio)
`AIRequestType` define: `Summarization(4)`, `TextReformulation(5)`, `ParentCommunication(6)`.
`PromptBuilderService` implementa os 3 prompts correspondentes.
**Falta**: features de Application e endpoints de API para os 3 tipos auxiliares.

### L-007 — Filtros avançados ausentes nas queries de listagem (Impacto: Baixo)
`ListLessonPlansQuery`, `ListActivitiesQuery`, `ListReportsQuery` filtram apenas por `teacherId`.
Sem filtros por `Status`, `IsAIGenerated`, `Type` (Activity), ou intervalo de datas.

### L-008 — Histórico de uso de IA sem endpoint (Impacto: Baixo)
`AIRequest`/`AIResponse` são persistidos com todos os metadados (tokens, custo, modelo, status),
mas não há query, handler ou endpoint para consultar esse histórico via API.

### L-009 — Global query filter de soft delete não confirmado em Activity e Report (Impacto: Médio)
Confirmado em `LessonPlanConfiguration`. As configurações de `Activity`, `PedagogicalReport` e `Teacher`
requerem verificação de que `HasQueryFilter(e => e.DeletedAt == null)` está aplicado.

### L-010 — Inconsistência no formato de resposta da IA (Impacto: Baixo)
`GenerateLessonPlanCommandHandler` solicita JSON estruturado e faz parse/desserialização.
`GenerateActivityCommandHandler` e `GenerateReportCommandHandler` recebem texto em prosa e armazenam diretamente.
Inconsistência dificulta consulta programática ao conteúdo gerado.

---

## Divergências Arquiteturais

### D-001 — User/Teacher desacoplados (Impacto: Alto)
`User` (autenticação) e `Teacher` (pedagógico) são entidades sem FK entre si.
**Bloqueante**: qualquer feature que necessite derivar `teacherId` a partir de um usuário autenticado.
**Correção necessária**: adicionar `UserId (Guid)` em `Teacher` + criar `Teacher` automaticamente no `RegisterCommandHandler`.

### D-002 — Desserialização IA parcial (Impacto: Médio)
`GenerateLessonPlanCommandHandler` faz parse JSON com fallback parcial.
`GenerateActivityCommandHandler` e `GenerateReportCommandHandler` armazenam resposta em prosa.
Inconsistência entre os 3 handlers do mesmo padrão de geração.

### D-003 — Repositórios sem auditoria de interface completa (Impacto: Baixo)
Estrutura correta (interfaces em Application, implementações em Infra).
Requer verificação manual de que nenhum repositório usa `DbContext` diretamente sem passar pela interface.

### D-004 — Validadores ausentes permitem dados inválidos nos handlers (Impacto: Médio)
Ver L-003. Commands `Generate*` e `Update*` chegam ao handler sem filtragem FluentValidation
porque o `ValidationBehavior` depende de validators registrados.

### D-005 — Acesso cruzado entre professores: violação LGPD (Impacto: Alto — CRÍTICO)
`LessonPlansController`, `ActivitiesController` e `ReportsController` nos endpoints `GetById` e `Update`
não verificam se o recurso solicitado pertence ao professor autenticado.
Qualquer professor com token válido pode ler ou modificar dados de outro professor.
**Correção necessária**: nos handlers `GetById` e `Update`, comparar `entity.TeacherId` com o `teacherId`
extraído do token JWT; retornar `Result.Failure("Recurso não encontrado")` em caso de divergência.
**Não deve ser deployado em produção sem esta correção.**

---

## Integrações Pendentes

### WhatsApp / Mensageria (Status: NÃO INICIADO — 0% implementado)
- Nenhuma referência a WhatsApp, Twilio, SMS, push notification ou canal de envio no codebase
- `AGENTS.md` reserva a pasta `Siaed.Infra/Messaging/` para implementação futura
- A feature de "comunicado para pais" existe apenas como geração de texto (`PedagogicalReport.ParentCommunication`) sem canal de envio automatizado
- **Requer spec dedicada antes de qualquer implementação**

---

## Banco de Dados

| Migration | Data criação | Tabelas |
|-----------|-------------|---------|
| `20260515192120_AddUser` | 2026-05-15 | `Users`, `Teachers`, `LessonPlans`, `Activities`, `PedagogicalReports`, `AIRequests`, `AIResponses` |

**Alertas de performance e integridade**:
- Índices ausentes em `TeacherId`, `Status`, `IsAIGenerated` — risco de performance em produção com volume alto
- FK constraints ausentes na migration (`AIResponse → AIRequest`, `Activity → LessonPlan`) — risco de dados órfãos
- Charset `utf8mb4` em todas as tabelas ✅

---

## Comandos de Build e Migrations

```powershell
# Restaurar dependências
dotnet restore

# Build
dotnet build

# Executar API (desenvolvimento)
dotnet run --project Siaed.Api

# Adicionar migration
dotnet ef migrations add <NomeDaMigration> --project Siaed.Infra --startup-project Siaed.Api

# Aplicar migrations
dotnet ef database update --project Siaed.Infra --startup-project Siaed.Api
```
