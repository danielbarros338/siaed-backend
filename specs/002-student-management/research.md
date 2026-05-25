# Research: Gerenciamento de Alunos

**Phase**: 0 | **Date**: 2026-05-18 | **Feature**: 002-student-management

---

## Decisões Técnicas

---

### 1. Entidade de Domínio: Student vs. User

**Decisão**: `Student` é uma entidade de domínio separada, sem vínculo com `User`.

**Rationale**: O aluno não possui credenciais de acesso ao sistema — é uma entidade pedagógica consultada por professores, coordenadores e diretores. Criar um `User` para ele violaria o princípio de menor privilégio e aumentaria a superfície de ataque sem valor funcional. A relação com responsáveis (pais/tutores) é responsabilidade de um módulo de comunicação futuro.

**Alternativas consideradas**:
- Vincular `Student` a um `User` com role especial → rejeitado (acoplamento desnecessário, viola LGPD ao criar credenciais sem uso)
- Usar `Teacher.UserId` como modelo → rejeitado (professor tem usuário por necessidade real de login; aluno não tem)

---

### 2. Entidade Turma (Turma)

**Decisão**: `Turma` é uma entidade de domínio nova, com relação many-to-many com `Teacher` via tabela de junção `TurmaTeachers`.

**Rationale**: Turmas são o agrupamento pedagógico natural. Professores podem estar em múltiplas turmas, e turmas têm múltiplos professores. EF Core 6+ suporta many-to-many implícito sem entidade intermediária explícita quando não há campos extras na tabela de junção.

**Alternativas consideradas**:
- Usar `SchoolId` como substituto de turma → rejeitado (não modela séries/anos letivos; já existia em `Teacher` como string livre, insuficiente)
- Turma com lista de strings (nomes de professores) → rejeitado (viola normalização e DDD)

---

### 3. Identificador Único do Aluno (CPF vs. ID Interno)

**Decisão**: Campo `DocumentoIdentificador` do tipo `string` normalizado (apenas dígitos), com índice único no banco. O tipo do documento é determinado por um enum `TipoDocumento` (CPF, IdInterno).

**Rationale**: Escolas recebem alunos estrangeiros e crianças sem CPF. Usar apenas CPF seria excludente. Normalizar para string única com tipo associado mantém a simplicidade do índice único e a flexibilidade documental.

**Alternativas consideradas**:
- Dois campos separados (`Cpf`, `IdInterno`) → rejeitado (complixa o índice de unicidade e o modelo)
- CPF obrigatório com nullable `IdInterno` → rejeitado (não cobre bem o caso de aluno sem CPF)

---

### 4. Eventos de Domínio

**Decisão**: Eventos de domínio são classes POCO em `Siaed.Domain/Events/`. Não implementam `INotification` do MediatR (para manter o Domain sem dependências externas). A publicação desses eventos via MediatR `IPublisher` é responsabilidade da camada Application — handlers coletam eventos da entidade e os publicam após o `SaveChangesAsync`.

**Rationale**: O Domain não pode depender do MediatR (regra da constituição). O padrão de "eventos coletados na entidade e publicados pelo handler/UoW" é bem estabelecido em DDD sem violar as regras de dependência.

**Alternativas consideradas**:
- Implementar `INotification` diretamente nas entidades → rejeitado (Domain dependeria de MediatR)
- Usar um event dispatcher customizado no Domain → rejeitado (overengineering para o estágio atual)
- Apenas logar sem publicar → rejeitado (quebra os casos de uso dos módulos de IA e alertas)

**Implementação prática**: Cada handler, ao concluir o `SaveChangesAsync`, publica o evento correspondente via `IPublisher` do MediatR. Na fase inicial, eventos podem ser apenas logados — a infraestrutura de consumo (módulo de alertas, IA reativa) será implementada em módulos futuros.

---

### 5. Importação CSV

**Decisão**: Usar a biblioteca **CsvHelper** (já disponível no ecossistema .NET, amplamente adotado) para parsing do CSV. Processamento síncrono para lotes de até 500 linhas. O command `ImportStudentsCsvCommand` recebe o conteúdo do arquivo como `Stream` ou `string` e retorna `Result<CsvImportResultDto>`.

