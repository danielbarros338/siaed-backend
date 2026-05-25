---
applyTo: "Siaed.Domain/**"
---

# Convenções da Camada Domain

## Entidade Base

Toda entidade deve herdar de `BaseEntity` (a criar em `Domain/Entities/`):

```csharp
public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; protected set; }

    public bool IsDeleted => DeletedAt.HasValue;

    protected void MarkAsDeleted() => DeletedAt = DateTime.UtcNow;
    protected void MarkAsUpdated() => UpdatedAt = DateTime.UtcNow;
}
```

## Entidades do Módulo 1

| Entidade           | Descrição                              |
|--------------------|----------------------------------------|
| `Teacher`          | Professor (agregado raiz)              |
| `LessonPlan`       | Plano de aula gerado/editado           |
| `Activity`         | Atividade pedagógica                   |
| `PedagogicalReport`| Relatório pedagógico                   |
| `AIRequest`        | Solicitação enviada à IA               |
| `AIResponse`       | Resposta gerada pela OpenAI            |

## Regras

- Construtores privados/protected — criar via factory methods estáticos: `LessonPlan.Create(...)`
- Propriedades com setters `private set` — mutação apenas via métodos de domínio
- Domain Events: usar `IEnumerable<IDomainEvent>` na entidade, disparar via MediatR após persistência
- Value Objects são imutáveis (records): `public sealed record SubjectName(string Value)`
- **Nunca** referenciar EF Core, MediatR, OpenAI ou qualquer pacote NuGet externo nesta camada
