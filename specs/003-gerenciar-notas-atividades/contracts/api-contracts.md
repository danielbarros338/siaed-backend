# API Contracts: Gerenciamento de Notas por Atividade

**Phase**: 1 | **Date**: 2026-05-25 | **Feature**: 003-gerenciar-notas-atividades

**Base URL**: /api/v1

**Auth**: JWT Bearer Token

## Regras de acesso

- Escrita (create/update/delete): professor da turma da atividade e coordenador.
- Leitura (list/get): professor da turma da atividade, coordenador e diretor.

## Recurso Grade (/grades)

### POST /api/v1/grades

Cria um registro de nota para aluno em atividade.

Request body:

```json
{
  "activityId": "b9f4c55c-5a9b-4d8e-998f-7372fd7ee201",
  "studentId": "3f8ad03a-c037-4cc0-a4fb-72f6a8bbf623",
  "schoolClassId": "e22f9d97-8f88-4e81-b817-1db8a7366e1a",
  "teacherId": "0ac6d3fd-af18-49d7-83e2-9cb89a3a65c2",
  "gradeValue": "MB",
  "conventionKey": "2026-FUND1-QUALITATIVA"
}
```

Response 201 Created:

```json
{
  "id": "guid",
  "activityId": "guid",
  "studentId": "guid",
  "schoolClassId": "guid",
  "teacherId": "guid",
  "gradeValue": "MB",
  "conventionKey": "2026-FUND1-QUALITATIVA",
  "version": "AAAAAAAAB9E="
}
```

Response 400 Bad Request:

```json
{
  "errors": [
    "O valor da nota nao pertence a convencao permitida da atividade."
  ]
}
```

Response 403 Forbidden:

```json
{
  "errors": [
    "Usuario sem permissao para lancar nota nesta atividade."
  ]
}
```

### GET /api/v1/grades/{id}

Retorna uma nota por id.

Response 200 OK:

```json
{
  "id": "guid",
  "activityId": "guid",
  "studentId": "guid",
  "schoolClassId": "guid",
  "teacherId": "guid",
  "gradeValue": "8.5",
  "conventionKey": "2026-FUND1-NUMERICA",
  "createdAt": "2026-05-25T15:10:00Z",
  "updatedAt": "2026-05-25T15:15:00Z",
  "version": "AAAAAAAAB9E="
}
```

Response 404 Not Found:

```json
{
  "errors": [
    "Nota nao encontrada."
  ]
}
```

### GET /api/v1/grades

Lista notas com filtros por turma, professor e valor de nota.

Query params:
- page (int, default 1)
- pageSize (int, default 20, max 100)
- activityId (guid, opcional)
- schoolClassId (guid, opcional)
- teacherId (guid, opcional)
- gradeValue (string, opcional)

Response 200 OK:

```json
{
  "items": [
    {
      "id": "guid",
      "activityId": "guid",
      "studentId": "guid",
      "studentName": "Aluno Exemplo",
      "schoolClassId": "guid",
      "teacherId": "guid",
      "gradeValue": "A",
      "conventionKey": "2026-FUND1-QUALITATIVA",
      "updatedAt": "2026-05-25T15:15:00Z"
    }
  ],
  "totalCount": 32,
  "page": 1,
  "pageSize": 20,
  "totalPages": 2
}
```

### PUT /api/v1/grades/{id}

Atualiza valor da nota (concorrencia otimista por versao).

Request body:

```json
{
  "gradeValue": "R",
  "conventionKey": "2026-FUND1-QUALITATIVA",
  "version": "AAAAAAAAB9E="
}
```

Response 200 OK:

```json
{
  "id": "guid",
  "activityId": "guid",
  "studentId": "guid",
  "schoolClassId": "guid",
  "teacherId": "guid",
  "gradeValue": "R",
  "conventionKey": "2026-FUND1-QUALITATIVA",
  "createdAt": "2026-05-25T15:10:00Z",
  "updatedAt": "2026-05-25T15:30:00Z",
  "version": "AAAAAAAACAA="
}
```

Response 409 Conflict:

```json
{
  "errors": [
    "Conflito de concorrencia detectado. Recarregue a nota antes de salvar novamente."
  ]
}
```

### DELETE /api/v1/grades/{id}

Remove logicamente uma nota (soft delete).

Response 204 No Content

Response 403 Forbidden:

```json
{
  "errors": [
    "Usuario sem permissao para excluir nota nesta atividade."
  ]
}
```

Response 404 Not Found:

```json
{
  "errors": [
    "Nota nao encontrada."
  ]
}
```

## Endpoint de subsecao em atividade (navegacao)

### GET /api/v1/activities/{activityId}/grades

Retorna a subsecao de notas da atividade, incluindo alunos atuais sem nota e historico de notas ja lancadas.

Response 200 OK:

```json
{
  "activityId": "guid",
  "schoolClassId": "guid",
  "items": [
    {
      "studentId": "guid",
      "studentName": "Aluno A",
      "gradeId": "guid",
      "gradeValue": "MB",
      "hasGrade": true,
      "isHistorical": false
    },
    {
      "studentId": "guid",
      "studentName": "Aluno Novo",
      "gradeId": null,
      "gradeValue": null,
      "hasGrade": false,
      "isHistorical": false
    },
    {
      "studentId": "guid",
      "studentName": "Aluno Transferido",
      "gradeId": "guid",
      "gradeValue": "A",
      "hasGrade": true,
      "isHistorical": true
    }
  ]
}
```
