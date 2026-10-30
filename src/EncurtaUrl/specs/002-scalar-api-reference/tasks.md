---

description: "Task list for feature 002-scalar-api-reference"
---

# Tasks: Interface de Testes da API (Scalar)

**Input**: Design documents from `/specs/002-scalar-api-reference/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/scalar-routes.md, quickstart.md

**Tests**: sem projeto de testes (constituição, Princípio VII). Validação por cenários no
`src/EncurtaUrl.Api/EncurtaUrl.Api.http`, navegador e `dotnet build`.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

**Estado inicial**: a implementação foi feita antes da spec e está no commit `b067b3d`. As tarefas
já concluídas por esse commit estão marcadas `[X]` e precisam apenas ser conferidas.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1, US2, US3)
- Caminhos relativos à raiz do repositório (`src/EncurtaUrl.Api/...`)

---

## Phase 1: Setup (Shared Infrastructure)

- [X] T001 Adicionar `<PackageReference Include="Scalar.AspNetCore" Version="2.18.7" />` em `src/EncurtaUrl.Api/EncurtaUrl.Api.csproj` — versão fixa, sem curinga (research.md R1) — feito em `b067b3d`

---

## Phase 2: Foundational (Blocking Prerequisites)

Nenhuma: a página reutiliza o documento OpenAPI já existente (`AddOpenApi` / `MapOpenApi`).

---

## Phase 3: User Story 1 - Explorar e testar a API pelo navegador (Priority: P1) 🎯 MVP

**Goal**: página interativa em `/scalar` em Development

**Independent Test**: `dotnet run --launch-profile http`, abrir `http://localhost:5080/scalar`, enviar `POST /urls` pela página e ver 201

### Implementation for User Story 1

- [X] T002 [US1] Em `src/EncurtaUrl.Api/Program.cs`, adicionar `using Scalar.AspNetCore;` e chamar `app.MapScalarApiReference();` logo após `app.MapOpenApi();`, dentro do bloco `if (app.Environment.IsDevelopment())` — feito em `b067b3d`
- [X] T003 [US1] Em `src/EncurtaUrl.Api/EncurtaUrl.Api.http`, adicionar ao final `### Interface Scalar (200, apenas em Development)` com `GET {{baseUrl}}/scalar/`
- [X] T004 [US1] Validar os cenários 1–3 de `specs/002-scalar-api-reference/quickstart.md` em Development: `/scalar` → 302 para `/scalar/`; `/scalar/` → 200 com título "Scalar API Reference"; `/openapi/v1.json` lista `POST /urls` com 201, 400 e 409

**Checkpoint**: página funcional em Development

---

## Phase 4: User Story 2 - Interface não exposta fora de desenvolvimento (Priority: P1)

**Goal**: `/scalar` inexistente fora de Development

**Independent Test**: `dotnet run --no-launch-profile --urls http://localhost:5080` e `GET /scalar` → 404

### Implementation for User Story 2

- [X] T005 [US2] Conferir em `src/EncurtaUrl.Api/Program.cs` que `MapScalarApiReference()` está somente dentro do bloco `IsDevelopment` (sem alteração esperada)
- [X] T006 [US2] Validar os cenários 5–6 de `specs/002-scalar-api-reference/quickstart.md`: rodar `dotnet run --no-launch-profile --urls http://localhost:5080` em `src/EncurtaUrl.Api/` (ambiente Production) e confirmar `GET /scalar` → 404 e `GET /openapi/v1.json` → 404

**Checkpoint**: US1 e US2 verificadas

---

## Phase 5: User Story 3 - `scalar` não pode ser usado como código curto (Priority: P2)

**Goal**: `scalar` (qualquer caixa) recusado como código reservado

**Independent Test**: `POST /urls` com `"code": "Scalar"` → 400, chave `code`

### Implementation for User Story 3

- [X] T007 [P] [US3] Em `src/EncurtaUrl.Api/Services/CustomCodeValidator.cs`, `ReservedCodes` = `["urls", "openapi", "scalar"]` com `StringComparer.OrdinalIgnoreCase` — feito em `b067b3d`
- [X] T008 [US3] Em `src/EncurtaUrl.Api/EncurtaUrl.Api.http`, adicionar logo após `### Código reservado (400)` o cenário `### Código reservado scalar (400)` com POST `{"url": "https://balta.io", "code": "Scalar"}`; esperado: mensagem `O código 'Scalar' é reservado e não pode ser usado.`

**Checkpoint**: todas as histórias verificadas

---

## Phase 6: Polish & Cross-Cutting Concerns

- [X] T009 [P] Em `README.md`, linha `| GET | \`/scalar\` | Interface de testes Scalar (apenas em Development) |` na tabela de endpoints — feito em `b067b3d`
- [X] T010 Rodar `dotnet build` em `src/EncurtaUrl.Api/` e confirmar 0 erros e 0 warnings (linha de base)
- [X] T011 Remover `src/EncurtaUrl.Api/encurtaurl.db*` criado durante as validações

---

## Dependencies & Execution Order

- T001, T002, T007, T009 já concluídas (`b067b3d`)
- T003 e T008 alteram o mesmo arquivo (`EncurtaUrl.Api.http`): sequenciais
- T004 depende de T003 e T010; T006 depende de T010
- T004 (Development) e T006 (Production) não podem rodar ao mesmo tempo: usam a mesma porta 5080
- T011 por último

### User Story Dependencies

- US1, US2 e US3 independentes entre si; US3 depende apenas da feature 001 (lista de reservados)

### Parallel Opportunities

- Poucas: T003/T008 são o mesmo arquivo e as validações usam a mesma porta. T010 pode rodar enquanto se editam os cenários.

---

## Parallel Example: User Story 3

```bash
Task: "Conferir ReservedCodes em src/EncurtaUrl.Api/Services/CustomCodeValidator.cs"  # T007 (feito)
Task: "Conferir linha /scalar no README.md"                                           # T009 (feito)
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

Já entregue no código (`b067b3d`); falta T003 + T004 para cumprir o Princípio VII.

### Incremental Delivery

1. T010 (build) → T003 → T004 (US1)
2. T005 → T006 (US2)
3. T008 (US3) e validação do cenário novo
4. T011

---

## Notes

- Nenhuma mudança de schema, contrato ou mensagem
- O único pacote novo (`Scalar.AspNetCore`) está justificado no plan.md (Complexity Tracking)
