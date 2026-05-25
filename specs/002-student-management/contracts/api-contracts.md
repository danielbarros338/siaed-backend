# API Contracts: Gerenciamento de Alunos

**Phase**: 1 | **Date**: 2026-05-18 | **Feature**: 002-student-management

**Base URL**: `/api/v1`

**Auth**: JWT Bearer Token — `Authorization: Bearer <token>`

**Roles**:
- `Professor` — leitura
- `Coordenador` — leitura + escrita (exceto exclusão definitiva — não existe)
- `Diretor` — acesso completo

---

## Alunos (`/students`)

---

### POST /api/v1/students

Cria um novo aluno.

**Roles**: `Coordenador`, `Diretor`

**Request Body**:
```json
{
  "fullName": "João da Silva",
  "documentType": 1,
  "documentId": "12345678901",
  "birthDate": "2010-05-15",
  "turmaId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "enrollmentDate": "2024-02-01",
  "notes": "Observação opcional"
}
```

| Campo | Tipo | Obrigatório | Validação |
|-------|------|-------------|-----------|
| `fullName` | `string` | ✅ | 1–200 chars |
| `documentType` | `int` (enum) | ✅ | 1=CPF, 2=RegistroEstrangeiro, 3=IdInterno |
| `documentId` | `string` | ✅ | 1–50 chars; deve ser único na escola |
| `birthDate` | `date` | ✅ | Não futura |
| `turmaId` | `guid` | ✅ | Deve referenciar turma ativa |
| `enrollmentDate` | `date` | ✅ | Não futura |
| `notes` | `string?` | ❌ | Máx. 2000 chars |

**Response 201 Created**:
```json
{
  "id": "guid",
  "fullName": "João da Silva",
  "status": 1,
  "turmaId": "guid",
  "turmaName": "3º Ano A",
  "enrollmentDate": "2024-02-01"
}
```

**Response 400 Bad Request** (validação):
```json
{
  "errors": ["O campo FullName é obrigatório.", "A Turma informada não existe ou está inativa."]
}
```

**Response 409 Conflict** (documento duplicado):
```json
{
  "errors": ["Já existe um aluno com o documento '12345678901' nesta escola."]
}
```

---

### PUT /api/v1/students/{id}

Atualiza dados cadastrais de um aluno.

**Roles**: `Coordenador`, `Diretor`

**Path Params**: `id` (guid)

**Request Body**:
```json
{
  "fullName": "João da Silva Atualizado",
  "birthDate": "2010-05-15",
  "turmaId": "guid",
  "notes": "Observação atualizada"
}
```

**Response 200 OK**: `StudentDetailDto` atualizado.

**Response 404 Not Found**:
```json
{ "errors": ["Aluno não encontrado."] }
```

---

### PATCH /api/v1/students/{id}/deactivate

Inativa ou marca como evadido um aluno ativo.

**Roles**: `Coordenador`, `Diretor`

**Path Params**: `id` (guid)

**Request Body**:
```json
{
  "newStatus": 2,
  "reason": "Transferência para outra escola"
}
```

| Campo | Tipo | Obrigatório | Validação |
|-------|------|-------------|-----------|
| `newStatus` | `int` (enum) | ✅ | 2=Inativo, 3=Evadido (não aceita 1=Ativo) |
| `reason` | `string?` | ❌ | Máx. 500 chars |

**Response 200 OK**:
```json
{ "id": "guid", "status": 2 }
```

**Response 422 Unprocessable Entity** (aluno já inativo):
```json
{ "errors": ["O aluno já está inativo."] }
```

---

### PATCH /api/v1/students/{id}/transfer

Transfere um aluno para outra turma.

**Roles**: `Coordenador`, `Diretor`

**Path Params**: `id` (guid)

