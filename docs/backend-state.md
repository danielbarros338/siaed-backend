# SIAED Backend — Estado Atual da Implementação

> Documento de referência para desenvolvimento de frontend e geração de documentação via SpecKit.  
> Gerado em: 20/05/2026  
> Versão da API: v1

---

## 1. Visão Geral

### Atualizacao 25/05/2026 - Modulo de Notas por Atividade

Foi implementado o modulo de gerenciamento de notas por atividade com persistencia em tabela independente `Grades`.

Capacidades adicionadas:
- CRUD de notas em `/api/v1/grades`
- Filtros de listagem por `schoolClassId`, `teacherId` e `gradeValue`
- Subsecao de notas por atividade em `GET /api/v1/activities/{id}/grades`
- Concorrencia otimista por versao (`RowVersion`)
- Soft delete de notas

Arquivos principais adicionados/alterados:
- Domain: `Grade`, `GradeStatus`, eventos de dominio de nota
- Application: feature `Grades` (Commands, Queries, Handlers, DTOs, Validators)
- Infra: `GradeRepository`, `GradeConfiguration`, `DbSet<Grade>` e migration `AddGradesTable`
- API: `GradesController` e extensao em `ActivitiesController`

O SIAED Backend é uma plataforma de inteligência operacional para escolas particulares. Atua como uma camada de IA sobre ERPs existentes, com foco em:

- Assistente pedagógico para professores (geração de planos de aula, atividades e relatórios com IA)
- Gerenciamento de alunos e turmas
- Comunicação escola-família automatizada via IA
- Redução de evasão escolar

### Stack Tecnológico

| Camada       | Tecnologia                              |
|--------------|-----------------------------------------|
| Framework    | .NET 10, ASP.NET Core                   |
| ORM          | Entity Framework Core + MySQL           |
| Padrão       | Clean Architecture + DDD + CQRS        |
| Mediador     | MediatR                                 |
| Validação    | FluentValidation                        |
| IA           | OpenAI API (`gpt-4o-mini`)              |
| Autenticação | JWT Bearer (8 horas de expiração)       |
| Logging      | Serilog (arquivo + console)             |

---

## 2. Ambiente de Desenvolvimento

### URLs da API

| Ambiente    | URL                            |
|-------------|-------------------------------|
| HTTP        | `http://localhost:5248`        |
| HTTPS       | `https://localhost:7284`       |
| Swagger UI  | `http://localhost:5248/swagger` |

### Banco de Dados

- **Engine**: MySQL
- **Database**: `siaed`
- **Host**: `localhost`
- **Tabelas recentes**: `Classes` e `ClassTeachers` substituem `SchoolClasses`/`SchoolClassTeachers`; `Teachers` foi removida e professores passam a ser `Users` com `role = Professor`

### Autenticação JWT

- **Scheme**: `Bearer`
- **Issuer / Audience**: `siaed`
- **Expiração**: 8 horas
- **Header**: `Authorization: Bearer <token>`
- **CORS**: política `AllowLocalhost`, aceitando origens `localhost` e `127.0.0.1` com credenciais

---

## 3. Endpoints da API

> **Base URL**: `http://localhost:5248`  
> Todos os endpoints com `[Authorize]` exigem o header `Authorization: Bearer <token>`.

### 3.1 Autenticação — `/api/v1/auth`

Acesso público (`[AllowAnonymous]`).

#### `POST /api/v1/auth/register`

Cria um novo usuário no sistema.

**Corpo da Requisição (JSON)**:
```json
{
  "name": "string",
  "email": "string",
  "password": "string",
  "role": 1,
  "subject": "string | null",
  "schoolId": "string | null"
}
```

| Campo    | Tipo     | Obrigatório | Descrição                              |
|----------|----------|-------------|----------------------------------------|
| name     | string   | Sim         | Nome completo                          |
| email    | string   | Sim         | E-mail único                           |
| password | string   | Sim         | Senha (mínimo 8 caracteres)            |
| role     | int (enum) | Sim       | 1=Professor, 2=Diretor, 3=Coordenador  |
| subject  | string   | Não         | Campo legado no contrato; não é persistido atualmente |
| schoolId | string   | Não         | Campo legado no contrato; não é persistido atualmente |

