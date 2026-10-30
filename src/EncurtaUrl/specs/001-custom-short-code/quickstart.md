# Quickstart: validar o código customizado

## Pré-requisitos

- .NET 10 SDK.
- Banco limpo para resultados previsíveis: apague `src/EncurtaUrl.Api/encurtaurl.db` (o código
  `balta-io` só pode ser criado uma vez).

## Build

```bash
cd src/EncurtaUrl.Api
dotnet build
```

Esperado: sucesso, sem warnings novos (Princípio VII).

## Executar

```bash
dotnet run
```

A API sobe em `http://localhost:5080`. Use os cenários em `EncurtaUrl.Api.http`, na ordem:

| # | Cenário | Esperado |
| --- | --- | --- |
| 1 | POST `/urls` só com `url` (fluxo atual) | 201, código gerado de 7 caracteres |
| 2 | POST `/urls` com `url` inválida, sem `code` | 400, só a chave `url` (igual a hoje) |
| 3 | POST `/urls` com `code: "balta-io"` | 201, `code` = `balta-io`, `shortUrl` termina em `/balta-io` |
| 4 | GET `/balta-io` | 302 para `https://balta.io` |
| 5 | GET `/urls/balta-io` | 200, `clicks` = 1 |
| 6 | GET `/urls` | 200, lista contém URLs geradas e customizadas no mesmo formato |
| 7 | POST `/urls` com `code: "balta-io"` de novo | 409, "já está em uso" |
| 8 | POST `/urls` com `code: "balta.io"` | 400, chave `code`, mensagem de formato |
| 9 | POST `/urls` com `code: "ab"` | 400, chave `code`, mensagem de tamanho |
| 10 | POST `/urls` com `code` de 33 caracteres | 400, chave `code`, mensagem de tamanho |
| 11 | POST `/urls` com `code: "URLS"` | 400, chave `code`, mensagem de reservado |
| 12 | POST `/urls` com `code: ""` | 201, código gerado |
| 13 | POST `/urls` com `url` inválida e `code` inválido | 400, chaves `url` e `code` |

Contrato completo: [contracts/create-short-url.md](contracts/create-short-url.md).

## Corrida (opcional)

Disparar duas criações com o mesmo código novo ao mesmo tempo (ex.: dois terminais com `curl`):
uma retorna 201 e a outra 409, nunca 500.
