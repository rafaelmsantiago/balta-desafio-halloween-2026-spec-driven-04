# Data Model: Código Customizado para URL Curta

## Schema

**Sem alteração.** A entidade `ShortUrl` ([Models/ShortUrl.cs](../../src/EncurtaUrl.Api/Models/ShortUrl.cs))
e o mapeamento em [Data/AppDbContext.cs](../../src/EncurtaUrl.Api/Data/AppDbContext.cs) já atendem:

| Campo | Tipo | Restrições existentes | Uso nesta feature |
| --- | --- | --- | --- |
| `Id` | int | PK | — |
| `Code` | string | obrigatório, máx. 32, índice único | recebe o código customizado ou o gerado |
| `OriginalUrl` | string | obrigatório, máx. 2048 | — |
| `CreatedAt` | DateTime | padrão `DateTime.UtcNow` | — |
| `Clicks` | int | — | incrementado no redirecionamento, igual para os dois tipos de código |

Como não há mudança de schema, bancos `encurtaurl.db` existentes continuam válidos (Princípio IV).

## Regras de validação do código customizado

Aplicadas somente quando o campo `code` do request é informado e não vazio.

| Regra | Condição de recusa | Resultado |
| --- | --- | --- |
| Tamanho | `Length < 3` ou `Length > 32` | 400, chave `code` |
| Formato | algum caractere fora de `A–Z`, `a–z`, `0–9`, `-`, `_` | 400, chave `code` |
| Reservado | igual a `urls` ou `openapi`, ignorando caixa | 400, chave `code` |
| Unicidade | já existe `ShortUrl` com o mesmo `Code` (comparação com caixa) | 409 |

Regras de tamanho, formato e reservado podem ser reportadas juntas, e junto com o erro de `url`.
Unicidade só é verificada depois que todas as validações passam.

## Entidades novas

Nenhuma. A lista de códigos reservados é uma constante dentro de `CustomCodeValidator`, não um
dado persistido.
