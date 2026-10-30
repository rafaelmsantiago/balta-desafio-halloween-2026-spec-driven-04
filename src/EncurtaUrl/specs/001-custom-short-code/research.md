# Research: Código Customizado para URL Curta

Não restou nenhum NEEDS CLARIFICATION no Technical Context: a stack é a existente e as
diretrizes do usuário definem o desenho. As decisões abaixo cobrem os pontos de implementação
com mais de uma opção razoável.

## R1. Como adicionar o campo opcional ao request

- **Decision**: `public record CreateShortUrlRequest(string Url, string? Code = null);`
- **Rationale**: o System.Text.Json desserializa records pelo construtor; um parâmetro ausente no
  JSON recebe o valor padrão (`null`). Requests atuais (`{ "url": "..." }`) continuam válidos
  (Princípio V). Nome `code` coincide com o campo `code` da resposta.
- **Alternatives considered**: `CustomCode` (mais explícito, mas destoa do nome do campo na
  resposta); um record de request novo (violaria a diretriz de não criar endpoint/contrato novo).

## R2. Onde e como validar o código customizado

- **Decision**: classe `CustomCodeValidator` com interface `ICustomCodeValidator` em
  `Services/CustomCodeValidator.cs`, registrada como singleton. Método
  `string[] Validate(string code)` devolve as mensagens de erro (vazio = válido).
  - Tamanho: `code.Length` entre 3 e 32.
  - Formato: laço sobre os caracteres aceitando `char.IsAsciiLetterOrDigit(c) || c is '-' or '_'`.
  - Reservados: `HashSet<string>` com `StringComparer.OrdinalIgnoreCase` contendo `urls` e
    `openapi`.
- **Rationale**: segue o padrão de `ShortCodeGenerator` (interface + implementação no mesmo
  arquivo, singleton) e a diretriz de concentrar as regras numa classe pequena sem pacote externo.
  O laço é mais simples que regex e evita a armadilha de `$` em regex .NET, que aceita um `\n`
  final (`"balta-io\n"` passaria em `^[A-Za-z0-9_-]{3,32}$`).
- **Alternatives considered**: `[GeneratedRegex]` com `\z` (funciona, mas exige classe `partial`
  e é menos legível para 3 regras); DataAnnotations/FluentValidation (pacote ou infraestrutura
  extra, viola Princípios I e VI).

## R3. Código vazio vs. ausente

- **Decision**: `string.IsNullOrEmpty(request.Code)` → fluxo atual de geração. Qualquer outro
  valor (inclusive só espaços ou com espaços nas pontas) passa pelo validador e é recusado se
  inválido. Sem `Trim`.
- **Rationale**: cumpre os Edge Cases da spec.

## R4. Unicidade e conflito (409)

- **Decision**: duas camadas.
  1. Antes de gravar, `AnyAsync(x => x.Code == code)`; se existir, retorna
     `TypedResults.Problem(title: ..., detail: ..., statusCode: 409)`.
  2. Corrida: `SaveChangesAsync` dentro de `try/catch (DbUpdateException ex) when
     (IsUniqueViolation(ex))`, onde `IsUniqueViolation` verifica
     `ex.InnerException is SqliteException { SqliteErrorCode: 19, SqliteExtendedErrorCode: 2067 }`
     (SQLITE_CONSTRAINT / SQLITE_CONSTRAINT_UNIQUE). Retorna o mesmo 409.
- **Rationale**: a pré-checagem dá a resposta comum sem depender de exceção; o catch garante a
  regra sob concorrência, usando o índice único que já existe. `SqliteException` vem de
  `Microsoft.Data.Sqlite`, dependência transitiva de `Microsoft.EntityFrameworkCore.Sqlite`, então
  não há pacote novo.
- **Escopo do catch**: aplicado somente quando há código customizado. No fluxo gerado, o
  comportamento atual é preservado (colisão na gravação continua caindo no `UseExceptionHandler`).
- **Alternatives considered**: só o catch (funciona, mas transforma o caso comum em exceção);
  transação serializável (desnecessária com índice único).

## R5. Tipo de retorno do handler

- **Decision**: manter `Results<Created<ShortUrlResponse>, ValidationProblem, ProblemHttpResult>`;
  o 409 usa `TypedResults.Problem(...)` (já `ProblemHttpResult`). Adicionar
  `.ProducesProblem(StatusCodes.Status409Conflict)` no `MapPost` para documentar no OpenAPI.
- **Rationale**: Princípio III (ProblemDetails com status correto) sem alterar a assinatura.
- **Alternatives considered**: `Conflict<ProblemDetails>` no union (mais um tipo sem ganho real).

## R6. Diferença entre maiúsculas e minúsculas

- **Decision**: nenhuma mudança. O índice e a comparação `x.Code == code` no SQLite usam
  collation `BINARY`, que diferencia caixa, como já ocorre com códigos gerados.
- **Rationale**: atende à Assumption da spec; reservados são checados à parte, sem caixa.

## R7. Conflito com rotas

- **Decision**: reservados `urls` e `openapi`. `GET /urls` é rota literal e tem precedência sobre
  `/{code}`, então um código `urls` ficaria inacessível; `openapi` é reservado preventivamente
  (prefixo de `/openapi/v1.json`).
- **Rationale**: FR-006 e Restrições Técnicas da constituição.
