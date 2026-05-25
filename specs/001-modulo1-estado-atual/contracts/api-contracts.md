# API Contracts: Módulo 1 — Endpoints Novos e Modificados

**Branch**: `001-modulo1-estado-atual` | **Data**: 2025-07-17

> Apenas endpoints **novos** ou **modificados** estão documentados. Endpoints já existentes e funcionais não foram alterados.

---

## Auth — Modificado

### `POST /api/v1/auth/register`

**Motivo da modificação**: Adição de campos para criação automática de `Teacher` quando `Role == Professor`.

**Request Body**:
```json
{
  "name": "João da Silva",
  "email": "joao@escola.com.br",
  "password": "SenhaForte@123",
  "role": "Professor",
  "subject": "Matemática",
  "schoolId": "escola-abc-001"
}
```

**Campos novos**:
| Campo | Tipo | Obrigatório para | Descrição |
|---|---|---|---|
| `subject` | string | `Professor` | Disciplina principal |
| `schoolId` | string | `Professor` | Identificador da escola |

**Campos obrigatórios quando `role == Professor`**: `subject`, `schoolId`.  
**Campos ignorados quando `role == Diretor` ou `Coordenador`**.

**Response (sem alteração)**:
```json
{
  "userId": "...",
  "name": "...",
  "email": "...",
  "role": "Professor",
  "token": "eyJ...",
  "expiresAt": "2025-07-18T..."
}
```

---

## LessonPlans — Endpoints novos

### `DELETE /api/v1/lessonplans/{id}`

**Autenticação**: `Bearer Token` (Role: qualquer)  
**Autorização**: Apenas o professor dono do plano.

**Path Parameters**:
| Param | Tipo | Descrição |
|---|---|---|
| `id` | Guid | ID do plano de aula |

**Response**:
- `204 No Content` — soft delete realizado com sucesso
- `404 Not Found` — plano não encontrado ou não pertence ao professor autenticado
- `401 Unauthorized` — sem token válido

---

### `PATCH /api/v1/lessonplans/{id}/publish`

**Autenticação**: `Bearer Token`  
**Autorização**: Apenas o professor dono do plano.

**Request Body**: vazio

**Response**:
- `204 No Content` — status alterado para `Published`
- `400 Bad Request` — transição inválida (ex: plano já `Archived`)
- `404 Not Found` — plano não encontrado ou não pertence ao professor

---

### `PATCH /api/v1/lessonplans/{id}/archive`

**Autenticação**: `Bearer Token`  
**Autorização**: Apenas o professor dono do plano.

**Request Body**: vazio

**Response**:
- `204 No Content` — status alterado para `Archived`
- `400 Bad Request` — transição inválida
- `404 Not Found` — plano não encontrado ou não pertence ao professor

---

## Activities — Endpoints novos

### `DELETE /api/v1/activities/{id}`

Idêntico ao DELETE de LessonPlan. Retorna `204`, `404` ou `401`.

---

### `PATCH /api/v1/activities/{id}/publish`

Idêntico ao PATCH publish de LessonPlan.

---

### `PATCH /api/v1/activities/{id}/archive`

Idêntico ao PATCH archive de LessonPlan.

---

## Reports — Endpoints novos

### `DELETE /api/v1/reports/{id}`

Idêntico ao DELETE de LessonPlan.

---

### `POST /api/v1/reports/{id}/summarize`

**Autenticação**: `Bearer Token`  
**Autorização**: Apenas o professor dono do relatório.

**Descrição**: Usa IA para gerar um resumo executivo do relatório pedagógico. Salva o resumo em `PedagogicalReport.Summary`. Registra `AIRequest`/`AIResponse` com tipo `Summarization (4)`.

**Request Body**: vazio (usa conteúdo existente do relatório)

**Response**:
```json
{
  "reportId": "...",
  "summary": "Resumo gerado pela IA...",
  "tokensUsed": 350,
  "estimatedCost": 0.000053
}
```

**Códigos**:
- `200 OK` — resumo gerado e salvo
- `400 Bad Request` — relatório sem conteúdo; falha na IA
- `404 Not Found` — relatório não encontrado

---

### `POST /api/v1/reports/{id}/parent-communication`

**Autenticação**: `Bearer Token`  
**Autorização**: Apenas o professor dono do relatório.