> Mudança: o cadastro não cria mais registro em `Teachers`. Para professores, o próprio `User` com `role = 1` passa a ser usado como `teacherId` nos módulos pedagógicos.

**Resposta 200 OK**:
```json
{
  "userId": "guid",
  "name": "string",
  "email": "string",
  "role": 1,
  "token": "string (JWT)",
  "expiresAt": "2026-05-20T20:00:00Z"
}
```

**Resposta 400 Bad Request**:
```json
{ "errors": ["mensagem de erro"] }
```

---

#### `POST /api/v1/auth/login`

Autentica um usuário existente.

**Corpo da Requisição (JSON)**:
```json
{
  "email": "string",
  "password": "string"
}
```

**Resposta 200 OK**:
```json
{
  "userId": "guid",
  "name": "string",
  "email": "string",
  "role": 1,
  "token": "string (JWT)",
  "expiresAt": "2026-05-20T20:00:00Z"
}
```

**Resposta 400 Bad Request**: credenciais inválidas.

---

### 3.2 Professores — `/api/v1/teachers`

Requer `[Authorize]`.

> Mudança em relação ao estado anterior: não existe mais entidade/tabela `Teacher` separada. Professores agora são usuários (`User`) com `role = Professor`; portanto, o `id` retornado aqui é o próprio `userId`.

#### `GET /api/v1/teachers`

Lista usuários com perfil de professor, com paginação e busca.

**Query Parameters**:
| Parâmetro | Tipo   | Obrigatório | Default | Descrição                    |
|-----------|--------|-------------|---------|------------------------------|
| page      | int    | Não         | 1       | Página atual                 |
| pageSize  | int    | Não         | 20      | Itens por página             |
| search    | string | Não         | null    | Busca por nome ou e-mail     |

**Resposta 200 OK**:
```json
{
  "items": [
    {
      "id": "guid",
      "name": "string",
      "email": "string",
      "createdAt": "2026-05-20T00:00:00Z"
    }
  ],
  "totalCount": 0,
  "page": 1,
  "pageSize": 20,
  "totalPages": 0,
  "hasNextPage": false,
  "hasPreviousPage": false
}
```

---

#### `GET /api/v1/teachers/me`

Retorna o perfil do professor logado, identificado pelo JWT.

**Resposta 200 OK**:
```json
{
  "id": "guid",
  "name": "string",
  "email": "string",
  "createdAt": "2026-05-20T00:00:00Z"
}
```

---

### 3.3 Planos de Aula — `/api/v1/lessonplans`

Requer `[Authorize]`.

#### `POST /api/v1/lessonplans`

Cria um plano de aula manualmente.

**Corpo da Requisição (JSON)**:
```json
{
  "teacherId": "guid",
  "title": "string",
  "subject": "string",
  "grade": "string",
  "durationMinutes": 50,
  "objectives": "string",
  "content": "string",
  "methodology": "string",
  "resources": "string",
  "evaluation": "string",
  "ageRange": "string"
}
```

> `teacherId` deve ser o `id` do usuário com `role = Professor`.

**Resposta 201 Created**:
```json
{ "id": "guid" }
```

---

#### `POST /api/v1/lessonplans/generate`

Gera um plano de aula com IA (OpenAI).

**Corpo da Requisição (JSON)**:
```json
{
  "teacherId": "guid",
  "subject": "string",
  "grade": "string",
  "ageRange": "string",
  "durationMinutes": 50,
  "additionalInstructions": "string (opcional, default: '')"
}
```

**Resposta 201 Created**:
```json
{ "id": "guid" }
```

---

#### `GET /api/v1/lessonplans`

Lista planos de aula com paginação.

**Query Parameters**:
| Parâmetro      | Tipo    | Obrigatório | Default | Descrição                              |
|----------------|---------|-------------|---------|----------------------------------------|
| teacherId      | guid    | Sim         | —       | ID do professor                        |
| page           | int     | Não         | 1       | Página atual                           |
| pageSize       | int     | Não         | 10      | Itens por página                       |
| status         | string  | Não         | null    | `Draft`, `Published` ou `Archived`     |
| isAIGenerated  | bool    | Não         | null    | Filtrar por gerado por IA              |

