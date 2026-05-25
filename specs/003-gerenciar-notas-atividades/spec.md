# Feature Specification: Gerenciamento de Notas por Atividade

**Feature Branch**: `003-manage-student-grades`

**Created**: 2026-05-25

**Status**: Draft

**Input**: User description: "Eu como sistema preciso gerenciar as notas dos alunos em cada atividade. preciso de uma subsessão em atividades em cada atividade para poder inserir as notas de cada aluno na turma para a atividade."

## Clarifications

### Session 2026-05-25

- Q: Qual regra de escala de nota deve ser adotada no lançamento? -> A: A nota deve ser recebida como string, conforme convenção da escola (numérica ou literal, ex.: MB, A, R).
- Q: Como tratar alunos transferidos/inativados e novos alunos na atividade após lançamento inicial? -> A: Manter notas já lançadas para alunos que saíram e incluir novos alunos sem nota obrigatória automática.
- Q: Quais perfis podem lançar e editar notas da atividade? -> A: Professor da turma e coordenador podem lançar e editar notas.
- Q: Qual estratégia de concorrência deve ser adotada em edições simultâneas de nota? -> A: Detectar conflito por versão e rejeitar gravação desatualizada.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Lançar notas da turma na atividade (Priority: P1)

Um professor precisa abrir uma atividade específica e registrar a nota de cada aluno da turma vinculada para concluir a avaliação daquela atividade em um único fluxo.

**Why this priority**: Sem o lançamento de notas por atividade, não existe registro de desempenho individual dos alunos e o processo avaliativo fica incompleto.

**Independent Test**: Pode ser testado criando uma atividade com turma vinculada, preenchendo as notas de todos os alunos e verificando que o boletim da atividade exibe os valores salvos para cada aluno.

**Acceptance Scenarios**:

1. **Given** uma atividade existente vinculada a uma turma com alunos ativos, **When** o professor acessa a subseção de notas e informa as notas dos alunos, **Then** o sistema salva os lançamentos e confirma o sucesso da operação.
2. **Given** uma atividade existente, **When** o professor tenta salvar sem informar nota para ao menos um aluno obrigatório, **Then** o sistema bloqueia o envio e indica quais alunos estão pendentes.
3. **Given** um usuário sem permissão de edição de atividade, **When** ele tenta lançar notas, **Then** o sistema rejeita a operação por autorização.

---

### User Story 2 - Consultar notas já lançadas (Priority: P2)

Um professor ou coordenador precisa visualizar rapidamente as notas já registradas de uma atividade para acompanhar desempenho da turma e identificar pendências.

**Why this priority**: A consulta garante transparência pedagógica e evita retrabalho de lançamento duplicado.

**Independent Test**: Pode ser testado após um lançamento inicial, reabrindo a subseção de notas e confirmando que todas as notas previamente salvas são exibidas corretamente por aluno.

**Acceptance Scenarios**:

1. **Given** uma atividade com notas previamente lançadas, **When** o usuário abre a subseção de notas, **Then** o sistema apresenta a lista de alunos da turma com suas respectivas notas atuais.
2. **Given** uma atividade sem notas lançadas, **When** o usuário abre a subseção de notas, **Then** o sistema exibe a turma com estado de notas em branco e orientação para preenchimento.

---

### User Story 3 - Atualizar notas antes do fechamento (Priority: P3)

Um professor precisa corrigir notas lançadas incorretamente enquanto a atividade ainda estiver em período de edição, mantendo histórico confiável da avaliação.

**Why this priority**: Correções controladas evitam inconsistências pedagógicas e reduzem solicitações administrativas manuais.

**Independent Test**: Pode ser testado alterando uma nota previamente salva em atividade editável e confirmando que o novo valor substitui o anterior na consulta subsequente.

**Acceptance Scenarios**:

1. **Given** uma atividade em período de edição com notas já salvas, **When** o professor altera uma ou mais notas e salva novamente, **Then** o sistema atualiza os valores informados com confirmação de sucesso.
2. **Given** uma atividade bloqueada para edição, **When** o professor tenta alterar notas, **Then** o sistema impede a alteração e informa que a atividade está fechada para mudanças.

---

### Edge Cases

