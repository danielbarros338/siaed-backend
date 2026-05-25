# Data Model: Gerenciamento de Notas por Atividade

**Phase**: 1 | **Date**: 2026-05-25 | **Feature**: 003-gerenciar-notas-atividades

## Entidades

### Grade (nova tabela independente)

Description: Registro de nota de um aluno em uma atividade.

Fields:
- Id: Guid (PK)
- ActivityId: Guid (FK, obrigatorio)
- StudentId: Guid (FK, obrigatorio)
- SchoolClassId: Guid (FK, obrigatorio)
- TeacherId: Guid (FK, obrigatorio, professor responsavel pela atividade)
- GradeValue: string(30) (obrigatorio)
- ConventionKey: string(100) (obrigatorio, identifica convencao da escola/atividade)
- Version: byte[] (RowVersion para concorrencia otimista)
- CreatedAt: DateTime
- UpdatedAt: DateTime
- DeletedAt: DateTime? (soft delete)

Uniqueness and indexes:
- UX_Grades_Activity_Student_Active: unico por (ActivityId, StudentId, DeletedAt null)
- IX_Grades_SchoolClassId
- IX_Grades_TeacherId
- IX_Grades_GradeValue
- IX_Grades_ActivityId

Validation rules:
- GradeValue nao pode ser vazio.
- GradeValue deve pertencer ao conjunto permitido pela ConventionKey da atividade.
- Nao permitir create/update se atividade estiver bloqueada para edicao.
- Somente professor da turma da atividade ou coordenador pode escrever.

State transitions:
- Create: registro ativo criado.
- Update: atualiza GradeValue com controle de versao.
- Delete: soft delete (preenche DeletedAt).

### Activity (existente, impacto)

Description: Unidade avaliativa vinculada a turma e professor.

Impacto na feature:
- Fonte da regra de editabilidade (draft/published/archived).
- Fonte da ConventionKey para validacao de GradeValue.

### Student (existente, impacto)

Description: Aluno da turma elegivel para nota.

Impacto na feature:
- Participa da chave logica da nota por atividade.
- Historico de nota deve ser preservado mesmo se sair da turma depois do lancamento.

### SchoolClass (existente, impacto)

Description: Turma vinculada a atividade.

Impacto na feature:
- Filtro primario de listagem de notas.

## Relacionamentos

- Activity 1:N Grade
- Student 1:N Grade
- SchoolClass 1:N Grade
- Teacher 1:N Grade

## Regras de negocio refletidas no modelo

- Nota e string, nao decimal.
- Convencao de nota e contextual (ConventionKey), nao global fixa.
- Novos alunos apos lancamento inicial aparecem sem nota obrigatoria retroativa.
- Notas de alunos que sairam da turma permanecem para historico.
- Conflito de versao retorna erro de concorrencia e exige recarga.