**Resposta 200 OK**:
```json
{
  "items": [
    {
      "id": "guid",
      "teacherId": "guid",
      "title": "string",
      "subject": "string",
      "grade": "string",
      "durationMinutes": 50,
      "objectives": "string",
      "content": "string",
      "methodology": "string",
      "resources": "string",
      "evaluation": "string",
      "ageRange": "string",
      "isAIGenerated": false,
      "status": 1,
      "createdAt": "2026-05-20T00:00:00Z",
      "updatedAt": "2026-05-20T00:00:00Z"
    }
  ],
  "totalCount": 100,
  "page": 1,
  "pageSize": 10,
  "totalPages": 10
}
```

---

#### `GET /api/v1/lessonplans/{id}`

Retorna um plano de aula pelo ID.

**Resposta 200 OK**: mesmo schema do item acima.  
**Resposta 404 Not Found**.

---

#### `PUT /api/v1/lessonplans/{id}`

Atualiza um plano de aula.

**Corpo da Requisição (JSON)**:
```json
{
  "id": "guid",
  "title": "string",
  "objectives": "string",
  "content": "string",
  "methodology": "string",
  "resources": "string",
  "evaluation": "string"
}
```

**Resposta 204 No Content**.  
**Resposta 404 Not Found** | **400 Bad Request**.

> Mudança: `requestingUserId` é obtido do JWT no controller e sobrescreve qualquer valor enviado no corpo.

---

#### `DELETE /api/v1/lessonplans/{id}`

Remove (soft delete) um plano de aula.

**Resposta 204 No Content** | **404 Not Found**.

---

#### `PATCH /api/v1/lessonplans/{id}/publish`

Publica um plano de aula (muda status de `Draft` → `Published`).

**Resposta 204 No Content** | **404 Not Found** | **400 Bad Request** (se não for `Draft`).

---

#### `PATCH /api/v1/lessonplans/{id}/archive`

Arquiva um plano de aula (muda status → `Archived`).

**Resposta 204 No Content** | **404 Not Found** | **400 Bad Request**.

---

### 3.4 Atividades — `/api/v1/activities`

Requer `[Authorize]`.

#### `POST /api/v1/activities`

Cria uma atividade manualmente.

**Corpo da Requisição (JSON)**:
```json
{
  "teacherId": "guid",
  "title": "string",
  "description": "string",
  "subject": "string",
  "grade": "string",
  "ageRange": "string",
  "content": "string",
  "type": 1,
  "lessonPlanId": "guid | null"
}
```

| Campo       | Tipo   | Valores do enum `type`                          |
|-------------|--------|--------------------------------------------------|
| type        | int    | 1=Exercise, 2=Quiz, 3=Project, 4=Homework        |

**Resposta 201 Created**: `{ "id": "guid" }`.

---

#### `POST /api/v1/activities/generate`

Gera uma atividade com IA.

**Corpo da Requisição (JSON)**:
```json
{
  "teacherId": "guid",
  "subject": "string",
  "grade": "string",
  "ageRange": "string",
  "type": 1,
  "numberOfQuestions": 10,
  "lessonPlanId": "guid | null",
  "additionalInstructions": "string (opcional, default: '')"
}
```

**Resposta 201 Created**: `{ "id": "guid" }`.

---

#### `GET /api/v1/activities`

Lista atividades com paginação.

**Query Parameters**:
| Parâmetro      | Tipo    | Obrigatório | Default |
|----------------|---------|-------------|---------|
| teacherId      | guid    | Sim         | —       |
| page           | int     | Não         | 1       |
| pageSize       | int     | Não         | 10      |
| status         | string  | Não         | null    |
| isAIGenerated  | bool    | Não         | null    |

**Resposta 200 OK**:
```json
{
  "items": [
    {
      "id": "guid",
      "teacherId": "guid",
      "lessonPlanId": "guid | null",
      "title": "string",
      "description": "string",
      "subject": "string",
      "grade": "string",
      "ageRange": "string",
      "content": "string",
      "answerKey": "string",
      "simplifiedVersion": "string",
      "type": 1,
      "isAIGenerated": false,
      "status": 1,
      "createdAt": "2026-05-20T00:00:00Z",
      "updatedAt": "2026-05-20T00:00:00Z"
    }
  ],
  "totalCount": 0,
  "page": 1,
  "pageSize": 10,
  "totalPages": 0,
  "hasNextPage": false,
  "hasPreviousPage": false
}
```

---

