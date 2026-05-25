---
applyTo: "Siaed.Application/**"
---

# Convenções da Camada Application (CQRS + MediatR)

## Estrutura de uma Feature

Toda feature deve seguir o padrão:

```
Features/<NomeDaFeature>/
  Commands/
    Create<Feature>Command.cs       # IRequest<Result>
    Update<Feature>Command.cs
  Queries/
    Get<Feature>ByIdQuery.cs        # IRequest<Result<FeatureDto>>
    List<Feature>sQuery.cs          # IRequest<Result<PagedResult<FeatureDto>>>
  Handlers/
    Create<Feature>CommandHandler.cs
    Get<Feature>ByIdQueryHandler.cs
  DTOs/
    <Feature>Dto.cs
  Validators/
    Create<Feature>CommandValidator.cs
```

## Command Handler — Template

```csharp
public sealed class Create<Feature>CommandHandler
    : IRequestHandler<Create<Feature>Command, Result>
{
    private readonly I<Feature>Repository _repository;
    private readonly IUnitOfWork _unitOfWork;

    // Injetar apenas interfaces — nunca implementações concretas

    public async Task<Result> Handle(Create<Feature>Command request, CancellationToken ct)
    {
        // 1. Criar entidade via factory/construtor do Domain
        // 2. Persistir via repositório
        // 3. Commit via UnitOfWork
        // 4. Retornar Result.Success() ou Result.Failure(error)
    }
}
```

## Validator — Template

```csharp
public sealed class Create<Feature>CommandValidator
    : AbstractValidator<Create<Feature>Command>
{
    public Create<Feature>CommandValidator()
    {
        RuleFor(x => x.Titulo).NotEmpty().MaximumLength(200);
        // ...
    }
}
```

## Regras

- Handlers **não** devem conter lógica de domínio — delegar para entidades/Domain Services
- Validators **não** devem acessar banco diretamente para regras de negócio (isso vai no Domain)
- DTOs são imutáveis (records preferidos): `public sealed record FeatureDto(Guid Id, string Titulo)`
- Paginação obrigatória em queries de lista: usar `PagedResult<T>` com `Page` e `PageSize`
