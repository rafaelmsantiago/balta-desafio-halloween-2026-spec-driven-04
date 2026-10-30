# Data Model: Interface de Testes da API (Scalar)

**Sem alteração de dados ou schema.** Nenhuma entidade, tabela ou coluna muda; bancos
`encurtaurl.db` existentes continuam válidos (Princípio IV).

## Regra alterada

| Regra (feature 001) | Antes | Depois |
| --- | --- | --- |
| Códigos reservados (ignorando caixa) | `urls`, `openapi` | `urls`, `openapi`, `scalar` |

Mensagem de recusa inalterada: `O código '{code}' é reservado e não pode ser usado.`