#### `GET /api/v1/activities/{id}`

**Resposta 200 OK**: mesmo schema do item acima.  
**Resposta 404 Not Found**.

---

#### `PUT /api/v1/activities/{id}`

**Corpo da Requisição (JSON)**:
```json
{
  "id": "guid",
  "title": "string",
  "description": "string",
  "content": "string"
}
```

**Resposta 204 No Content**.

> Mudança: `requestingUserId` é obtido do JWT no controller e usado para validar propriedade do recurso.

---

#### `DELETE /api/v1/activities/{id}`

**Resposta 204 No Content** | **404 Not Found**.

---

#### `PATCH /api/v1/activities/{id}/publish`

Muda status para `Published`.

**Resposta 204 No Content** | **404 Not Found** | **400 Bad Request**.

---

#### `PATCH /api/v1/activities/{id}/archive`

Muda status para `Archived`.

**Resposta 204 No Content** | **404 Not Found** | **400 Bad Request**.

---

### 3.5 Relatórios Pedagógicos — `/api/v1/reports`

Requer `[Authorize]`.

#### `POST /api/v1/reports`

Cria um relatório pedagógico manualmente.

**Corpo da Requisição (JSON)**:
```json
{
  "teacherId": "guid",
  "studentName": "string",
  "grade": "string",
  "period": "string",
  "content": "string"
}
```

**Resposta 201 Created**: `{ "id": "guid" }`.

---

#### `POST /api/v1/reports/generate`

Gera um relatório pedagógico com IA.

**Corpo da Requisição (JSON)**:
```json
{
  "teacherId": "guid",
  "studentName": "string",
  "grade": "string",
  "period": "string",
  "performanceNotes": "string",
  "additionalInstructions": "string (opcional, default: '')"
}
```

**Resposta 201 Created**: `{ "id": "guid" }`.

---

#### `GET /api/v1/reports`

Lista relatórios com paginação.

**Query Parameters**:
| Parâmetro      | Tipo    | Obrigatório | Default |
|----------------|---------|-------------|---------|
| teacherId      | guid    | Sim         | —       |
| page           | int     | Não         | 1       |
| pageSize       | int     | Não         | 10      |
| isAIGenerated  | bool    | Não         | null    |

**Resposta 200 OK**:
```json
{
  "items": [
    {
      "id": "guid",
      "teacherId": "guid",
      "studentName": "string",
      "grade": "string",
      "period": "string",
      "content": "string",
      "summary": "string",
      "parentCommunication": "string",
      "isAIGenerated": false,
      "createdAt": "2026-05-20T00:00:00Z",
      "updatedAt": "2026-05-20T00:00:00Z"
    }
  ],
  "totalCount": 0,
  "page": 1,
  "pageSize": 10,
  "totalPages": 0
}
```

---

#### `GET /api/v1/reports/{id}`

**Resposta 200 OK**: mesmo schema do item acima.  
**Resposta 404 Not Found**.

---

#### `PUT /api/v1/reports/{id}`

**Corpo da Requisição (JSON)**:
```json
{
  "id": "guid",
  "content": "string",
  "summary": "string",
  "parentCommunication": "string"
}
```

**Resposta 204 No Content**.

> Mudança: `requestingUserId` é obtido do JWT no controller e usado em atualização, exclusão, resumo e comunicação com responsáveis.

---

#### `DELETE /api/v1/reports/{id}`

**Resposta 204 No Content** | **404 Not Found**.

---

#### `POST /api/v1/reports/{id}/summarize`

Gera um resumo do relatório usando IA.

**Resposta 200 OK**:
```json
{
  "reportId": "guid",
  "summary": "string",
  "tokensUsed": 150,
  "estimatedCost": 0.000225
}
```

---

#### `POST /api/v1/reports/{id}/parent-communication`

Gera texto de comunicação com pais/responsáveis usando IA.

**Resposta 200 OK**:
```json
{
  "reportId": "guid",
  "parentCommunication": "string",
  "tokensUsed": 200,
  "estimatedCost": 0.0003
}
```

---

### 3.6 Alunos — `/api/v1/students`

Requer `[Authorize]`.

#### `POST /api/v1/students`

Registra um aluno.