**Rationale**: CsvHelper é o padrão de fato para CSV em .NET. Suporta mapeamento por atributos, validação por linha e é bem testado. Para o volume esperado (escola de médio porte), processamento síncrono é suficiente e mais simples de implementar e depurar.

**Alternativas consideradas**:
- CSV manual com `string.Split` → rejeitado (frágil para campos com vírgulas, aspas, encoding)
- Processamento assíncrono em background (Hangfire/queue) → rejeitado (overengineering para volume atual; pode ser adicionado como evolução)
- EPPlus / ClosedXML para Excel → rejeitado (spec define CSV explicitamente)

**Modos de operação**:
- `FailFast`: Para no primeiro erro, nenhum aluno é persistido, transação revertida.
- `PartialSuccess`: Persiste os válidos linha a linha, coleta erros das inválidas, retorna relatório.

---

### 6. Paginação e Filtros

**Decisão**: Usar `PagedResult<T>` já existente em `Siaed.Application.Common`. Query `GetStudentsPagedQuery` recebe `PageNumber`, `PageSize`, `TurmaId?`, `Status?`, `SearchTerm?`.

**Rationale**: `PagedResult<T>` já está implementado e testado. Reutilizá-lo mantém consistência com o restante da API.

**Alternativas consideradas**:
- Cursor-based pagination → rejeitado (desnecessário para o volume esperado; mais complexo de implementar e consumir)

---

### 7. Sanitização de Dados para IA

**Decisão**: O `AIContextManager` (já existente em Infra) será responsável por sanitizar dados do aluno. A sanitização remove/mascara CPF, data de nascimento completa (substituindo por faixa etária) e nome completo (substituindo por iniciais ou pseudônimo quando necessário).

**Rationale**: LGPD proíbe uso de dados pessoais desnecessários. A IA precisa de contexto pedagógico (turma, desempenho, status), não de dados de identificação pessoal.

**Alternativas consideradas**:
- Sanitização no handler antes de chamar IA → rejeitado (responsabilidade do AIContextManager, que tem visão completa do contexto)
- Não enviar dados do aluno à IA → rejeitado (quebra os casos de uso de relatórios e análises pedagógicas)

---

### 8. Concorrência na Criação (CPF Duplicado)

**Decisão**: Índice único na coluna `DocumentoIdentificador` no banco de dados (EF Core constraint). O handler verifica duplicidade via `IStudentRepository.ExistsByDocumentAsync()` antes de persistir. Em caso de race condition, o banco garante a unicidade via constraint — o handler captura a exceção de violação de constraint e retorna `Result.Failure`.

**Rationale**: Double-check (verificação no handler + constraint no banco) é o padrão mais seguro para unicidade em ambientes concorrentes.

**Alternativas consideradas**:
- Apenas constraint no banco sem verificação prévia → rejeitado (mensagem de erro do banco é técnica e não amigável)
- Pessimistic locking → rejeitado (impacto de performance desnecessário para o volume esperado)

---

## Dependências Externas

| Dependência | Versão | Projeto | Finalidade |
|-------------|--------|---------|------------|
| CsvHelper | 33.x | Siaed.Application ou Siaed.Infra | Parsing de CSV para importação |

> **Nota**: CsvHelper pode ser referenciado em `Siaed.Application` (para definir o contrato de parsing) ou `Siaed.Infra` (para a implementação). Recomendação: criar `ICsvParser<T>` em Application e implementar em Infra para manter a regra de dependência.

---

## Riscos Identificados

| Risco | Probabilidade | Impacto | Mitigação |
|-------|--------------|---------|-----------|
| Volume alto de CSV (> 1000 linhas) causa timeout | Baixa | Médio | Limitar a 500 linhas por request na validação; documentar limite |
| Race condition na criação de aluno com mesmo CPF | Muito Baixa | Alto | Constraint único no banco + verificação prévia no handler |
| Dados sensíveis de alunos enviados à IA sem sanitização | Muito Baixa | Alto | Sanitização obrigatória no AIContextManager; revisão de PR |
| Aluno transferido para turma inativa | Controlado | Médio | Validação no TransferStudentHandler antes de persistir |
