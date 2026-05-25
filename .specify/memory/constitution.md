# SIAED Backend Constitution

## Core Principles

### I. Clean Architecture + DDD (NON-NEGOTIABLE)
`Siaed.Domain` não depende de nenhum framework externo (EF Core, OpenAI, MediatR, etc.).
`Siaed.Application` depende apenas de abstrações (interfaces); nunca referencia diretamente EF Core ou OpenAI SDK.
`Siaed.Infra` implementa interfaces definidas em Application; nunca contém regras de negócio.
`Siaed.Api` é fina: apenas HTTP, autenticação/autorização e delegação via `IMediator`.
Entidades do domínio nunca expõem dependências de infraestrutura.

### II. CQRS com MediatR (NON-NEGOTIABLE)
Commands → `IRequest<Result>` | Queries → `IRequest<Result<T>>`.
Handlers ficam em `Application/Features/<Feature>/`.
A Api delega exclusivamente via `IMediator` — nunca acessa serviços de Application diretamente.
Regra de negócio em Controller = violação crítica. Regra de negócio em Infra = violação crítica.

### III. Result Pattern (NON-NEGOTIABLE)
Toda resposta de handler retorna `Result<T>` ou `Result`. Nunca `throw` sem tratar.
Nenhuma exceção não tratada pode chegar à Api.
Tratamento centralizado via `ExceptionHandlingMiddleware` (já implementado).

### IV. Entidades de Domínio Ricas (NON-NEGOTIABLE)
Modelos anêmicos são proibidos.
Toda entidade herda de `BaseEntity`: `Id (Guid)`, `CreatedAt`, `UpdatedAt`, `DeletedAt`.
Soft delete obrigatório com `DeletedAt` — DELETE físico proibido em entidades de domínio.
Invariantes de domínio protegidas por métodos factory (`Create(...)`) e setters `private set`.

### V. Segurança e Observabilidade
Serilog estruturado obrigatório (console + arquivo rolling diário).
Toda chamada à OpenAI deve registrar `AIRequest` (ciclo `Pending→Processing→Completed/Failed`) e `AIResponse` com tokens consumidos e custo estimado.
JWT Bearer com Roles (`Professor`, `Diretor`, `Coordenador`) em todos os endpoints de recurso.
LGPD: dados de alunos nunca expostos sem necessidade; sanitizar antes de enviar à IA.
A IA responde sempre em português brasileiro, com linguagem pedagógica.
A IA NÃO emite diagnósticos médicos, NÃO inventa fatos, NÃO substitui decisões pedagógicas humanas.

## Stack

| Camada | Tecnologia |
|--------|-----------|
| Framework | .NET 10, ASP.NET Core |
| ORM | Entity Framework Core + MySQL |
| Mediador | MediatR (CQRS) |
| Validação | FluentValidation via `ValidationBehavior` (pipeline MediatR) |
| IA | OpenAI API — padrão `gpt-4o-mini`; `gpt-4o` disponível |
| Auth | JWT Bearer com Roles |
| Logging | Serilog estruturado |

## Regras de Dependência

```
Siaed.Api  →  Siaed.Application  →  Siaed.Domain
Siaed.Infra  →  Siaed.Application  →  Siaed.Domain
```

`Siaed.Api` NÃO referencia `Siaed.Infra` diretamente — registro de DI feito via módulos de extensão em `Siaed.Infra/DependencyInjection/`.

## Entidades do Domínio (Módulo 1)

`User`, `Teacher`, `LessonPlan`, `Activity`, `PedagogicalReport`, `AIRequest`, `AIResponse`

Enums relevantes: `UserRole`, `LessonPlanStatus`, `ActivityStatus`, `ActivityType`, `AIRequestType`, `AIRequestStatus`

## Estrutura de Pastas

```
Siaed.Application/
  Features/<Feature>/
    Commands/     ← IRequest<Result> ou IRequest<Result<T>>
    Queries/      ← IRequest<Result<T>>
    Handlers/     ← IRequestHandler
    Validators/   ← AbstractValidator<TCommand>
    DTOs/
  Interfaces/     ← Contratos de repositórios e serviços externos
  Behaviors/      ← ValidationBehavior, LoggingBehavior
  Common/         ← Result<T>, PagedResult<T>

Siaed.Infra/
  Persistence/    ← DbContext, EntityConfigurations
  Repositories/   ← Implementações de IRepository
  OpenAI/         ← OpenAIService, PromptBuilderService
  Identity/       ← JwtService, PasswordHasherService
  Migrations/     ← EF Core migrations versionadas
  Messaging/      ← RESERVADO (WhatsApp/SMS — não implementado)
```

## O que NÃO Fazer

- Regra de negócio em Controller ou Infra
- `DbContext` acessado diretamente em Application ou Domain
- `HttpClient` para OpenAI sem interface registrada no DI
- Exceções raw retornadas ao cliente
- DELETE físico em entidades de domínio
- Dados pessoais de alunos enviados à IA sem sanitização
- Entidades EF Core expostas diretamente na API (usar DTOs)
- Adicionar referência de `Siaed.Infra` em `Siaed.Api` diretamente

## Governance

`AGENTS.md` é a fonte de verdade absoluta sobre arquitetura e regras do projeto.
Esta constituição reflete `AGENTS.md` na linguagem do Specify e tem como objetivo orientar agentes de IA.
Em caso de conflito entre este arquivo e `AGENTS.md`, `AGENTS.md` sempre prevalece.
Amendments requerem atualização simultânea neste arquivo e no `AGENTS.md`.

**Version**: 1.0.0 | **Ratified**: 2026-05-18 | **Last Amended**: 2026-05-18