**Corpo da Requisição (JSON)**:
```json
{
  "fullName": "string",
  "documentType": 1,
  "documentId": "string",
  "birthDate": "2010-01-15",
  "classId": "guid",
  "enrollmentDate": "2026-02-01",
  "notes": "string | null"
}
```

| Campo        | Tipo   | Valores do enum `documentType`                      |
|--------------|--------|------------------------------------------------------|
| documentType | int    | 1=Cpf, 2=RegistroEstrangeiro, 3=IdInterno            |

**Resposta 201 Created**: `{ "id": "guid" }`.  
**Resposta 400 Bad Request**: duplicidade de documento.

---

#### `GET /api/v1/students`

Lista alunos com paginação e filtros.

**Query Parameters**:
| Parâmetro | Tipo          | Obrigatório | Default |
|-----------|---------------|-------------|---------|
| page      | int           | Não         | 1       |
| pageSize  | int           | Não         | 20      |
| search    | string        | Não         | null    |
| status    | int (enum)    | Não         | null    |
| classId   | guid          | Não         | null    |

**Resposta 200 OK**:
```json
{
  "items": [
    {
      "id": "guid",
      "fullName": "string",
      "documentIdMasked": "XXX***XXXX",
      "classId": "guid",
      "className": "string",
      "status": 1
    }
  ],
  "totalCount": 0,
  "page": 1,
  "pageSize": 20,
  "totalPages": 0,
  "hasNextPage": false,
  "hasPreviousPage": false
}
```

---

#### `GET /api/v1/students/{id}`

Retorna detalhes completos de um aluno.

**Resposta 200 OK**:
```json
{
  "id": "guid",
  "fullName": "string",
  "documentType": 1,
  "documentIdMasked": "XXX***XXXX",
  "birthDate": "2010-01-15",
  "classId": "guid",
  "className": "string",
  "status": 1,
  "enrollmentDate": "2026-02-01",
  "notes": "string | null",
  "createdAt": "2026-05-20T00:00:00Z"
}
```

---

#### `PUT /api/v1/students/{id}`

Atualiza dados cadastrais de um aluno.

**Corpo da Requisição (JSON)**:
```json
{
  "id": "guid",
  "fullName": "string",
  "documentType": 1,
  "documentId": "string",
  "birthDate": "2010-01-15",
  "notes": "string | null"
}
```

**Resposta 204 No Content** | **404 Not Found**.

---

#### `PATCH /api/v1/students/{id}/transfer`

Transfere um aluno para outra turma.

**Corpo da Requisição (JSON)**:
```json
{
  "newClassId": "guid"
}
```

**Resposta 204 No Content** | **400 Bad Request** | **404 Not Found**.

---

#### `PATCH /api/v1/students/{id}/deactivate`

Inativa ou marca evasão de um aluno.

**Corpo da Requisição (JSON)**:
```json
{
  "status": 2
}
```

| status | Significado |
|--------|-------------|
| 2      | Inativo     |
| 3      | Evadido     |

**Resposta 204 No Content** | **400 Bad Request** | **404 Not Found**.

---

#### `PATCH /api/v1/students/{id}/reactivate`

Reativa um aluno inativo/evadido.

**Corpo da Requisição (JSON)**:
```json
{
  "classId": "guid"
}
```

**Resposta 204 No Content** | **404 Not Found**.

---

#### `POST /api/v1/students/import`

Importa alunos em lote via arquivo CSV.

**Content-Type**: `multipart/form-data`  
**Campo**: `file` (arquivo `.csv`)

**Resposta 200 OK**:
```json
{
  "imported": 45,
  "skipped": 3,
  "errors": ["Linha 5: documento duplicado", "Linha 12: turma não encontrada"]
}
```

---

### 3.7 Turmas — `/api/v1/classes`

Requer `[Authorize]`.

#### `POST /api/v1/classes`

Cria uma turma.

**Corpo da Requisição (JSON)**:
```json
{
  "name": "string",
  "grade": "string",
  "schoolYear": 2026,
  "teacherIds": ["guid"]
}
```

| Campo      | Tipo        | Obrigatório | Descrição                                      |
|------------|-------------|-------------|------------------------------------------------|
| teacherIds | guid[]      | Não         | IDs de usuários com `role = Professor` associados à turma |

**Resposta 201 Created**: `{ "id": "guid" }`.

---

#### `GET /api/v1/classes`

