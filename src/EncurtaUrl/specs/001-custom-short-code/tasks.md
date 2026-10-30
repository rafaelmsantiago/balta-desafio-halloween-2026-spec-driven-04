---

description: "Task list for feature 001-custom-short-code"
---

# Tasks: Código Customizado para URL Curta

**Input**: Design documents from `/specs/001-custom-short-code/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/create-short-url.md, quickstart.md

**Tests**: sem projeto de testes (constituição, Princípio VII). A validação é feita por cenários
no `src/EncurtaUrl.Api/EncurtaUrl.Api.http` e por `dotnet build` sem warnings novos.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1, US2, US3)
- Caminhos relativos à raiz do repositório (`src/EncurtaUrl.Api/...`)

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: registrar a linha de base antes de mexer no código

- [X] T001 Rodar `dotnet build` em `src/EncurtaUrl.Api/` e anotar a quantidade de warnings atual (linha de base para "sem warnings novos")

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: contrato do request, usado por todas as histórias

- [X] T002 Em `src/EncurtaUrl.Api/Contracts/ShortUrlContracts.cs`, alterar para `public record CreateShortUrlRequest(string Url, string? Code = null);` — o parâmetro `Code` MUST ser opcional com padrão `null`; `ShortUrlResponse` não muda

**Checkpoint**: projeto compila e requests atuais (`{ "url": "..." }`) continuam válidos

---

## Phase 3: User Story 1 - Encurtar com código customizado (Priority: P1) 🎯 MVP

**Goal**: `POST /urls` com `code` cria a URL curta com esse código; `/{code}` redireciona e conta o clique

**Independent Test**: criar com `"code": "balta-io"`, conferir `code`/`shortUrl` na resposta 201, acessar `GET /balta-io` (302) e ver `clicks` = 1 em `GET /urls/balta-io`

### Implementation for User Story 1

- [X] T003 [US1] Em `CreateAsync` de `src/EncurtaUrl.Api/Endpoints/ShortUrlEndpoints.cs`: quando `!string.IsNullOrEmpty(request.Code)`, usar `request.Code` como código e não executar o laço de geração; caso contrário manter o laço de geração exatamente como está (sem `Trim` no código)
- [X] T004 [US1] Em `src/EncurtaUrl.Api/EncurtaUrl.Api.http`, adicionar após os cenários de criação existentes: `### Criar com código customizado (201, code = balta-io)` (POST com `"url": "https://balta.io", "code": "balta-io"`), `### Navegar pelo código customizado (302)` (`GET {{baseUrl}}/balta-io`) e `### Detalhes do código customizado (200, clicks = 1)` (`GET {{baseUrl}}/urls/balta-io`)

**Checkpoint**: US1 funciona ponta a ponta para códigos válidos

---

## Phase 4: User Story 2 - Fluxo atual sem código continua igual (Priority: P1)

**Goal**: requests sem `code` (ausente, `null` ou `""`) se comportam como antes

**Independent Test**: rodar os cenários originais do `.http` e o novo com `"code": ""`; todos retornam código gerado de 7 caracteres e o erro de URL inválida continua só na chave `url`

### Implementation for User Story 2

- [X] T005 [US2] Revisar `CreateAsync` em `src/EncurtaUrl.Api/Endpoints/ShortUrlEndpoints.cs` e confirmar que, com `Code` nulo ou vazio, o caminho executado (validação de URL, laço de geração, 500 "Não foi possível gerar um código único.", `SaveChangesAsync` sem `try/catch`) é idêntico ao original
- [X] T006 [US2] Em `src/EncurtaUrl.Api/EncurtaUrl.Api.http`, manter intactos os cenários existentes e adicionar `### Código vazio é tratado como não informado (201, código gerado)` com `"url": "https://balta.io/blog", "code": ""`

**Checkpoint**: US1 e US2 funcionam; nenhum cliente atual quebra

---

## Phase 5: User Story 3 - Recusa de códigos inválidos, reservados ou em uso (Priority: P2)

**Goal**: códigos fora do formato, reservados ou já usados são recusados com mensagem em português, sem gravar nada

**Independent Test**: enviar `balta.io`, `ab`, 33 caracteres, `URLS` (400 na chave `code`), `balta-io` repetido (409) e URL + código inválidos juntos (400 com `url` e `code`)

### Implementation for User Story 3

- [X] T007 [P] [US3] Criar `src/EncurtaUrl.Api/Services/CustomCodeValidator.cs` (namespace `EncurtaUrl.Api.Services`) com `public interface ICustomCodeValidator { string[] Validate(string code); }` e `public class CustomCodeValidator : ICustomCodeValidator` no mesmo arquivo, seguindo o estilo de `ShortCodeGenerator.cs`. Constantes `MinLength = 3`, `MaxLength = 32`; `HashSet<string>` reservados `["urls", "openapi"]` com `StringComparer.OrdinalIgnoreCase`. `Validate` devolve lista vazia se válido, senão as mensagens aplicáveis, nesta ordem:
  - tamanho fora de 3–32: `O código deve ter entre 3 e 32 caracteres.`
  - algum caractere que não satisfaz `char.IsAsciiLetterOrDigit(c) || c is '-' or '_'` (laço, sem regex): `O código deve conter apenas letras, números, hífen (-) e underscore (_).`
  - reservado: `$"O código '{code}' é reservado e não pode ser usado."`
