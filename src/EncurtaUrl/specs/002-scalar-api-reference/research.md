# Research: Interface de Testes da API (Scalar)

Nenhum NEEDS CLARIFICATION restou no Technical Context. As decisões abaixo cobrem a justificativa
de dependência exigida pelo Princípio VI e os pontos de integração.

## R1. Pacote `Scalar.AspNetCore` (justificativa do Princípio VI)

- **Decision**: adicionar `Scalar.AspNetCore` versão `2.18.7`, fixa.
- **Problema resolvido**: o projeto gera o documento OpenAPI (`/openapi/v1.json`), mas não tem
  interface para navegar e enviar requisições; o .NET 10 não traz uma (o template deixou de incluir
  o Swagger UI desde o .NET 9).
- **Rationale**: pedido explícito do usuário; uma chamada (`MapScalarApiReference()`) consome o
  documento OpenAPI que já existe, sem configuração extra e sem dependências transitivas
  relevantes. Mantido pelo projeto Scalar e referenciado na documentação oficial do ASP.NET Core
  como opção para o .NET 9+.
- **Versão fixa**: diferente dos pacotes Microsoft (`10.0.*`), que acompanham o SDK, o Scalar
  publica versões com frequência e altera a interface entre elas; fixar evita mudanças silenciosas
  no restore. Atualizações passam a ser explícitas.
- **Alternatives considered**:
  - *Página HTML estática carregando o Scalar por CDN* (sem pacote): rejeitada, exige manter HTML e
    rota à mão e depende de rede externa na hora de abrir a página.
  - *Swashbuckle / Swagger UI*: também é pacote novo, e o pedido foi Scalar.
  - *NSwag*: pacote maior, inclui geração de cliente que não é necessária.
  - *Só o `.http`*: continua sendo a validação oficial, mas não atende ao pedido de interface.

## R2. Restrição a Development

- **Decision**: chamar `app.MapScalarApiReference()` dentro do bloco
  `if (app.Environment.IsDevelopment())`, logo após `app.MapOpenApi()`.
- **Rationale**: a página depende do documento OpenAPI, que já só existe em Development (FR-003).
- **Comportamento fora de Development**: `GET /scalar` cai na rota `/{code}`; como `scalar` é
  reservado, nenhum código com esse valor existe e a resposta é 404 (ProblemDetails via
  `UseStatusCodePages`).

## R3. Rota `/scalar` e códigos reservados

- **Decision**: adicionar `"scalar"` a `ReservedCodes` em `Services/CustomCodeValidator.cs`, em todos
  os ambientes.
- **Rationale**: a rota literal `/scalar` tem precedência sobre `/{code}`; um código `scalar` ficaria
  inacessível em Development. Reservar em todos os ambientes mantém a criação previsível (FR-004)
  e cumpre a regra da constituição de não sombrear `/{code}`.
- **Alternatives considered**: mudar o prefixo do Scalar para algo sob `/openapi` (ex.:
  `/openapi/scalar`), evitando o novo reservado; rejeitado porque `/scalar` é a rota padrão
  documentada e já validada, e reservar um nome custa uma linha.

## R4. Autenticação e configuração

- **Decision**: sem opções extras (tema, título, autenticação). Padrões do pacote.
- **Rationale**: Princípio I; a API não tem autenticação.