Lista turmas com paginação.

**Query Parameters**:
| Parâmetro | Tipo   | Obrigatório | Default |
|-----------|--------|-------------|---------|
| page      | int    | Não         | 1       |
| pageSize  | int    | Não         | 20      |
| search    | string | Não         | null    |

**Resposta 200 OK**:
```json
{
  "items": [
    {
      "id": "guid",
      "name": "string",
      "grade": "string",
      "schoolYear": 2026,
      "status": 1
    }
  ],
  "totalCount": 0,
  "page": 1,
  "pageSize": 20,
  "totalPages": 0
}
```

---

#### `GET /api/v1/classes/{id}`

**Resposta 200 OK**:
```json
{
  "id": "guid",
  "name": "string",
  "grade": "string",
  "schoolYear": 2026,
  "status": 1,
  "createdAt": "2026-05-20T00:00:00Z",
  "teachers": [
    {
      "id": "guid",
      "name": "string",
      "email": "string",
      "createdAt": "2026-05-20T00:00:00Z"
    }
  ]
}
```

---

#### `PUT /api/v1/classes/{id}`

**Corpo da Requisição (JSON)**:
```json
{
  "id": "guid",
  "name": "string",
  "grade": "string",
  "schoolYear": 2026,
  "teacherIds": ["guid"]
}
```

**Resposta 204 No Content** | **404 Not Found**.

---

#### `DELETE /api/v1/classes/{id}`

Inativa uma turma, alterando `status` para `Inactive`.

**Resposta 204 No Content** | **404 Not Found**.

---

#### `PATCH /api/v1/classes/{id}/reactivate`

Reativa uma turma inativa.

**Resposta 204 No Content** | **404 Not Found**.

---

### 3.8 Histórico de IA — `/api/v1/ai`

Requer `[Authorize]`.

#### `GET /api/v1/ai/requests`

Lista requisições feitas à IA pelo usuário autenticado, com paginação.

**Query Parameters**:
| Parâmetro | Tipo | Obrigatório | Default |
|-----------|------|-------------|---------|
| page      | int  | Não         | 1       |
| pageSize  | int  | Não         | 10      |

**Resposta 200 OK**:
```json
{
  "items": [
    {
      "id": "guid",
      "requestType": "LessonPlan",
      "status": "Completed",
      "model": "gpt-4o-mini",
      "maxTokens": 2000,
      "tokensUsed": 850,
      "estimatedCost": 0.001275,
      "errorMessage": null,
      "createdAt": "2026-05-20T00:00:00Z",
      "updatedAt": "2026-05-20T00:00:00Z"
    }
  ],
  "totalCount": 0,
  "page": 1,
  "pageSize": 10,
  "totalPages": 0
}
```

**Valores de `requestType`**: `LessonPlan`, `Activity`, `Report`, `Summarization`, `TextReformulation`, `ParentCommunication`  
**Valores de `status`**: `Pending`, `Processing`, `Completed`, `Failed`

> Mudança: o filtro por professor não vem mais por query string; é sempre derivado do `userId` do JWT.

---

## 4. Enums e Valores Válidos

### UserRole
| Valor | Nome         |
|-------|--------------|
| 1     | Professor    |
| 2     | Diretor      |
| 3     | Coordenador  |

### LessonPlanStatus / ActivityStatus
| Valor | Nome      |
|-------|-----------|
| 1     | Draft     |
| 2     | Published |
| 3     | Archived  |

### ActivityType
| Valor | Nome      |
|-------|-----------|
| 1     | Exercise  |
| 2     | Quiz      |
| 3     | Project   |
| 4     | Homework  |

### StudentStatus
| Valor | Nome    |
|-------|---------|
| 1     | Ativo   |
| 2     | Inativo |
| 3     | Evadido |

### DocumentType
| Valor | Nome                   |
|-------|------------------------|
| 1     | Cpf                    |
| 2     | RegistroEstrangeiro    |
| 3     | IdInterno              |

### ClassStatus
| Valor | Nome     |
|-------|----------|
| 1     | Active   |
| 2     | Inactive |

### AIRequestStatus
| Valor | Nome       |
|-------|------------|
| 1     | Pending    |
| 2     | Processing |
| 3     | Completed  |
| 4     | Failed     |