**Request Body**:
```json
{
  "newTurmaId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

**Response 200 OK**:
```json
{ "id": "guid", "turmaId": "guid", "turmaName": "4º Ano B" }
```

**Response 422 Unprocessable Entity**:
```json
{ "errors": ["Não é possível transferir um aluno inativo."] }
```

---

### GET /api/v1/students

Lista alunos com paginação e filtros.

**Roles**: `Professor`, `Coordenador`, `Diretor`

**Query Params**:

| Param | Tipo | Padrão | Descrição |
|-------|------|--------|-----------|
| `page` | `int` | `1` | Número da página (≥ 1) |
| `pageSize` | `int` | `20` | Itens por página (máx. 100) |
| `turmaId` | `guid?` | — | Filtrar por turma |
| `status` | `int?` | — | Filtrar por status (1, 2 ou 3) |
| `search` | `string?` | — | Busca por nome ou documento (parcial) |

**Response 200 OK**:
```json
{
  "items": [
    {
      "id": "guid",
      "fullName": "João da Silva",
      "documentType": 1,
      "documentId": "123***8901",
      "birthDate": "2010-05-15",
      "turmaId": "guid",
      "turmaName": "3º Ano A",
      "status": 1,
      "enrollmentDate": "2024-02-01"
    }
  ],
  "totalCount": 150,
  "page": 1,
  "pageSize": 20,
  "totalPages": 8
}
```

> **Nota LGPD**: O `documentId` é mascarado na listagem (ex: `123***8901`). O detalhe individual retorna o valor completo.

---

### GET /api/v1/students/{id}

Retorna detalhes completos de um aluno.

**Roles**: `Professor`, `Coordenador`, `Diretor`

**Response 200 OK**: `StudentDetailDto` completo (sem mascaramento de documento).

**Response 404 Not Found**:
```json
{ "errors": ["Aluno não encontrado."] }
```

---

### POST /api/v1/students/import

Importa alunos em lote via CSV.

**Roles**: `Coordenador`, `Diretor`

**Content-Type**: `multipart/form-data`

**Form Fields**:

| Campo | Tipo | Obrigatório | Descrição |
|-------|------|-------------|-----------|
| `file` | `IFormFile` | ✅ | Arquivo CSV (máx. 2MB) |
| `turmaId` | `guid` | ✅ | Turma destino para todos os alunos do CSV |
| `mode` | `string` | ✅ | `fail-fast` ou `partial` |

**Formato do CSV**:
```csv
FullName,DocumentType,DocumentId,BirthDate,EnrollmentDate,Notes
João da Silva,1,12345678901,2010-05-15,2024-02-01,
Maria Souza,1,98765432100,2011-03-22,2024-02-01,Observação opcional
```

**Response 200 OK** (mode=partial com erros):
```json
{
  "totalRows": 50,
  "successCount": 48,
  "failureCount": 2,
  "errors": [
    { "rowNumber": 12, "documentId": "12345678901", "reason": "Documento já cadastrado." },
    { "rowNumber": 31, "documentId": "", "reason": "O campo DocumentId é obrigatório." }
  ]
}
```

**Response 400 Bad Request** (mode=fail-fast com erros):
```json
{
  "totalRows": 50,
  "successCount": 0,
  "failureCount": 1,
  "errors": [
    { "rowNumber": 12, "documentId": "12345678901", "reason": "Documento já cadastrado." }
  ]
}
```

**Response 422 Unprocessable Entity** (arquivo inválido, turma inativa, etc.):
```json
{ "errors": ["A turma informada está inativa."] }
```

---

## Turmas (`/turmas`)

---

### POST /api/v1/turmas

Cria uma nova turma.

**Roles**: `Coordenador`, `Diretor`

**Request Body**:
```json
{
  "name": "3º Ano A",
  "grade": "3º Ano EF",
  "schoolYear": 2024
}
```

**Response 201 Created**: `TurmaDto` criada.

---

### PUT /api/v1/turmas/{id}

Atualiza dados de uma turma.

**Roles**: `Coordenador`, `Diretor`

**Request Body**: Mesmos campos do POST.

**Response 200 OK**: `TurmaDto` atualizada.

---

### PATCH /api/v1/turmas/{id}/deactivate

Inativa uma turma. Somente turmas sem alunos ativos podem ser inativadas.

**Roles**: `Coordenador`, `Diretor`

**Response 200 OK**:
```json
{ "id": "guid", "status": 2 }
```

**Response 422 Unprocessable Entity**:
```json
{ "errors": ["Não é possível inativar uma turma com alunos ativos. Transfira ou inative os alunos primeiro."] }
```

---

### GET /api/v1/turmas

Lista turmas com paginação.

**Roles**: `Professor`, `Coordenador`, `Diretor`

**Query Params**: `page`, `pageSize`, `status?`, `schoolYear?`, `search?`

**Response 200 OK**: `PagedResult<TurmaDto>` com `studentCount` e lista de professores resumida.

---

### GET /api/v1/turmas/{id}

Detalhe de uma turma.

**Roles**: `Professor`, `Coordenador`, `Diretor`

**Response 200 OK**: `TurmaDto` completo.

---

### POST /api/v1/turmas/{id}/teachers/{teacherId}

Vincula um professor a uma turma.

**Roles**: `Coordenador`, `Diretor`

**Response 200 OK**:
```json
{ "turmaId": "guid", "teacherId": "guid" }
```

**Response 409 Conflict**:
```json
{ "errors": ["O professor já está vinculado a esta turma."] }
```

---

### DELETE /api/v1/turmas/{id}/teachers/{teacherId}

Remove vínculo de professor de uma turma.

**Roles**: `Coordenador`, `Diretor`

**Response 204 No Content**

**Response 404 Not Found**:
```json
{ "errors": ["Vínculo não encontrado."] }
```

---

## Códigos de Status HTTP Utilizados

| Código | Significado | Quando usar |
|--------|-------------|-------------|
| 200 | OK | Operação de leitura ou atualização bem-sucedida |
| 201 | Created | Recurso criado com sucesso |
| 204 | No Content | Remoção bem-sucedida sem corpo de resposta |
| 400 | Bad Request | Erro de validação de entrada |
| 401 | Unauthorized | Token ausente ou inválido |
| 403 | Forbidden | Role insuficiente |
| 404 | Not Found | Recurso não encontrado |
| 409 | Conflict | Duplicação de recurso único |
| 422 | Unprocessable Entity | Regra de negócio violada (ex: transferir aluno inativo) |
| 500 | Internal Server Error | Erro não tratado (retornado pelo middleware) |
