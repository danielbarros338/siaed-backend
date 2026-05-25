---
applyTo: "Siaed.Api/**"
---

# Convenções da Camada API

## Controller — Template

```csharp
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public sealed class LessonPlansController : ControllerBase
{
    private readonly IMediator _mediator;

    public LessonPlansController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(CreateLessonPlanRequest request, CancellationToken ct)
    {
        var command = new CreateLessonPlanCommand(/* mapear do request */);
        var result = await _mediator.Send(command, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value }, null)
            : BadRequest(result.Errors);
    }
}
```

## Regras

- Controllers são **finos**: receber request → mapear para Command/Query → enviar via `IMediator` → retornar resposta HTTP
- **Nunca** injetar repositórios, DbContext ou serviços de infraestrutura diretamente em controllers
- Versionamento de API via rota: `api/v1/`, `api/v2/`
- Exception handling global via middleware — não usar try/catch nos controllers
- Rate limiting configurado em `Configurations/RateLimitingConfiguration.cs`
- Registros de DI de cada camada em `DependencyInjection/` como extension methods de `IServiceCollection`

## Autenticação e Autorização

- JWT Bearer configurado em `Configurations/AuthConfiguration.cs`
- Roles: `Admin`, `Director`, `Coordinator`, `Teacher`, `Parent`
- Usar `[Authorize(Roles = "Teacher,Coordinator")]` ou Policies nomeadas