**Descrição**: Usa IA para gerar um comunicado para os pais com base no relatório pedagógico. Salva em `PedagogicalReport.ParentCommunication`. Registra `AIRequest`/`AIResponse` com tipo `ParentCommunication (6)`.

**Request Body**: vazio

**Response**:
```json
{
  "reportId": "...",
  "parentCommunication": "Prezados pais...",
  "tokensUsed": 420,
  "estimatedCost": 0.000063
}
```

**Códigos**: Idênticos ao summarize.

---

## Teacher — Controller novo

### `GET /api/v1/teachers/me`

**Autenticação**: `Bearer Token`  
**Autorização**: Role `Professor`.

**Descrição**: Retorna o perfil do professor autenticado, resolvendo via `UserId` extraído do JWT.

**Response**:
```json
{
  "id": "...",
  "userId": "...",
  "name": "João da Silva",
  "email": "joao@escola.com.br",
  "subject": "Matemática",
  "schoolId": "escola-abc-001",
  "createdAt": "2025-07-17T..."
}
```

**Códigos**:
- `200 OK` — perfil retornado
- `404 Not Found` — Teacher não associado ao usuário autenticado (estado temporário para usuários registrados antes desta feature)
- `401 Unauthorized` — sem token
- `403 Forbidden` — Role não é `Professor`

---

## AI — Histórico de uso (L-008)

### `GET /api/v1/ai/requests`

**Autenticação**: `Bearer Token`  
**Autorização**: Professor vê apenas seus próprios requests. `Diretor`/`Coordenador` veem todos (futuro).

**Query Parameters**:
| Param | Tipo | Padrão | Descrição |
|---|---|---|---|
| `page` | int | 1 | Página |
| `pageSize` | int | 20 | Itens por página (máx: 100) |

**Response**:
```json
{
  "items": [
    {
      "id": "...",
      "type": "LessonPlanGeneration",
      "status": "Completed",
      "model": "gpt-4o-mini",
      "promptTokens": 800,
      "completionTokens": 1200,
      "estimatedCost": 0.00048,
      "createdAt": "2025-07-17T..."
    }
  ],
  "totalCount": 42,
  "page": 1,
  "pageSize": 20,
  "totalPages": 3
}
```

---

## Mudanças nos endpoints existentes (ownership fix — D-005)

Os endpoints abaixo **não mudam seu contrato externo** (mesma URL, mesmo body, mesma response), mas recebem correção interna de ownership:

| Endpoint | Mudança |
|---|---|
| `GET /api/v1/lessonplans/{id}` | Handler passa a verificar `TeacherId` do usuário logado |
| `PUT /api/v1/lessonplans/{id}` | Handler passa a verificar `TeacherId` do usuário logado |
| `GET /api/v1/activities/{id}` | Handler passa a verificar `TeacherId` do usuário logado |
| `PUT /api/v1/activities/{id}` | Handler passa a verificar `TeacherId` do usuário logado |
| `GET /api/v1/reports/{id}` | Handler passa a verificar `TeacherId` do usuário logado |
| `PUT /api/v1/reports/{id}` | Handler passa a verificar `TeacherId` do usuário logado |

**Comportamento após fix**: Tentar acessar recurso de outro professor retorna `404` (não revela existência do recurso).

---

## Resumo dos novos endpoints

| Método | Endpoint | Prioridade | Lacuna |
|---|---|---|---|
| DELETE | `/api/v1/lessonplans/{id}` | Alta | L-001 |
| PATCH | `/api/v1/lessonplans/{id}/publish` | Média | L-002 |
| PATCH | `/api/v1/lessonplans/{id}/archive` | Média | L-002 |
| DELETE | `/api/v1/activities/{id}` | Alta | L-001 |
| PATCH | `/api/v1/activities/{id}/publish` | Média | L-002 |
| PATCH | `/api/v1/activities/{id}/archive` | Média | L-002 |
| DELETE | `/api/v1/reports/{id}` | Alta | L-001 |
| POST | `/api/v1/reports/{id}/summarize` | Média | L-006 |
| POST | `/api/v1/reports/{id}/parent-communication` | Média | L-006 |
| GET | `/api/v1/teachers/me` | Alta | L-004 |
| GET | `/api/v1/ai/requests` | Baixa | L-008 |