### AIRequestType
| Valor | Nome                 |
|-------|----------------------|
| 1     | LessonPlan           |
| 2     | Activity             |
| 3     | Report               |
| 4     | Summarization        |
| 5     | TextReformulation    |
| 6     | ParentCommunication  |

---

## 5. Padrões de Resposta

### Resposta de Paginação (`PagedResult<T>`)

Todos os endpoints de listagem retornam este envelope:

```json
{
  "items": [],
  "totalCount": 0,
  "page": 1,
  "pageSize": 10,
  "totalPages": 0,
  "hasNextPage": false,
  "hasPreviousPage": false
}
```

> Mudança: além de `totalPages`, o envelope também expõe `hasNextPage` e `hasPreviousPage`.

### Resposta de Criação

Todos os endpoints `POST` de criação retornam:

```json
{ "id": "guid" }
```

### Resposta de Erro (400 Bad Request)

```json
{
  "errors": ["mensagem de validação 1", "mensagem de validação 2"]
}
```

### Resposta de Erro (404 Not Found)

```json
{
  "errors": ["recurso não encontrado"]
}
```

---

## 6. Autenticação — Fluxo Completo

1. **Registrar**: `POST /api/v1/auth/register` → recebe `token`
2. **Login**: `POST /api/v1/auth/login` → recebe `token`
3. **Usar token**: adicionar header `Authorization: Bearer <token>` em todas as requisições protegidas
4. **Expiração**: 8 horas; após isso, refazer login

O payload do JWT contém:
- `nameidentifier`: `userId` (Guid)
- `name`: nome do usuário
- `email`: e-mail do usuário
- `role`: nome da role (`Professor`, `Diretor` ou `Coordenador`)

> Mudança: controllers usam `ClaimTypes.NameIdentifier` para identificar o usuário autenticado.

---

## 7. Modelo de Dados — Relacionamentos

```
User (role=Professor) (1) ──── (N) LessonPlan
User (role=Professor) (1) ──── (N) Activity
User (role=Professor) (1) ──── (N) PedagogicalReport
User (role=Professor) (1) ──── (N) AIRequest
LessonPlan (1) ──── (N) Activity (opcional, via lessonPlanId)
AIRequest (1) ──── (1) AIResponse
SchoolClass (1) ──── (N) Student
SchoolClass (N) ──── (N) User (role=Professor), via ClassTeachers
```

> Mudança: a tabela `Teachers` foi removida. A associação entre turmas e professores agora usa `ClassTeachers` apontando para `Users`.

---

## 8. Recursos com IA

Os endpoints de IA (`/generate`) consomem a OpenAI automaticamente. O modelo padrão é `gpt-4o-mini` com limite de 2000 tokens por requisição.

### Tipos de Geração Disponíveis

| Endpoint                                       | O que a IA gera                                |
|------------------------------------------------|------------------------------------------------|
| `POST /api/v1/lessonplans/generate`            | Plano de aula completo com conteúdo pedagógico |
| `POST /api/v1/activities/generate`             | Atividade com gabarito e versão simplificada   |
| `POST /api/v1/reports/generate`                | Relatório pedagógico completo do aluno         |
| `POST /api/v1/reports/{id}/summarize`          | Resumo executivo do relatório                  |
| `POST /api/v1/reports/{id}/parent-communication` | Texto para comunicação com pais/responsáveis |

### Resposta de Custo da IA

Endpoints de IA que retornam conteúdo diretamente, como resumo e comunicação com responsáveis, retornam metadados de uso. Endpoints `/generate` persistem o recurso gerado e retornam `{ "id": "guid" }`.

```json
{
  "tokensUsed": 850,
  "estimatedCost": 0.001275
}
```

---

## 9. Segurança e LGPD

- **Documentos dos alunos** são armazenados em formato completo no banco, mas retornados mascarados na API (`documentIdMasked`: `"XXX***XXXX"`).
- **Soft delete / inativação lógica**: entidades pedagógicas usam `DeletedAt`; turmas usam `Status = Inactive`.
- **Concorrência otimista**: prevista, mas não há `RowVersion`/concurrency token configurado no estado atual do código.
- **Dados de alunos na IA**: regra de produto exige sanitização; no estado atual, geração de relatório/comunicação ainda recebe `studentName` no prompt.
- **Todas as respostas da IA** são logadas com tokens utilizados e custo estimado.
- **Escopo por usuário**: consultas por ID, atualização, exclusão, publicação, arquivamento e ações de IA dos módulos pedagógicos validam o proprietário pelo `userId` do JWT.
- **CORS local**: a API aceita origens `localhost` e `127.0.0.1` pela policy `AllowLocalhost`.

