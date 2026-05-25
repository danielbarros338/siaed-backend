# Quickstart: Gerenciamento de Notas por Atividade

## 1. Preparacao

1. Restaurar dependencias:
   - dotnet restore
2. Build da solucao:
   - dotnet build
3. Criar migration da tabela Grades:
   - dotnet ef migrations add AddGradesTable --project Siaed.Infra --startup-project Siaed.Api
4. Aplicar migration:
   - dotnet ef database update --project Siaed.Infra --startup-project Siaed.Api
5. Executar API:
   - dotnet run --project Siaed.Api

## 2. Fluxo minimo de validacao manual

1. Criar ou obter uma atividade existente com turma e professor.
2. Criar notas (POST /api/v1/grades) para alguns alunos da atividade.
   - Exemplo de convencao com valores permitidos: `QUALITATIVA:MB,B,R`
3. Listar notas com filtros (GET /api/v1/grades?schoolClassId=...&teacherId=...&gradeValue=...).
4. Consultar nota por id (GET /api/v1/grades/{id}).
5. Atualizar nota com controle de versao (PUT /api/v1/grades/{id}).
6. Excluir logicamente (DELETE /api/v1/grades/{id}).
7. Validar subsecao da atividade (GET /api/v1/activities/{activityId}/grades).

## 3. Casos de aceite prioritarios

1. Nota string valida (ex.: MB) persiste com sucesso.
2. Nota string fora da convencao da atividade retorna erro de validacao.
3. Usuario nao autorizado (fora professor da turma/coordenador) recebe erro de autorizacao.
4. Conflito de concorrencia em update retorna erro sem sobrescrita silenciosa.
5. Filtros por turma, professor e nota retornam subconjunto correto.

## 4. Estrategia de testes

1. Unit tests (Domain/Application):
   - invariantes de Grade e validacao de convencao.
   - regras de autorizacao e bloqueio por status de atividade.
2. Integration tests (Infra/API):
   - persistencia de Grade + soft delete + filtros.
   - concorrencia otimista por RowVersion.
3. Contract tests:
   - payloads de CRUD e codigos HTTP conforme contratos.
