# Contrato: rotas da interface Scalar

Nenhum contrato existente muda (Princípio V). Rotas novas, apenas em Development:

| Método | Rota | Resposta em Development | Resposta fora de Development |
| --- | --- | --- | --- |
| GET | `/scalar` | 302 para `/scalar/` | 404 (cai em `/{code}`; `scalar` é reservado) |
| GET | `/scalar/` | 200, página HTML "Scalar API Reference" | 404 |

A página consome o documento existente `GET /openapi/v1.json`, que deve listar:

| Endpoint | Resultados documentados |
| --- | --- |
| `POST /urls` | 201, 400, 409 |
| `GET /urls` | 200 |
| `GET /urls/{code}` | 200, 404 |
| `GET /{code}` | 404 (o 302 de `RedirectHttpResult` pode não aparecer no OpenAPI; não é requisito) |

## Mudança em `POST /urls`

Somente a lista de reservados: `"code": "scalar"` (qualquer caixa) → 400, chave `code`, mensagem
`O código 'scalar' é reservado e não pode ser usado.` (com o valor enviado).
