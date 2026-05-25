# Feature Specification: Gerenciamento de Alunos

**Feature Branch**: `002-student-management`

**Created**: 2026-05-18

**Status**: Draft

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 — Cadastro de Aluno (Priority: P1)

Um Diretor ou Coordenador precisa cadastrar um novo aluno no sistema, vinculando-o obrigatoriamente a uma turma ativa. O aluno é uma entidade pedagógica central utilizada por relatórios, IA e alertas — por isso seu cadastro precisa ser completo e sem duplicidade.

**Why this priority**: Sem alunos cadastrados, nenhum outro módulo (relatórios pedagógicos, IA, alertas de evasão) pode funcionar. É o pré-requisito fundamental do sistema.

**Independent Test**: Pode ser totalmente testado criando um aluno com dados válidos e verificando que ele aparece listado com status Ativo na turma informada. Entrega valor imediato ao permitir rastrear quem faz parte de cada turma.

**Acceptance Scenarios**:

1. **Given** um Diretor autenticado e uma turma ativa existente, **When** ele submete os dados completos do novo aluno (nome, CPF, data de nascimento, data de matrícula, turmaId), **Then** o aluno é persistido com status Ativo, vinculado à turma, e um evento `StudentCreatedEvent` é emitido.
2. **Given** dados de um aluno já existente com o mesmo CPF, **When** o sistema tenta criar outro aluno com o mesmo CPF, **Then** a operação é rejeitada com mensagem de duplicidade.
3. **Given** um Coordenador autenticado, **When** ele tenta criar um aluno sem informar turmaId, **Then** a operação é rejeitada com mensagem de campo obrigatório.
4. **Given** um Professor autenticado (sem role de Diretor ou Coordenador), **When** ele tenta criar um aluno, **Then** a operação é rejeitada com erro de autorização (403).

---

### User Story 2 — Atualização de Dados do Aluno (Priority: P1)

Um Diretor ou Coordenador precisa corrigir ou atualizar informações cadastrais de um aluno existente (nome, data de nascimento, documento).

**Why this priority**: Dados incorretos impactam diretamente relatórios pedagógicos e comunicações com responsáveis geradas via IA.

**Independent Test**: Pode ser testado atualizando o nome de um aluno existente e verificando que os dados novos são retornados na consulta seguinte, e que `UpdatedAt` foi modificado.

**Acceptance Scenarios**:

1. **Given** um Coordenador autenticado e um aluno Ativo existente, **When** ele atualiza o nome do aluno, **Then** os dados são persistidos, `UpdatedAt` é atualizado, e um evento `StudentUpdatedEvent` é emitido.
2. **Given** dados válidos de atualização, **When** o campo de documento é alterado para um CPF já pertencente a outro aluno, **Then** a operação é rejeitada com mensagem de duplicidade.
3. **Given** um aluno com status Inativo, **When** um Diretor tenta atualizar seus dados, **Then** a operação é permitida (aluno inativo ainda pode ter dados corrigidos).

---

### User Story 3 — Transferência de Turma (Priority: P2)

Um Diretor ou Coordenador precisa mover um aluno de uma turma para outra (ex: reclassificação, ajuste de grade). A transferência substitui a turma anterior, e o histórico deve ser rastreável.

**Why this priority**: Turmas afetam diretamente planos de aula, atividades e relatórios pedagógicos. Uma transferência mal registrada compromete todo o histórico acadêmico.

**Independent Test**: Pode ser testado transferindo um aluno de uma turma A para uma turma B e verificando que o aluno aparece na turma B (e não mais na A), além de que um evento `StudentTransferredEvent` foi emitido.

**Acceptance Scenarios**:

1. **Given** um aluno Ativo na turma A e uma turma B ativa existente, **When** um Diretor executa a transferência, **Then** o `TurmaId` do aluno é atualizado para B, `UpdatedAt` é registrado, e um evento `StudentTransferredEvent` é emitido.
2. **Given** uma turma de destino com status Inativa, **When** a transferência é solicitada, **Then** a operação é rejeitada com mensagem indicando que a turma destino deve estar ativa.
3. **Given** um aluno com status Evadido, **When** a transferência é solicitada, **Then** a operação é rejeitada com mensagem indicando que alunos evadidos não podem ser transferidos.

---

### User Story 4 — Inativação de Aluno (Priority: P2)

Um Diretor ou Coordenador precisa registrar a saída de um aluno do sistema, seja por evasão, transferência para outra escola ou outros motivos, sem apagar seus dados históricos.

**Why this priority**: A preservação de histórico é obrigatória por regulamentação educacional e é fundamental para análises de evasão e relatórios.

**Independent Test**: Pode ser testado inativando um aluno e verificando que seu status muda, `DeletedAt` é preenchido, mas ele ainda aparece em consultas que incluem alunos inativos/evadidos.

**Acceptance Scenarios**:

