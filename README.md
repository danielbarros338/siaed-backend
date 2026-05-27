# SIAED Backend

Backend da plataforma SIAED, uma camada de inteligência operacional para escolas particulares. O sistema segue Clean Architecture + DDD + CQRS e oferece apoio pedagógico com IA, gerenciamento de alunos e turmas, autenticação JWT e recursos para criação e consulta de planos de aula, atividades, relatórios e notas por atividade.

## Stack

- .NET 10 / ASP.NET Core
- Entity Framework Core + MySQL
- MediatR para CQRS
- FluentValidation
- OpenAI API
- JWT Bearer
- Serilog

## Estrutura da solução

- `Siaed.Api`: camada HTTP, autenticação, autorização e controllers
- `Siaed.Application`: casos de uso, validações, interfaces e behaviors do MediatR
- `Siaed.Domain`: entidades, enums, eventos e regras de domínio
- `Siaed.Infra`: persistência, repositórios, integrações externas e configurações de banco

## Requisitos

- .NET 10 SDK
- MySQL 8.4 ou compatível
- Chave válida da OpenAI, quando for usar os recursos de IA

## Configuração local

1. Ajuste a string de conexão em `Siaed.Api/appsettings.Development.json`.
2. Configure `Jwt:Key`, `Jwt:Issuer` e `Jwt:Audience`.
3. Configure `OpenAI:ApiKey` se for usar geração por IA.
4. Aplique as migrations antes de iniciar a API.

## Como executar

### Restaurar dependências

```bash
dotnet restore
```

### Build

```bash
dotnet build
```

### Rodar a API localmente

```bash
dotnet run --project Siaed.Api/SiaedBackend.Api.csproj
```

### Aplicar migrations

```bash
dotnet ef database update --project Siaed.Infra --startup-project Siaed.Api
```

### Criar uma nova migration

```bash
dotnet ef migrations add NomeDaMigration --project Siaed.Infra --startup-project Siaed.Api
```

## Execução com Docker

O arquivo `docker-compose.yml` sobe a API e um container MySQL. Antes de executar, defina as variáveis de ambiente obrigatórias:

- `MYSQL_PASSWORD`
- `MYSQL_ROOT_PASSWORD`
- `Jwt__Key`
- `Jwt__Issuer`
- `Jwt__Audience`
- `OpenAI__ApiKey`

Para subir o ambiente:

```bash
docker compose up --build
```

## URLs úteis em desenvolvimento

- API HTTP: `http://localhost:5248`
- API HTTPS: `https://localhost:7284`
- Swagger: `http://localhost:5248/swagger`

## Módulos principais

- Autenticação de usuários com JWT
- Gestão de professores, alunos e turmas
- Planos de aula com geração assistida por IA
- Atividades pedagógicas
- Notas por atividade com paginação, filtros e soft delete
- Relatórios pedagógicos

## Convenções importantes

- A API deve apenas orquestrar requisições via MediatR.
- A camada de aplicação retorna `Result` ou `Result<T>`.
- O domínio não depende de frameworks externos.
- Operações de escrita devem respeitar as regras de domínio e a validação antes da persistência.

## Documentação adicional

- [Estado atual da implementação](docs/backend-state.md)
- [Especificações do módulo 3](specs/003-gerenciar-notas-atividades/)