- O que acontece quando a atividade não possui alunos vinculados na turma no momento do lançamento?
- Em edição simultânea da mesma nota, o sistema detecta conflito por versão e rejeita gravação desatualizada, exigindo nova leitura antes de salvar.
- O que acontece quando é informado um valor de nota fora da escala permitida pela escola?
- Alunos transferidos ou inativados após lançamento inicial mantêm histórico das notas já registradas; novos alunos da turma passam a aparecer sem nota obrigatória automática.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: O sistema DEVE disponibilizar, dentro de cada atividade, uma subseção específica para gerenciamento de notas da turma vinculada.
- **FR-002**: O sistema DEVE listar, na subseção de notas, todos os alunos ativos da turma da atividade com identificação suficiente para lançamento individual.
- **FR-003**: O sistema DEVE permitir que usuário autorizado informe e salve nota por aluno para a atividade selecionada.
- **FR-004**: O sistema DEVE receber nota como string e validar se o valor informado pertence à convenção de avaliação configurada para a atividade (numérica ou literal, como MB, A, R).
- **FR-005**: O sistema DEVE impedir salvamento quando houver aluno obrigatório sem nota, retornando mensagem clara de pendência.
- **FR-006**: O sistema DEVE persistir as notas lançadas de forma vinculada simultaneamente à atividade, turma e aluno.
- **FR-007**: O sistema DEVE permitir atualização de notas já lançadas enquanto a atividade estiver em estado editável.
- **FR-008**: O sistema NÃO DEVE permitir criação ou atualização de notas quando a atividade estiver bloqueada para edição.
- **FR-009**: O sistema DEVE exibir as notas já salvas ao reabrir a subseção da atividade.
- **FR-010**: O sistema DEVE permitir lançamento e alteração de notas apenas para o professor da turma vinculada à atividade e para coordenadores.
- **FR-011**: O sistema DEVE registrar auditoria das ações de criação e alteração de notas com identificação do usuário e data/hora.
- **FR-012**: O sistema DEVE tratar concorrência de edição para evitar perda silenciosa de alterações em lançamentos simultâneos.
- **FR-013**: O sistema DEVE rejeitar valores de nota em string que não pertençam ao conjunto permitido pela convenção da escola para aquela atividade.
- **FR-014**: O sistema DEVE preservar notas já lançadas de alunos que saíram da turma após o lançamento inicial, para manter histórico avaliativo.
- **FR-015**: O sistema DEVE incluir novos alunos da turma na subseção de notas com estado inicial sem nota, sem obrigatoriedade automática de preenchimento retroativo.
- **FR-016**: O sistema DEVE adotar concorrência otimista por versão no lançamento de notas e rejeitar gravações baseadas em versão desatualizada.

### Key Entities *(include if feature involves data)*

- **Atividade**: Unidade avaliativa associada a uma turma, que define contexto e regras para lançamento de notas.
- **Aluno da Turma**: Estudante ativo elegível para receber nota na atividade.
- **Lançamento de Nota**: Registro da nota de um aluno em uma atividade específica, incluindo valor da nota em formato string, responsável pelo lançamento e momento da atualização.
- **Escala de Avaliação**: Regra que define o conjunto válido de valores de nota para a atividade, aceitando convenções numéricas e/ou literais conforme escola.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Professores autorizados conseguem lançar notas de uma turma completa em uma atividade em até 5 minutos para turmas de até 40 alunos.
- **SC-002**: 100% das tentativas de salvar notas fora da convenção permitida da atividade (numérica ou literal) são bloqueadas com feedback claro ao usuário.
- **SC-003**: Em testes de aceitação, pelo menos 95% dos usuários concluem o lançamento sem necessidade de suporte externo na primeira tentativa.
- **SC-004**: 100% das notas salvas permanecem corretamente associadas ao aluno e à atividade após reabertura da subseção.
- **SC-005**: 100% das alterações de nota geram registro de auditoria consultável para rastreabilidade.
- **SC-006**: 100% das notas já lançadas permanecem acessíveis para consulta histórica mesmo após mudança de vínculo do aluno com a turma.
- **SC-007**: 100% das tentativas de lançamento/edição por usuários fora dos perfis permitidos são rejeitadas por autorização.
- **SC-008**: 100% dos conflitos de edição simultânea são detectados e retornados ao usuário sem sobrescrita silenciosa de nota.

## Assumptions

- A atividade já possui vínculo obrigatório com uma turma antes do lançamento de notas.
- O conceito de aluno ativo da turma já está disponível e reutilizável no sistema.
- A escala de avaliação da atividade já está definida previamente pela escola.
- O fluxo contempla somente lançamento e atualização de notas, sem cálculo automático de médias nesta versão.
- O gerenciamento é realizado por usuários internos (professor/coordenador), sem acesso direto por responsáveis ou alunos.
