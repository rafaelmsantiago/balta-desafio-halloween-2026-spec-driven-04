# Feature Specification: Código Customizado para URL Curta

**Feature Branch**: `001-custom-short-code`

**Created**: 2026-10-30

**Status**: Draft

**Input**: User description: "Permitir que o usuário informe um código customizado ao encurtar uma URL, para depois navegar por ele (ex.: /balta-io em vez de /aZ3k9Qx). Hoje o POST /urls recebe apenas a URL e o código é sempre gerado automaticamente; esta feature estende esse fluxo. O código customizado é opcional, único, com regras de formato (letras, números, hífen e underscore, com tamanho mínimo e máximo) e não pode conflitar com rotas da própria API. Listagem e detalhes continuam iguais. Fora do escopo: editar ou remover o código, autenticação, expiração de links."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Encurtar com código customizado (Priority: P1)

Quem encurta uma URL informa, além da URL original, um código legível (ex.: `balta-io`). A URL curta
devolvida usa esse código, e quem acessar `/balta-io` é redirecionado para a URL original, com o
clique contabilizado como já acontece hoje.

**Why this priority**: é o valor central da feature. Links memoráveis são o motivo de ela existir.

**Independent Test**: criar uma URL curta informando o código `balta-io`, conferir que a resposta
traz esse código e a URL curta correspondente, acessar `/balta-io` e verificar o redirecionamento
e o incremento de cliques.

**Acceptance Scenarios**:

1. **Given** que nenhuma URL curta usa o código `balta-io`, **When** o usuário encurta
   `https://balta.io` informando o código `balta-io`, **Then** a criação é aceita e a resposta traz o
   código `balta-io` e a URL curta terminando em `/balta-io`.
2. **Given** uma URL curta criada com o código `balta-io`, **When** alguém acessa `/balta-io`,
   **Then** é redirecionado para `https://balta.io` e o total de cliques dessa URL aumenta em 1.
3. **Given** uma URL curta criada com o código `balta-io`, **When** o usuário consulta os detalhes
   por `balta-io` ou lista todas as URLs, **Then** ela aparece com os mesmos campos e formato de uma
   URL com código gerado.

---

### User Story 2 - Fluxo atual sem código continua igual (Priority: P1)

Quem encurta uma URL sem informar código continua recebendo um código gerado automaticamente,
exatamente como hoje.

**Why this priority**: compatibilidade. Clientes atuais não podem quebrar.

**Independent Test**: repetir os requests existentes de criação (apenas com a URL) e verificar que
a resposta e o comportamento são idênticos aos anteriores à feature.

**Acceptance Scenarios**:

1. **Given** um request de criação contendo apenas a URL, **When** ele é enviado, **Then** a URL
   curta é criada com um código gerado automaticamente de 7 caracteres, como hoje.
2. **Given** um request de criação com URL inválida e sem código, **When** ele é enviado, **Then** o
   erro de validação da URL é o mesmo de hoje.

---

### User Story 3 - Recusa de códigos inválidos, reservados ou em uso (Priority: P2)

Quem informa um código que não pode ser usado recebe uma recusa clara, em português, dizendo o
motivo: formato inválido, código reservado pelo sistema ou código já em uso.

**Why this priority**: sem essas regras a feature seria insegura (sobrescrita de links, conflito com
rotas da API), mas ela depende de a história 1 existir.

**Independent Test**: enviar criações com códigos fora do formato, com códigos reservados e com um
código já existente, e verificar que todas são recusadas com a mensagem adequada e que nada é
gravado.

**Acceptance Scenarios**:

1. **Given** que o código `balta-io` já está em uso, **When** o usuário tenta criar outra URL curta
   com o código `balta-io`, **Then** a criação é recusada como conflito, com mensagem informando que
   o código já está em uso, e a URL curta existente permanece inalterada.
2. **Given** um código com caracteres não permitidos (ex.: `balta io`, `balta.io`, `ação`),
   **When** o usuário tenta criar a URL curta, **Then** a criação é recusada como erro de validação
   do campo de código, com mensagem explicando o formato aceito.
3. **Given** um código menor que o mínimo (ex.: `ab`) ou maior que o máximo, **When** o usuário
   tenta criar a URL curta, **Then** a criação é recusada como erro de validação informando os
   limites de tamanho.
4. **Given** um código reservado (ex.: `urls`, `openapi`, em qualquer combinação de maiúsculas e
   minúsculas), **When** o usuário tenta criar a URL curta, **Then** a criação é recusada como erro
   de validação informando que o código é reservado.
5. **Given** um request com URL inválida e código inválido, **When** ele é enviado, **Then** a
   resposta de validação lista os dois problemas de uma vez.

