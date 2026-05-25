<!-- SPECKIT START -->
For additional context about technologies to be used, project structure,
shell commands, and other important information, read the current plan at:
specs/003-gerenciar-notas-atividades/plan.md
<!-- SPECKIT END -->

---

# SIAED Backend — Instruções do Copilot

Este arquivo contém orientações de execução para o GitHub Copilot.  
A fonte oficial e definitiva de regras arquiteturais e técnicas é o `AGENTS.md`.

Sempre considere o `AGENTS.md` como fonte de verdade.

---

## Contexto do Projeto

O SIAED é uma plataforma de backend para inteligência educacional em escolas particulares.

Ele atua como uma camada de IA sobre ERPs existentes, com foco em:

- Redução de evasão escolar
- Automação pedagógica
- Comunicação entre escola e família
- Apoio educacional com IA

---

## Regra central

O Copilot deve sempre assumir:

> Se uma regra existir no `AGENTS.md`, ela tem prioridade absoluta sobre qualquer instrução aqui.

---

## Regras arquiteturais mínimas (execução)

- O sistema segue Clean Architecture + DDD + CQRS
- Todo fluxo de aplicação deve passar por MediatR
- Não pode existir regra de negócio na camada API
- Não pode existir regra de negócio na camada Infra
- O Domain não pode depender de frameworks externos
- A Application depende apenas de abstrações

---

## Regras de geração de código

Ao gerar ou alterar código:

- Prefira implementações explícitas em vez de atalhos
- Nunca remova invariantes do domínio
- Nunca crie modelos anêmicos de domínio
- Separe sempre Command e Query (CQRS)
- Prefira interfaces na camada Application
- Assuma sempre MySQL + Entity Framework Core

---

## Regra do Result Pattern

- Toda resposta da Application deve usar `Result` ou `Result<T>`
- Não devem existir exceções não tratadas chegando até a API
- Erros devem ser tratados na Application ou via middleware

---

## Regras de uso do MediatR

- Commands → `IRequest<Result>`
- Queries → `IRequest<Result<T>>`
- Handlers devem ficar em `Features/<Feature>/`
- A API deve apenas chamar `IMediator`

---

## Integração com OpenAI / IA

- Toda integração com IA deve passar por abstrações na Application
- Não pode haver uso direto do SDK da OpenAI na API ou Domain
- Todas as requisições de IA devem ser validadas antes do envio
- Todas as respostas da IA devem ser logadas (tokens, custo e metadados)
- A IA deve sempre responder em português brasileiro
- A IA NÃO pode:
  - Emitir diagnósticos médicos
  - Inventar fatos inexistentes
  - Substituir decisões pedagógicas humanas

---

## Regras de banco de dados

- Usar apenas EF Core + MySQL
- Nunca usar SQL direto na Application ou Domain
- Soft delete obrigatório com `DeletedAt`
- Nunca realizar delete físico em entidades de domínio
- Usar controle de concorrência otimista
- Paginação obrigatória em consultas de listagem

---

## Regras de segurança

- Seguir LGPD rigorosamente
- Nunca expor dados sensíveis de alunos sem necessidade
- Sanitizar dados antes de enviar para IA
- Autenticação via JWT Bearer com Roles e Policies
- Operações sensíveis devem ter auditoria

---

## Estilo de código

- Prefira clareza em vez de código curto
- Prefira lógica explícita de domínio
- Mantenha nomenclatura alinhada ao domínio (DDD)
- Evite overengineering em CRUD simples, mas sem quebrar arquitetura

---

## Regras de relatórios

- O endpoint `/reports/generate` não deve depender nem exigir `performanceNotes`; o relatório deve ter viés comportamental sem relação com atividade por enquanto.

---

## Restrições absolutas

- NUNCA colocar regra de negócio em Controllers
- NUNCA burlar regras de domínio na Infra
- NUNCA expor entidades EF diretamente na API
- NUNCA quebrar regras de dependência entre camadas
- NUNCA retornar exceções não tratadas para o cliente

---

## Relação com o AGENTS.md

- O `AGENTS.md` define todas as regras arquiteturais do sistema
- Este arquivo define apenas comportamento de execução do Copilot
- Em caso de conflito, o `AGENTS.md` sempre prevalece