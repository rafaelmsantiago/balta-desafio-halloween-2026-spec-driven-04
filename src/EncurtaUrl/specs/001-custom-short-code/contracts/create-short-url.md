# Contrato: POST /urls

Endpoint existente, estendido com um campo opcional. Nenhuma outra rota muda.

## Request

```json
{
  "url": "https://balta.io",
  "code": "balta-io"
}
```

| Campo | Tipo | Obrigatório | Observação |
| --- | --- | --- | --- |
| `url` | string | sim | inalterado |
| `code` | string \| null | **não** (novo) | ausente, `null` ou `""` → código gerado como hoje |

## Respostas

### 201 Created

`Location: /urls/{code}`. Corpo `ShortUrlResponse` inalterado:

```json
{
  "code": "balta-io",
  "shortUrl": "http://localhost:5080/balta-io",
  "originalUrl": "https://balta.io",
  "createdAt": "2026-10-30T18:00:00Z",
  "clicks": 0
}
```

### 400 Bad Request (ValidationProblem)

Chaves `url` e/ou `code`, cada uma com uma ou mais mensagens:

```json
{
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "code": [
      "O código deve conter apenas letras, números, hífen (-) e underscore (_)."
    ]
  }
}
```

Mensagens de `code`:

| Regra | Mensagem |
| --- | --- |
| Tamanho | `O código deve ter entre 3 e 32 caracteres.` |
| Formato | `O código deve conter apenas letras, números, hífen (-) e underscore (_).` |
| Reservado | `O código '{code}' é reservado e não pode ser usado.` |

A mensagem de `url` continua a atual.

### 409 Conflict (ProblemDetails)

```json
{
  "title": "O código informado já está em uso.",
  "status": 409,
  "detail": "O código 'balta-io' já está em uso. Escolha outro."
}
```

Retornado tanto pela pré-checagem quanto pela violação do índice único em gravações
concorrentes.

### 500 Internal Server Error

Inalterado: só no fluxo de código gerado, quando não é possível gerar um código único.

## Rotas inalteradas

`GET /urls`, `GET /urls/{code}` e `GET /{code}` mantêm contrato e comportamento; funcionam para
códigos customizados sem mudança.