- [X] T008 [P] [US3] Em `src/EncurtaUrl.Api/Program.cs`, registrar `builder.Services.AddSingleton<ICustomCodeValidator, CustomCodeValidator>();` logo após o registro de `IShortCodeGenerator`
- [X] T009 [US3] Em `CreateAsync` de `src/EncurtaUrl.Api/Endpoints/ShortUrlEndpoints.cs` (depende de T007): injetar `ICustomCodeValidator validator`; montar `var errors = new Dictionary<string, string[]>()`; se a URL for inválida, `errors["url"]` = mensagem atual (texto inalterado); se `Code` não for nulo/vazio e `validator.Validate(request.Code)` não for vazio, `errors["code"]` = essas mensagens; se `errors.Count > 0` → `TypedResults.ValidationProblem(errors)`
- [X] T010 [US3] Em `CreateAsync` de `src/EncurtaUrl.Api/Endpoints/ShortUrlEndpoints.cs`: com código customizado válido, antes de gravar, `if (await db.ShortUrls.AnyAsync(x => x.Code == request.Code, ct))` → `return CodeInUse(request.Code);`, onde `CodeInUse` é um método `private static ProblemHttpResult` que retorna `TypedResults.Problem(title: "O código informado já está em uso.", detail: $"O código '{code}' já está em uso. Escolha outro.", statusCode: StatusCodes.Status409Conflict)`
- [X] T011 [US3] Em `CreateAsync` de `src/EncurtaUrl.Api/Endpoints/ShortUrlEndpoints.cs`: somente no caminho de código customizado, envolver `SaveChangesAsync` em `try { ... } catch (DbUpdateException ex) when (IsUniqueViolation(ex)) { return CodeInUse(code); }`. Adicionar `using Microsoft.Data.Sqlite;` e `private static bool IsUniqueViolation(DbUpdateException ex) => ex.InnerException is SqliteException { SqliteErrorCode: 19, SqliteExtendedErrorCode: 2067 };` (SQLITE_CONSTRAINT / SQLITE_CONSTRAINT_UNIQUE). Não adicionar pacote NuGet; o caminho de código gerado continua sem `try/catch`
- [X] T012 [US3] Em `MapShortUrlEndpoints` de `src/EncurtaUrl.Api/Endpoints/ShortUrlEndpoints.cs`, encadear `.ProducesProblem(StatusCodes.Status409Conflict)` no `group.MapPost("/", CreateAsync)`; manter a assinatura `Results<Created<ShortUrlResponse>, ValidationProblem, ProblemHttpResult>`
- [X] T013 [US3] Em `src/EncurtaUrl.Api/EncurtaUrl.Api.http`, adicionar após os cenários de US1: `### Código já em uso (409)` (repete `balta-io`), `### Código com caractere inválido (400)` (`balta.io`), `### Código curto demais (400)` (`ab`), `### Código longo demais (400)` (33 caracteres, ex.: `abcdefghijklmnopqrstuvwxyz0123456`), `### Código reservado (400)` (`URLS`) e `### URL e código inválidos (400 com url e code)` (`"url": "isso-nao-e-uma-url", "code": "a b"`)

**Checkpoint**: todas as histórias funcionam de forma independente

---

## Phase 6: Polish & Cross-Cutting Concerns

- [X] T014 [P] Em `README.md`, atualizar a descrição de `POST /urls` para "Cria uma URL curta com código customizado opcional (`code`) ou gerado automaticamente" e incluir `Services/CustomCodeValidator.cs   # regras do código customizado` na árvore de estrutura
- [X] T015 Rodar `dotnet build` em `src/EncurtaUrl.Api/` e confirmar zero erros e nenhum warning além da linha de base de T001
- [X] T016 Apagar `src/EncurtaUrl.Api/encurtaurl.db`, rodar `dotnet run` e executar os 13 cenários de `specs/001-custom-short-code/quickstart.md`, conferindo status e mensagens com `specs/001-custom-short-code/contracts/create-short-url.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (T001)** → **Foundational (T002)** → histórias → **Polish (T014–T016)**

### User Story Dependencies

- **US1**: depende só de T002
- **US2**: depende de T003 (revisa o mesmo método); verificável sem US3
- **US3**: T007 e T008 podem começar logo após T002; T009–T012 dependem de T003 e T007

### Within Each User Story

- Todas as alterações de `ShortUrlEndpoints.cs` (T003, T005, T009–T012) são sequenciais
- Todas as alterações de `EncurtaUrl.Api.http` (T004, T006, T013) são sequenciais

### Parallel Opportunities

- T007 (novo arquivo) e T008 (`Program.cs`) em paralelo entre si e com T003/T004
- T014 (`README.md`) em paralelo com qualquer tarefa de código

---

## Parallel Example: User Story 3

```bash
Task: "Criar CustomCodeValidator em src/EncurtaUrl.Api/Services/CustomCodeValidator.cs"   # T007
Task: "Registrar ICustomCodeValidator em src/EncurtaUrl.Api/Program.cs"                 # T008
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. T001 → T002 → T003 → T004
2. Validar US1 pelo `.http`
3. Não publicar sem US3: sem ela, códigos inválidos ou reservados seriam aceitos

### Incremental Delivery

1. US1 + US2 → códigos customizados funcionando e compatibilidade garantida
2. US3 → regras de formato, reservados e conflito (necessária antes de liberar)
3. Polish → README, build limpo, quickstart

---

## Notes

- Nenhuma mudança em `Models/ShortUrl.cs` ou `Data/AppDbContext.cs` (schema inalterado)
- Nenhum pacote NuGet novo; `Microsoft.Data.Sqlite` já é dependência transitiva
- Mensagens ao usuário em português, exatamente como em `contracts/create-short-url.md`