1. **Given** um aluno com status Ativo, **When** um Diretor solicita inativação com motivo "Evadido", **Then** o status é alterado para Evadido, `DeletedAt` é preenchido, e um evento `StudentDeactivatedEvent` é emitido.
2. **Given** um aluno já com status Inativo, **When** a inativação é solicitada novamente, **Then** a operação é rejeitada com mensagem adequada.
3. **Given** que um aluno é inativado, **When** o sistema tenta listá-lo sem filtro de inativos, **Then** ele não aparece na listagem padrão, mas aparece quando o filtro incluir inativos.

---

### User Story 5 — Listagem e Consulta de Alunos (Priority: P1)

Um Professor, Coordenador ou Diretor precisa consultar a lista de alunos filtrados por turma, status ou critérios combinados, com paginação, para uso em relatórios e acompanhamento pedagógico.

**Why this priority**: A consulta de alunos é base para todas as operações dos outros módulos (geração de relatório, análise de IA, alertas de evasão).

**Independent Test**: Pode ser testado consultando a lista de alunos de uma turma específica com paginação e verificando que apenas alunos daquela turma retornam, respeitando o tamanho de página.

**Acceptance Scenarios**:

1. **Given** alunos cadastrados em múltiplas turmas, **When** a consulta é feita com filtro por turmaId, **Then** apenas alunos daquela turma são retornados, com paginação correta.
2. **Given** alunos com diferentes status, **When** a consulta filtra por status "Ativo", **Then** apenas alunos Ativos são retornados.
3. **Given** uma consulta sem filtros, **When** executada, **Then** apenas alunos com `DeletedAt` nulo (status Ativo) são retornados por padrão.
4. **Given** um Professor autenticado, **When** ele consulta a lista de alunos de sua turma, **Then** os dados são retornados sem informações pessoais sensíveis desnecessárias.

---

### User Story 6 — Importação de Alunos via CSV (Priority: P3)

Um Diretor ou Coordenador precisa importar uma lista de alunos em lote a partir de um arquivo CSV, com validação de duplicidade e geração de relatório de erros por linha.

**Why this priority**: Facilita a migração inicial de dados de ERPs externos, mas não é bloqueante para o uso do sistema.

**Independent Test**: Pode ser testado enviando um CSV com alunos válidos e inválidos e verificando que os válidos são persistidos e um relatório de erros é retornado para as linhas inválidas.

**Acceptance Scenarios**:

1. **Given** um CSV com 50 alunos válidos e 5 inválidos (CPF duplicado e campos faltantes), **When** importação em modo partial success é executada, **Then** 50 alunos são persistidos e um relatório com 5 erros identificando linha e motivo é retornado.
2. **Given** um CSV com ao menos 1 linha inválida, **When** importação em modo fail-fast é executada, **Then** nenhum aluno é persistido e o erro da primeira linha inválida é retornado.
3. **Given** um CSV malformado (colunas faltantes), **When** a importação é executada, **Then** a operação é rejeitada antes de processar qualquer linha, com mensagem descritiva.

---

### Edge Cases

- O que acontece quando o CSV enviado está vazio (zero linhas de dados)?
- O que acontece ao tentar vincular um aluno a uma turmaId inexistente?
- O que acontece se `CreatedAt`/`UpdatedAt` forem manipulados externamente?
- Como o sistema se comporta em concorrência (dois usuários criando o mesmo CPF simultaneamente)?
- O que acontece ao tentar reativar um aluno Evadido?

---

## Requirements *(mandatory)*

### Functional Requirements

**Aluno — Gestão**

- **FR-001**: O sistema DEVE permitir que usuários com role Diretor ou Coordenador criem alunos com os campos: nome completo, documento identificador (CPF ou ID interno), data de nascimento, turmaId e data de matrícula.
- **FR-002**: O sistema DEVE rejeitar a criação de aluno com documento identificador duplicado (CPF ou ID interno já existente no sistema).
- **FR-003**: O sistema DEVE vincular obrigatoriamente todo aluno a uma turma ativa no momento da criação.
- **FR-004**: O sistema DEVE atribuir automaticamente o status Ativo ao aluno no momento da criação.
- **FR-005**: O sistema DEVE registrar `CreatedAt` e `UpdatedAt` automaticamente.
- **FR-006**: O sistema DEVE permitir que usuários com role Diretor ou Coordenador atualizem dados cadastrais do aluno (nome, documento, data de nascimento).
- **FR-007**: O sistema DEVE atualizar `UpdatedAt` e emitir `StudentUpdatedEvent` a cada modificação de dados.
- **FR-008**: O sistema DEVE permitir que usuários com role Diretor ou Coordenador inativem um aluno, definindo status como Inativo ou Evadido e preenchendo `DeletedAt`.
- **FR-009**: O sistema NÃO DEVE permitir exclusão física de alunos — apenas soft delete.
- **FR-010**: O sistema DEVE permitir que usuários com role Diretor ou Coordenador transfiram um aluno de turma, validando que a turma destino está ativa.
- **FR-011**: O sistema DEVE emitir o evento de domínio correspondente a cada operação de ciclo de vida: `StudentCreatedEvent`, `StudentUpdatedEvent`, `StudentTransferredEvent`, `StudentDeactivatedEvent`.