---

### Edge Cases

- Código informado como vazio (`""`) ou ausente/nulo: tratado como não informado; o código é
  gerado automaticamente.
- Código com espaços no início ou no fim (ex.: `" balta-io "`): recusado como formato inválido; o
  sistema não remove espaços silenciosamente.
- Código composto só de hífens ou underscores (ex.: `---`): aceito, pois respeita o formato.
- Maiúsculas e minúsculas: `Balta-IO` e `balta-io` são códigos distintos, assim como os códigos
  gerados hoje já diferenciam maiúsculas de minúsculas.
- Duas criações simultâneas com o mesmo código: apenas uma é aceita; a outra é recusada como
  código em uso, nunca como erro interno.
- Código customizado igual a um código gerado já existente: recusado como código em uso.
- Geração automática que sorteia um código já usado por um código customizado: o sistema sorteia
  outro, como já faz hoje para colisões.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: A criação de URL curta MUST aceitar um código customizado opcional, além da URL
  original.
- **FR-002**: Quando o código customizado não for informado (ausente, nulo ou vazio), o sistema MUST
  gerar o código automaticamente, com comportamento idêntico ao atual.
- **FR-003**: Quando o código customizado for informado e válido, o sistema MUST criar a URL curta
  com exatamente esse código e devolvê-lo na resposta, junto com a URL curta correspondente.
- **FR-004**: O código customizado MUST conter apenas letras ASCII (A–Z, a–z), dígitos (0–9),
  hífen (`-`) e underscore (`_`).
- **FR-005**: O código customizado MUST ter no mínimo 3 e no máximo 32 caracteres.
- **FR-006**: O sistema MUST recusar códigos reservados, que conflitam com rotas da própria API:
  no mínimo `urls` e `openapi`. A comparação com a lista de reservados MUST ignorar maiúsculas e
  minúsculas.
- **FR-007**: Violações de formato, tamanho ou de código reservado MUST ser recusadas como erro de
  validação do campo de código, com mensagem em português, e podem ser reportadas junto com o
  erro de validação da URL.
- **FR-008**: O código (customizado ou gerado) MUST ser único entre todas as URLs curtas. Uma
  tentativa de criação com código já em uso MUST ser recusada como conflito, distinto de erro de
  validação, com mensagem em português informando que o código está em uso.
- **FR-009**: Uma criação recusada MUST NOT gravar nem alterar nenhuma URL curta.
- **FR-010**: A navegação por `/{codigo}` MUST redirecionar para a URL original e contar o clique
  para códigos customizados exatamente como para códigos gerados.
- **FR-011**: A listagem e a consulta de detalhes MUST tratar URLs com código customizado e gerado
  da mesma forma, sem campos novos obrigatórios e sem mudanças nos campos existentes.
- **FR-012**: Requests e respostas existentes MUST continuar válidos sem alteração.

### Key Entities

- **URL curta**: associação entre um código único e uma URL original, com data de criação e total
  de cliques. A feature não adiciona atributos novos; o código passa a poder ser escolhido por
  quem cria, em vez de sempre gerado.
- **Código reservado**: valor que não pode ser usado como código porque corresponde a uma rota da
  própria API.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% das criações com código customizado válido e livre resultam em uma URL curta que
  redireciona para a URL original pelo código escolhido.
- **SC-002**: 100% dos cenários de criação existentes (sem código) produzem o mesmo resultado que
  antes da feature.
- **SC-003**: 100% das tentativas com código inválido, reservado ou em uso são recusadas com
  mensagem em português que identifica o motivo, sem gravar dados.
- **SC-004**: Nenhuma rota existente da API fica inacessível por causa de um código customizado.
- **SC-005**: Quem cria um link com código customizado consegue fazê-lo em uma única requisição,
  sem passos adicionais em relação ao fluxo atual.

## Assumptions

- Limites de tamanho: mínimo de 3 caracteres (evita códigos triviais) e máximo de 32 (limite atual
  de armazenamento do código).
- Códigos diferenciam maiúsculas de minúsculas, mantendo o comportamento atual dos códigos gerados;
  apenas a verificação de reservados ignora a caixa, porque as rotas da API não diferenciam.
- A lista de reservados contém os primeiros segmentos das rotas existentes (`urls`, `openapi`) e
  deve crescer quando novas rotas de primeiro nível forem criadas.
- O nome do campo opcional no request é definido no plano, respeitando os contratos existentes.
- Não há autenticação: qualquer cliente pode escolher qualquer código livre (primeiro a chegar,
  primeiro a usar).
- Fora do escopo: editar ou remover o código depois de criado, autenticação e expiração de links.
