# Research: Módulo 1 — Completar sem Refatoração Destrutiva

**Branch**: `001-modulo1-estado-atual` | **Data**: 2025-07-17

---

## Resumo

Todas as incertezas técnicas foram resolvidas via análise direta do codebase. Nenhum item permanece como NEEDS CLARIFICATION. As descobertas abaixo fundamentam as decisões de design do `plan.md`.

---

## Descoberta 1 — Soft delete filters (L-009 encerrada)

**Decisão**: L-009 está **resolvida** — não é uma lacuna real.

**Evidência**: `HasQueryFilter(x => x.DeletedAt == null)` está aplicado em `ActivityConfiguration`, `PedagogicalReportConfiguration`, `TeacherConfiguration`, `UserConfiguration` e `LessonPlanConfiguration`. Todas as entidades de domínio têm o filtro correto.

**Impacto**: Remover L-009 do backlog. Nenhuma ação necessária.

---

## Descoberta 2 — Associação User↔Teacher (D-001 / L-005)

**Decisão**: Adicionar `UserId (Guid)` à entidade `Teacher`. Atualizar `RegisterCommand` para aceitar campos pedagógicos do Teacher e criar automaticamente o `Teacher` correspondente na mesma transação.

**Racional**:
- `Teacher` e `User` são entidades separadas com responsabilidades distintas (autenticação vs. domínio pedagógico).
- Vincular via `UserId` na entidade `Teacher` é o padrão correto (não o inverso). O `User` não deve depender do domínio pedagógico.
- A criação automática no `RegisterCommandHandler` é a abordagem menos destrutiva: uma única transação, sem novo endpoint.
- O JWT já carrega `sub` (UserId); os handlers podem obter o `TeacherId` via `ITeacherRepository.GetByUserIdAsync(userId)`.

**Alternativas consideradas**:
- *Criar Teacher separado via endpoint `/api/v1/teachers/link`*: Rejeitada — exigiria dois passos para o usuário e deixaria estado inconsistente (User sem Teacher).
- *Adicionar `TeacherId` ao JWT*: Rejeitada — aumenta acoplamento Auth↔Domínio; o JWT deve carregar apenas dados de identidade.
- *Colocar `UserId` em `User`*: Rejeitada — viola a direção correta de dependência; `User` não deve saber nada de pedagógico.

**Implementação**:
1. Adicionar `public Guid? UserId { get; private set; }` em `Teacher.cs` com setter privado
2. Adicionar `Teacher.Create(name, email, subject, schoolId, userId)` como sobrecarga
3. Adicionar `ITeacherRepository.GetByUserIdAsync(Guid userId, CancellationToken ct)`
4. Atualizar `RegisterCommand` para receber `Subject` e `SchoolId` (campos obrigatórios para `Professor`; opcionais para `Diretor`/`Coordenador`)
5. No `RegisterCommandHandler`: após criar `User`, criar `Teacher` automaticamente se `Role == Professor`
6. Migration: `AddUserIdToTeacher` — adicionar coluna `UserId` nullable com FK para `Users.Id`
7. Índice em `Teacher.UserId` para buscas eficientes

---

## Descoberta 3 — Ownership check (D-005 — LGPD crítico)

**Decisão**: Adicionar verificação de propriedade nos handlers GetById, Update e (nos novos) Delete, Publish, Archive. A verificação usa `TeacherId` do recurso comparado com `TeacherId` do usuário autenticado.

**Racional**:
- Atualmente, qualquer professor autenticado pode acessar/modificar planos, atividades e relatórios de outros professores apenas sabendo o `Id` do recurso. Isso viola LGPD (exposição de dados pedagógicos de terceiros).
- A correção correta é no **handler** (Application), não no controller — segue o princípio CQRS: o handler é responsável por autorização de negócio.
- O `TeacherId` do usuário autenticado deve ser passado via Command/Query para que o handler possa comparar.

**Alternativas consideradas**:
- *Usar `[Authorize(Policy = "ResourceOwner")]` no controller*: Rejeitada — a política precisaria de acesso ao repositório, acoplando Infra ao middleware de auth.
- *Verificar no repositório (filtrar por teacherId)*: Válida como abordagem complementar, mas o handler deve ser o guardião primário para erros 403 vs 404 corretos.