---

## 10. Importação em Lote — Formato CSV

O endpoint `POST /api/v1/students/import` aceita arquivos CSV com as seguintes colunas:

```
fullName,documentType,documentId,birthDate,classId,enrollmentDate,notes
"João da Silva",1,"123.456.789-00","2010-03-15","<guid-da-turma>","2026-02-01",""
```

- `documentType`: número inteiro (1, 2 ou 3)
- `birthDate` e `enrollmentDate`: formato `YYYY-MM-DD`
- `classId`: UUID da turma
- Linhas com erros são ignoradas e reportadas no campo `errors` da resposta

---

## 11. Módulos Implementados vs. Pendentes

| Módulo                            | Status        | Controller              |
|-----------------------------------|---------------|-------------------------|
| Autenticação / Login              | ✅ Completo   | `AuthController`        |
| Professores (listagem + perfil)   | ✅ Completo   | `TeachersController`    |
| Planos de Aula                    | ✅ Completo   | `LessonPlansController` |
| Atividades                        | ✅ Completo   | `ActivitiesController`  |
| Relatórios Pedagógicos            | ✅ Completo   | `ReportsController`     |
| Alunos                            | ✅ Completo   | `StudentsController`    |
| Turmas / Classes                  | ✅ Completo   | `SchoolClassesController` |
| Histórico de IA                   | ✅ Completo   | `AIController`          |
| Importação CSV de Alunos          | ✅ Completo   | `StudentsController`    |
| Dashboard / Analytics             | ⬜ Pendente   | —                       |
| Notificações                      | ⬜ Pendente   | —                       |
| Comunicação escola-família (push) | ⬜ Pendente   | —                       |
| Gestão de múltiplas escolas       | ⬜ Pendente   | —                       |
| Módulo anti-evasão                | ⬜ Pendente   | —                       |

---

## 12. Estrutura de Projetos

```
SiaedBackend/
├── Siaed.Api/                          # Camada HTTP (Controllers, Middlewares)
│   ├── Controllers/
│   │   ├── AuthController.cs
│   │   ├── TeachersController.cs
│   │   ├── LessonPlansController.cs
│   │   ├── ActivitiesController.cs
│   │   ├── ReportsController.cs
│   │   ├── StudentsController.cs
│   │   ├── SchoolClassesController.cs
│   │   └── AIController.cs
│   └── Middlewares/
│       └── ExceptionHandlingMiddleware.cs
│
├── Siaed.Application/                  # Casos de uso (CQRS + MediatR)
│   ├── Features/
│   │   ├── Auth/                       # Login, Register
│   │   ├── LessonPlans/                # CRUD + Generate
│   │   ├── Activities/                 # CRUD + Generate
│   │   ├── Reports/                    # CRUD + Generate + Summarize + ParentComm
│   │   ├── Students/                   # CRUD + Transfer + Import
│   │   ├── SchoolClasses/              # CRUD
│   │   ├── Teachers/                   # List + GetMe (baseado em User role=Professor)
│   │   └── AI/                         # GetRequests
│   ├── Common/
│   │   ├── Result.cs                   # Result Pattern
│   │   └── PagedResult.cs              # Paginação
│   └── Interfaces/                     # Contratos de repositórios e serviços
│
├── Siaed.Domain/                       # Entidades, Enums, Regras de Negócio
│   ├── Entities/                       # User, Student, SchoolClass,
│   │                                   # LessonPlan, Activity, PedagogicalReport,
│   │                                   # AIRequest, AIResponse
│   └── Enums/                          # Todos os enums listados na seção 4
│
└── Siaed.Infra/                        # Implementações (EF Core, OpenAI, JWT)
    ├── Persistence/                    # DbContext (MySQL)
    ├── Repositories/                   # Implementações dos repositórios
    ├── OpenAI/                         # OpenAIService, PromptBuilderService
    ├── Identity/                       # JwtService, PasswordHasher
    └── Migrations/                     # Migrations do EF Core
```


