# SIAED Backend — Instruções para Agentes de IA

Plataforma de inteligência operacional para escolas particulares. Camada de IA sobre ERPs existentes, focada em redução de evasão, automação pedagógica e comunicação escola-família.

Especificações completas: [docs/projeto-base.pdf](docs/projeto-base.pdf) | [docs/modulo-1.pdf](docs/modulo-1.pdf)

---

## Stack

| Camada       | Tecnologia                                              |
|--------------|---------------------------------------------------------|
| Framework    | .NET 10, ASP.NET Core                                   |
| ORM          | Entity Framework Core + MySQL                           |
| Mediador     | MediatR (CQRS)                                          |
| Validação    | FluentValidation                                        |
| IA           | OpenAI API                                              |
| Auth         | JWT Bearer                                              |
| Logging      | Serilog (estruturado)                                   |

---

## Arquitetura

**Clean Architecture + DDD + CQRS**

```
Siaed.Api  →  Siaed.Application  →  Siaed.Domain
Siaed.Infra  →  Siaed.Application  →  Siaed.Domain
```

### Regra de dependência (CRÍTICA)

- `Siaed.Domain` **nunca** depende de Api, Infra ou qualquer framework externo (EF Core, OpenAI, etc.)
- `Siaed.Application` depende apenas de abstrações (interfaces); nunca referencia diretamente EF Core ou OpenAI
- `Siaed.Infra` implementa interfaces definidas em Application; nunca contém regras de negócio
- `Siaed.Api` é fina: apenas HTTP, autenticação/autorização e delegação para Application via MediatR

---

## Estrutura de Pastas por Projeto

### Siaed.Api
```
Controllers/
Middlewares/
Filters/
Extensions/
Configurations/
DependencyInjection/
```

### Siaed.Application
```
Features/
  LessonPlans/
  Activities/
  Reports/
  AI/
    Commands/
    Queries/
    Handlers/
    DTOs/
    Validators/
Interfaces/
Behaviors/
Services/
```

### Siaed.Domain
```
Entities/
ValueObjects/
Aggregates/
Enums/
Events/
Specifications/
Exceptions/
```

### Siaed.Infra
```
Persistence/
Repositories/
Providers/
OpenAI/
Messaging/
ExternalServices/
Cache/
Identity/
```

---

## Padrões Obrigatórios

### CQRS com MediatR
- Operações de escrita → `IRequest<Result>` (Commands)
- Operações de leitura → `IRequest<Result<T>>` (Queries)
- Handlers ficam em `Features/<Feature>/`
- Nunca enviar MediatR diretamente da Api — usar apenas via `IMediator`

### Result Pattern
Todas as respostas de handlers devem retornar um tipo `Result<T>` ou `Result` (sem exceções não tratadas fluindo para a Api).

### FluentValidation
- Validators em `Features/<Feature>/Validators/`
- Registrados automaticamente via pipeline behavior do MediatR (`ValidationBehavior`)
- Aplicar em Commands e Queries

### Repository Pattern + Unit of Work
- Interfaces de repositório definidas em `Application/Interfaces/`
- Implementações em `Infra/Repositories/`
- Transações controladas via `IUnitOfWork`

---

## Convenção de Entidades

Toda entidade de domínio deve herdar de uma base com:

```csharp
public Guid Id { get; private set; }
public DateTime CreatedAt { get; private set; }
public DateTime UpdatedAt { get; private set; }
public DateTime? DeletedAt { get; private set; } // soft delete quando aplicável
```

**Entidades do Módulo 1:** `Teacher`, `LessonPlan`, `Activity`, `PedagogicalReport`, `AIRequest`, `AIResponse`

---

## Integração OpenAI

- **Contratos** (interfaces) definidos em `Application/Interfaces/`: `IOpenAIService`, `IChatCompletionService`, `IPromptBuilder`, `IAIContextManager`
- **Implementações** em `Infra/OpenAI/`: `OpenAIService`, `OpenAIChatService`, `PromptBuilderService`
- Toda chamada à OpenAI deve: validar prompt, limitar tokens, tratar falhas com retry policy, registrar log com tokens utilizados e custo
- **A IA responde sempre em português brasileiro**, com linguagem pedagógica e adaptada à faixa etária
- **A IA NÃO deve** emitir diagnósticos, laudos médicos, inventar fatos ou substituir decisões pedagógicas

---

## Segurança

- **LGPD**: proteger dados de alunos menores — nunca expor dados pessoais desnecessariamente
- **Auth**: JWT Bearer com Roles + Policies granulares
- Logs de auditoria obrigatórios para operações sensíveis
- Não enviar dados sensíveis à OpenAI sem sanitização prévia

---

## Banco de Dados

- MySQL via Entity Framework Core
- Migrations versionadas (`dotnet ef migrations add`)
- Soft delete com `DeletedAt`; nunca `DELETE` físico em entidades de domínio
- Concorrência: controle otimista (RowVersion ou EF Concurrency Token)
- Índices otimizados; paginação obrigatória em queries de lista

---

## Comandos de Build e Execução

```bash
# Restaurar dependências
dotnet restore

# Build
dotnet build

# Executar API (desenvolvimento)
dotnet run --project Siaed.Api

# Aplicar migrations
dotnet ef database update --project Siaed.Infra --startup-project Siaed.Api

# Adicionar migration
dotnet ef migrations add <NomeDaMigration> --project Siaed.Infra --startup-project Siaed.Api
```

---

## O que NÃO fazer

- Não colocar regra de negócio em controllers ou Infra
- Não acessar `DbContext` diretamente em Application ou Domain
- Não instanciar `HttpClient` para OpenAI diretamente — usar interface registrada no DI
- Não retornar exceções raw para o cliente — sempre usar Result Pattern + middleware de exception handling
- Não adicionar referência de `Siaed.Infra` em `Siaed.Api` diretamente (usar DI via módulos de extensão)
