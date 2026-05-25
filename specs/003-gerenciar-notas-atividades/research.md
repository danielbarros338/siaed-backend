# Research: Gerenciamento de Notas por Atividade

**Phase**: 0 | **Date**: 2026-05-25 | **Feature**: 003-gerenciar-notas-atividades

## 1. Persistencia em tabela independente Grade

Decision: Criar entidade de dominio Grade com tabela independente Grades, vinculada a Activity, Student e SchoolClass.

Rationale: O usuario solicitou CRUD completo de notas e filtros por turma/professor/nota. Isolar em tabela propria melhora rastreabilidade, auditoria, desempenho de consulta e evita sobrecarga na entidade Activity.

Alternatives considered:
- Armazenar notas como JSON dentro de Activity: rejeitado por baixa auditabilidade e filtros ineficientes.
- Armazenar notas em StudentActivity join sem agregado proprio: rejeitado por dificultar regras de negocio e evolucao de historico.

## 2. Valor da nota em string

Decision: Persistir GradeValue como string (max 30), com validacao por convencao da atividade.

Rationale: Algumas escolas usam valores numericos; outras usam literais (MB, A, R). O tipo string atende os dois cenarios sem conversoes artificiais.

Alternatives considered:
- decimal: rejeitado por nao suportar letras.
- enum fixo: rejeitado por nao cobrir variacoes por escola.

## 3. Autorizacao de escrita

Decision: Permitir create/update/delete de nota apenas para professor da turma da atividade e para coordenador.

Rationale: Regra definida na clarificacao e alinhada ao principio de menor privilegio.

Alternatives considered:
- somente professor: rejeitado por limitar operacao administrativa.
- incluir diretor em escrita: rejeitado nesta feature para manter escopo da regra acordada.

## 4. Concorrencia em edicao simultanea

Decision: Adotar concorrencia otimista por versao (RowVersion) e rejeitar gravacao desatualizada.

Rationale: Evita sobrescrita silenciosa e preserva integridade pedagogica.

Alternatives considered:
- last write wins: rejeitado por risco de perda de dados.
- lock pessimista: rejeitado por aumentar contencao sem necessidade.

## 5. Endpoints e padrao de API

Decision: Expor CRUD de Grade em controller dedicado GradesController, com suporte a escopo de atividade e filtros combinados.

Rationale: Atende pedido de CRUD explicito e aproveita padrao existente do projeto (api/v1/[controller], MediatR, Result Pattern).

Alternatives considered:
- sub-rotas apenas em ActivitiesController: rejeitado para evitar controller inchado e simplificar manutencao da feature.

## 6. Filtros por turma, professor e nota

Decision: ListGradesQuery recebe filtros opcionais: activityId, schoolClassId, teacherId, gradeValue e status de registro.

Rationale: Cobre o requisito de busca operacional sem multiplicar endpoints.

Alternatives considered:
- endpoint dedicado por filtro: rejeitado por aumentar superficie de API sem ganho funcional.

## 7. Observacao de nomenclatura Grade x Activity.Grade

Decision: Manter campo Activity.Grade (ano/série) e introduzir entidade Grade (nota), documentando significado distinto no modelo.

Rationale: Evita breaking change desnecessario na entidade Activity existente, mantendo evolucao incremental.

Alternatives considered:
- renomear Activity.Grade agora: rejeitado por impacto amplo fora do escopo da feature.

## 8. Status da implementacao

Decision: Implementacao realizada com entidade `Grade`, repositorio dedicado, handlers CQRS, `GradesController`, endpoint de subsecao em `ActivitiesController` e migration `AddGradesTable`.

Rationale: Entrega o CRUD completo, filtros por turma/professor/nota, concorrencia otimista por versao e historico de notas na atividade.

Validation executed:
- `dotnet build` executado com sucesso em toda a solucao.
- `dotnet ef migrations add AddGradesTable --project Siaed.Infra --startup-project Siaed.Api` executado com sucesso (migration gerada).

Known limitation:
- Validacao manual ponta a ponta via HTTP depende de banco MySQL configurado e API em execucao; neste ambiente, foi validada apenas compilacao e geracao de migration.
