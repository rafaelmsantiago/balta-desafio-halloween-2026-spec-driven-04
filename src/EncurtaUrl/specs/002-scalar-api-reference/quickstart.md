# Quickstart: validar a interface Scalar

## Build

```bash
cd src/EncurtaUrl.Api
dotnet build
```

Esperado: sucesso, 0 warnings (linha de base da feature 001).

## Em Development

```bash
dotnet run --launch-profile http
```

| # | Cenário | Esperado |
| --- | --- | --- |
| 1 | Abrir `http://localhost:5080/scalar` no navegador | Redireciona para `/scalar/` e mostra "Scalar API Reference" |
| 2 | Na página, conferir a lista de endpoints | Os 4 endpoints de [contracts/scalar-routes.md](contracts/scalar-routes.md), `POST /urls` com 201/400/409 |
| 3 | Na página, enviar `POST /urls` com `{"url":"https://balta.io","code":"teste-scalar"}` | 201 com o corpo da resposta |
| 4 | `POST /urls` com `"code": "Scalar"` (pelo `.http`) | 400, chave `code`, mensagem de reservado |

## Fora de Development

```bash
dotnet run --no-launch-profile --urls http://localhost:5080
```

(sem launch profile o ambiente padrão é Production)

| # | Cenário | Esperado |
| --- | --- | --- |
| 5 | `GET http://localhost:5080/scalar` | 404 |
| 6 | `GET http://localhost:5080/openapi/v1.json` | 404 (inalterado) |
