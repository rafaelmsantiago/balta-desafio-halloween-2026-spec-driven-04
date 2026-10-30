# Implementation Plan: Interface de Testes da API (Scalar)

**Branch**: `002-scalar-api-reference` | **Date**: 2026-10-30 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/002-scalar-api-reference/spec.md`

**Note**: This template is filled in by the `/speckit-plan` command; its definition describes the execution workflow.

## Summary

Expor em Development uma página interativa em `/scalar`, gerada a partir do documento OpenAPI já
existente, usando o pacote `Scalar.AspNetCore` 2.18.7 (justificado em [research.md](research.md#r1-pacote-scalaraspnetcore-justificativa-do-princípio-vi)).
`scalar` passa a ser código reservado. A implementação já existe no working tree; faltam os
cenários no `.http` e a validação fora de Development.

## Technical Context

**Language/Version**: C# / .NET 10 (`net10.0`)

**Primary Dependencies**: as atuais + `Scalar.AspNetCore` 2.18.7 (**novo**, justificado)

**Storage**: SQLite via EF Core; sem mudança

**Testing**: cenários no `EncurtaUrl.Api.http` + `dotnet build`; página verificada no navegador

**Target Platform**: servidor web (Kestrel), `http://localhost:5080`

**Project Type**: web-service (API única)

**Performance Goals**: N/A (ferramenta de desenvolvimento)

**Constraints**: só em Development; nenhum contrato existente muda; sem warnings novos

**Scale/Scope**: `EncurtaUrl.Api.csproj` (1 linha), `Program.cs` (2 linhas), `CustomCodeValidator.cs`
(1 valor), `EncurtaUrl.Api.http` (cenários), `README.md` (1 linha)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Princípio | Verificação | Status |
| --- | --- | --- |
| I. Simplicidade | Uma chamada `MapScalarApiReference()`, sem configuração nem camada nova | ✅ |
| II. Organização | Mudança só em `Program.cs` (registro de pipeline) e na lista de reservados em `Services/` | ✅ |
| III. Respostas HTTP | Nenhum handler muda; recusa de `scalar` usa o `ValidationProblem` existente | ✅ |
| IV. Dados | Sem mudança de schema; banco existente não precisa ser recriado | ✅ |
| V. Compatibilidade | Nenhum contrato muda; rotas novas só em Development | ✅ |
| VI. Dependências | **Pacote novo** `Scalar.AspNetCore` — justificado em research.md R1 e em Complexity Tracking | ✅ (justificado) |
| VII. Validação | Cenários de sucesso (`/scalar/`) e erro (código `scalar`) no `.http`; build sem warnings | ⏳ cenários pendentes (tasks) |
| VIII. Idioma | Mensagem de reservado já em português; nenhuma mensagem nova | ✅ |
| Restrições Técnicas | `/scalar` não sombreia `/{code}` útil, pois `scalar` é reservado | ✅ |

**Resultado (pré-pesquisa e pós-design)**: aprovado; a única exceção (pacote novo) está
justificada. O item VII fica pendente até as tasks adicionarem os cenários ao `.http`.

## Project Structure

### Documentation (this feature)

```text
specs/002-scalar-api-reference/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── scalar-routes.md
├── checklists/
│   └── requirements.md
└── tasks.md             # gerado por /speckit-tasks
```

### Source Code (repository root)

```text
src/EncurtaUrl.Api/
├── EncurtaUrl.Api.csproj              # FEITO: PackageReference Scalar.AspNetCore 2.18.7
├── Program.cs                         # FEITO: using Scalar.AspNetCore; MapScalarApiReference() no bloco IsDevelopment
├── Services/CustomCodeValidator.cs    # FEITO: "scalar" em ReservedCodes
└── EncurtaUrl.Api.http                # PENDENTE: cenários de /scalar/ e código reservado scalar
README.md                              # FEITO: linha /scalar na tabela de endpoints
```

**Structure Decision**: projeto único existente; sem arquivos novos de código.

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
| --- | --- | --- |
| Pacote NuGet novo `Scalar.AspNetCore` (Princípio VI) | Interface interativa de testes pedida pelo usuário; o .NET 10 não inclui uma | Página HTML com Scalar via CDN exige HTML e rota mantidos à mão e rede externa; Swashbuckle/NSwag também são pacotes e não foram pedidos |
