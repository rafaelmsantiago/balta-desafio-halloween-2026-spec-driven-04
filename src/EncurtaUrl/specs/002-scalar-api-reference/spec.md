# Feature Specification: Interface de Testes da API (Scalar)

**Feature Branch**: `002-scalar-api-reference`

**Created**: 2026-10-30

**Status**: Draft

**Input**: User description: "Adicione o Scalar para poder ter uma interface de testes." (pedido feito
durante a feature 001; esta spec documenta a mudança, já implementada no working tree, para que ela
passe pelo fluxo da constituição.)

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Explorar e testar a API pelo navegador (Priority: P1)

Quem desenvolve ou avalia a API abre uma página no navegador que lista todos os endpoints, mostra
os formatos de request e response e permite enviar requisições e ver as respostas, sem precisar de
ferramentas externas nem do arquivo `.http`.

**Why this priority**: é o pedido em si; agiliza testes manuais e demonstrações.

**Independent Test**: com a API rodando em ambiente de desenvolvimento, abrir `/scalar`, localizar
`POST /urls`, enviar uma criação com `code` e ver a resposta 201.

**Acceptance Scenarios**:

1. **Given** a API rodando em ambiente de desenvolvimento, **When** alguém acessa `/scalar`,
   **Then** vê uma página de referência com todos os endpoints existentes.
2. **Given** a página aberta, **When** a pessoa envia `POST /urls` com `url` e `code` válidos,
   **Then** vê a resposta 201 com o corpo retornado pela API.
3. **Given** a página aberta, **When** a pessoa consulta `POST /urls`, **Then** vê documentados os
   resultados 201, 400 e 409.

---

### User Story 2 - Interface não exposta fora de desenvolvimento (Priority: P1)

A página de testes só existe em ambiente de desenvolvimento, assim como o documento OpenAPI.

**Why this priority**: evita expor a superfície da API e uma ferramenta de envio de requisições em
produção.

**Independent Test**: rodar a API fora do ambiente de desenvolvimento e acessar `/scalar`; a página
não deve existir.

**Acceptance Scenarios**:

1. **Given** a API rodando fora do ambiente de desenvolvimento, **When** alguém acessa `/scalar`,
   **Then** não recebe a página de testes.

---

### User Story 3 - `scalar` não pode ser usado como código curto (Priority: P2)

Como `/scalar` passa a ser uma rota da API, ele entra na lista de códigos reservados da feature 001.

**Why this priority**: sem isso, uma URL curta com código `scalar` seria criada mas nunca
redirecionaria em desenvolvimento.

**Independent Test**: tentar criar uma URL curta com `"code": "Scalar"` e receber recusa de código
reservado.

**Acceptance Scenarios**:

1. **Given** qualquer ambiente, **When** alguém tenta criar uma URL curta com código `scalar` em
   qualquer combinação de maiúsculas e minúsculas, **Then** a criação é recusada como erro de
   validação do campo de código, com a mensagem de código reservado em português.

---

### Edge Cases

- `/scalar` e `/scalar/` levam à mesma página.
- Um código `scalar` já gravado antes desta feature (improvável, pois a 001 ainda não foi
  publicada) continuaria no banco, mas inacessível em desenvolvimento; não há migração.
- O código é reservado em todos os ambientes, mesmo onde a página não existe, para que o
  comportamento de criação não dependa do ambiente.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Em ambiente de desenvolvimento, a API MUST oferecer em `/scalar` uma página de
  referência interativa gerada a partir do documento OpenAPI já existente.
- **FR-002**: A página MUST permitir enviar requisições aos endpoints existentes e exibir as
  respostas.
- **FR-003**: Fora do ambiente de desenvolvimento, `/scalar` MUST NOT expor a página.
- **FR-004**: `scalar` MUST ser adicionado aos códigos reservados, com a mesma regra de comparação
  sem diferenciar maiúsculas e minúsculas.
- **FR-005**: Nenhum endpoint, contrato ou mensagem existente MUST mudar.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Em desenvolvimento, 100% dos endpoints da API aparecem na página de testes.
- **SC-002**: Uma pessoa consegue criar e consultar uma URL curta pela página em menos de 1 minuto,
  sem outra ferramenta.
- **SC-003**: Fora de desenvolvimento, a página não fica acessível em 100% das tentativas.
- **SC-004**: 100% das tentativas de criar URL curta com código `scalar` são recusadas.

## Assumptions

- Ferramenta: Scalar, pedida explicitamente. Exige um pacote NuGet novo, que precisa de
  justificativa no plano (Princípio VI da constituição).
- A página reutiliza o documento OpenAPI que a API já gera em desenvolvimento; não há documentação
  escrita à mão.
- Sem autenticação na página, assim como na API.
- O arquivo `.http` continua sendo a validação oficial da constituição; a página é complementar.