**Implementação**:
- Commands de escrita (Update, Delete, Publish, Archive) recebem `TeacherId` extraído do JWT pelo controller
- Queries de leitura (GetById) recebem `TeacherId` opcional; se fornecido, valida propriedade
- Resposta padrão ao tentar acessar recurso de outro professor: `Result.Failure` com "Recurso não encontrado" (não revelar existência — segurança por obscuridade)

**Padrão de extração de TeacherId no controller**:
```csharp
// Controller (API layer — correto receber claim do JWT)
var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
// Handler busca TeacherId via ITeacherRepository.GetByUserIdAsync(userId)
// OU: Controller passa userId; Handler resolve TeacherId internamente
```

**Decisão final de design**: O controller extrai `userId` do JWT e passa para o Command/Query. O handler resolve `TeacherId` via `ITeacherRepository.GetByUserIdAsync()`. Isso evita que o controller tenha lógica de negócio e mantém o handler como responsável pela validação.

---

## Descoberta 4 — Validadores ausentes (L-003)

**Decisão**: Criar validadores faltantes para os 5 commands identificados.

**Evidência**: `GenerateActivityCommandValidator` e `GenerateReportCommandValidator` não existem. `UpdateLessonPlanCommandValidator`, `UpdateActivityCommandValidator` e `UpdateReportCommandValidator` não existem. O `ValidationBehavior` está configurado no pipeline do MediatR e é automático — basta criar a classe que implementa `AbstractValidator<T>`.

**Racional**: Sem validadores, os commands de geração de IA e atualização chegam ao handler sem validação de entrada. Isso pode causar chamadas desnecessárias à OpenAI (custo) e erros de nulidade.

---

## Descoberta 5 — Índices de banco

**Decisão**: Adicionar índices em `Status` (LessonPlan, Activity) e `IsAIGenerated` (LessonPlan, Activity, PedagogicalReport) na mesma migração que adiciona `UserId` ao `Teacher`.

**Racional**: Com filtros avançados (L-007) implementados, consultas por Status e IsAIGenerated se tornarão frequentes. O índice é custo zero agora e evita scan full-table mais tarde.

**Evidência**: TeacherId já tem índice em todas as entidades relevantes (confirmado). Status e IsAIGenerated não têm (ausência confirmada nas configurações EF).

---

## Descoberta 6 — Handler de GetById recebe ownership check via teacherId

**Decisão**: `GetLessonPlanByIdQuery`, `GetActivityByIdQuery` e `GetReportByIdQuery` receberão `RequestingTeacherId` como campo. Se `RequestingTeacherId` for fornecido e diferir do `TeacherId` do recurso, o handler retorna Failure (recurso não encontrado).

**Racional**: O controller deve extrair `userId` do JWT, resolver para `teacherId` via repositório, e passar no Query. Isso mantém a verificação no handler (Application) sem lógica no controller.

---

## Mapa de dependências de implementação

```
D-001/L-005 (UserId em Teacher + criação no Register)
  └── D-005 (ownership check — depende de Teacher.UserId para resolver teacherId do usuário logado)
        └── L-001 (DELETE — usa mesmo padrão de ownership)
              └── L-002 (Publish/Archive — usa mesmo padrão de ownership)

L-003 (validadores) — independente, pode ir em paralelo

L-004 (Teacher API /me) — depende de D-001

L-006 (IA auxiliar) — independente de D-001, pode ir após L-001

L-007 (filtros avançados) — depende de índices (mesma migração de D-001)

L-008 (histórico IA) — independente
```

---

## Conclusão

- **Nenhum item é NEEDS CLARIFICATION** — todas as incertezas foram resolvidas via análise de código.
- **Ordem de implementação determinada**: D-001 → D-005 → L-001 → L-002 → L-003 → L-004 → L-006 → L-007 → L-008.
- **L-009 encerrada** — não é uma lacuna real.
- **Escopo de migração definido**: 1 migration (`AddUserIdAndIndexes`) cobre D-001 e índices de L-007.
- **Restrição honrada**: Nenhuma reestruturação de solution. Auth preservada. Todas as mudanças são incrementais dentro da arquitetura existente.