**Aluno — Consulta**

- **FR-012**: O sistema DEVE fornecer uma listagem paginada de alunos com filtros por turmaId e status.
- **FR-013**: A listagem padrão DEVE excluir alunos com soft delete (DeletedAt preenchido), salvo quando o filtro explicitamente incluir status Inativo/Evadido.
- **FR-014**: O sistema DEVE fornecer endpoint de detalhe do aluno por Id.
- **FR-015**: Usuários com role Professor DEVEM poder consultar alunos, mas sem acesso a operações de escrita.

**Turma**

- **FR-016**: O sistema DEVE gerenciar turmas com os campos: Id, nome, série/ano, ano letivo e status (Ativa/Inativa).
- **FR-017**: O sistema DEVE suportar vínculo many-to-many entre turmas e professores.
- **FR-018**: O sistema DEVE impedir a vinculação de alunos a turmas inativas.

**Importação CSV**

- **FR-019**: O sistema DEVE suportar importação de alunos via CSV como caso de uso separado, acessível apenas por Diretor ou Coordenador.
- **FR-020**: A importação DEVE validar duplicidade de documento identificador antes da persistência.
- **FR-021**: A importação DEVE suportar dois modos de operação: fail-fast (aborta ao primeiro erro) e partial success (persiste os válidos, reporta os inválidos).
- **FR-022**: A importação DEVE retornar relatório de erros com número de linha e descrição do problema para cada linha inválida.

**Segurança e LGPD**

- **FR-023**: O sistema NÃO DEVE expor dados pessoais de alunos (nome completo, CPF, data de nascimento) em contextos desnecessários ou para roles não autorizados.
- **FR-024**: Dados pessoais do aluno DEVEM ser sanitizados antes de qualquer envio para serviços de IA.
- **FR-025**: Operações de criação, transferência e inativação DEVEM ser registradas em log de auditoria.

---

### Key Entities

- **Aluno**: Entidade pedagógica central. Não é usuário do sistema, não possui credenciais. Possui status (Ativo, Inativo, Evadido), vínculo com uma única turma ativa por vez, e documento identificador único (CPF ou ID interno). Suporta soft delete.

- **Turma**: Agrupamento pedagógico de alunos. Possui nome, série/ano, ano letivo e status (Ativa/Inativa). Vinculada a múltiplos professores (many-to-many). Uma turma inativa não pode receber novos alunos ou transferências.

- **Professor**: Entidade já existente no sistema. Pode estar vinculado a múltiplas turmas.

- **StudentCreatedEvent / StudentUpdatedEvent / StudentTransferredEvent / StudentDeactivatedEvent**: Eventos de domínio emitidos a cada mudança de ciclo de vida do aluno, consumíveis pelos módulos de relatórios, IA e alertas.

---

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Diretores e Coordenadores conseguem cadastrar, editar, transferir e inativar alunos em menos de 2 minutos por operação, sem precisar de suporte técnico.
- **SC-002**: O sistema garante 100% de unicidade de documento identificador — nenhum aluno duplicado por CPF ou ID interno existe em produção.
- **SC-003**: Listagens de alunos com filtros (turma + status) retornam resultados corretos e paginados, sem vazar dados de alunos de outras turmas.
- **SC-004**: Importação CSV de 500 alunos válidos é concluída com sucesso, com relatório de erros gerado para linhas inválidas, em tempo aceitável para operação diária.
- **SC-005**: 100% das operações de ciclo de vida (criação, atualização, transferência, inativação) emitem o evento de domínio correspondente, garantindo rastreabilidade completa.
- **SC-006**: Dados pessoais de alunos (CPF, nome completo, data de nascimento) nunca são transmitidos para a IA sem sanitização prévia.
- **SC-007**: Professores sem role de Diretor/Coordenador não conseguem executar operações de escrita sobre alunos, com rejeição imediata (sem acesso a dados que não lhes pertencem).

---

## Assumptions

- O sistema de autenticação JWT com Roles já está operacional — os roles Diretor, Coordenador e Professor já existem.
- A entidade `Professor` já existe no domínio com seu `Id`, podendo ser referenciada pelo vínculo de turma.
- CPF é o documento primário de identificação; ID interno é um fallback para alunos estrangeiros ou sem CPF.
- O campo documento identificador aceita formatação flexível (com ou sem pontuação), mas é validado e normalizado na persistência.
- Não há autenticação de responsáveis/pais neste módulo — comunicação com responsáveis é responsabilidade de outro módulo.
- A geração de relatórios pedagógicos e análises de IA são responsabilidade dos respectivos módulos — este módulo fornece apenas a estrutura de dados do aluno como base.
- Reativar um aluno Evadido ou Inativo está fora do escopo desta spec (pode ser abordado em iteração futura).
- A importação CSV opera de forma síncrona para volumes razoáveis (até ~500 linhas); processamento assíncrono em background é considerado evolução futura.
- O histórico detalhado de transferências (data, turma anterior, turma destino) é rastreado via eventos de domínio e logs de auditoria, não via tabela de histórico dedicada nesta versão.
