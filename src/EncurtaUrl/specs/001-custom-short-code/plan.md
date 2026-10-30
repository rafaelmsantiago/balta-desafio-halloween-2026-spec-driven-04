# Implementation Plan: Código Customizado para URL Curta

**Branch**: `001-custom-short-code` | **Date**: 2026-10-30 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/001-custom-short-code/spec.md`

**Note**: This template is filled in by the `/speckit-plan` command; its definition describes the execution workflow.

## Summary

Permitir que `POST /urls` receba um código opcional (`code`) usado no lugar do código gerado.
O record `CreateShortUrlRequest` ganha `string? Code = null`; as regras de tamanho (3–32),
formato (`A–Z a–z 0–9 - _`) e reservados (`urls`, `openapi`) ficam em
`Services/CustomCodeValidator.cs`; violações retornam `ValidationProblem` (400) na chave `code`.
Código já em uso retorna `ProblemDetails` 409, tanto pela pré-checagem quanto capturando a
violação do índice único no `SaveChangesAsync` em caso de corrida. Sem endpoint novo, sem mudança
de schema, sem pacote novo. Detalhes em [research.md](research.md).

## Technical Context

**Language/Version**: C# / .NET 10 (`net10.0`), `Nullable` e `ImplicitUsings` habilitados

**Primary Dependencies**: ASP.NET Core Minimal APIs, `Microsoft.AspNetCore.OpenApi`,
`Microsoft.EntityFrameworkCore.Sqlite` (inclui `Microsoft.Data.Sqlite` transitivamente)

**Storage**: SQLite (`encurtaurl.db`) via EF Core, criado com `EnsureCreated`; schema inalterado

**Testing**: sem projeto de testes; cenários manuais em `EncurtaUrl.Api.http` + `dotnet build`

**Target Platform**: servidor web (Kestrel), `http://localhost:5080`

**Project Type**: web-service (API única)

**Performance Goals**: sem meta nova; no máximo uma consulta extra por criação com código

**Constraints**: contratos existentes inalterados; mensagens em português; sem warnings novos

**Scale/Scope**: 1 record alterado, 1 serviço novo, 1 handler alterado, `Program.cs` (1 linha),
`.http` e `README.md`

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Princípio | Verificação | Status |
| --- | --- | --- |
| I. Simplicidade | Handler continua usando `AppDbContext` direto; nenhuma camada, MediatR ou repositório | ✅ |
| II. Organização | Request em `Contracts/`; validador em `Services/` com interface no mesmo arquivo; endpoint segue em `ShortUrlEndpoints` no grupo `/urls` | ✅ |
| III. Respostas HTTP | `Results<Created<…>, ValidationProblem, ProblemHttpResult>` mantido; 400 via `ValidationProblem`, 409 via `TypedResults.Problem`; `.ProducesProblem(409)` documenta | ✅ |
| IV. Dados | Nenhuma mudança de schema; coluna `Code` (máx. 32, índice único) reutilizada; `encurtaurl.db` existente não precisa ser recriado | ✅ |
| V. Compatibilidade | `Code` opcional com padrão `null`; resposta e demais rotas inalteradas | ✅ |
| VI. Dependências | Nenhum pacote novo; `SqliteException` vem de dependência transitiva já presente | ✅ |
| VII. Validação | Cenários de sucesso e erro adicionados ao `.http` (lista no [quickstart.md](quickstart.md)); `dotnet build` sem warnings novos | ✅ |
| VIII. Idioma | Todas as mensagens novas em português ([contracts/create-short-url.md](contracts/create-short-url.md)) | ✅ |
| Restrições Técnicas | `urls` e `openapi` reservados; `/{code}` não é sombreado | ✅ |

**Resultado (pré-pesquisa e pós-design)**: aprovado, sem violações.

## Project Structure

### Documentation (this feature)

```text
specs/001-custom-short-code/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── create-short-url.md
├── checklists/
│   └── requirements.md
└── tasks.md             # gerado por /speckit-tasks
```

### Source Code (repository root)

```text
src/EncurtaUrl.Api/
├── Contracts/ShortUrlContracts.cs     # ALTERAR: CreateShortUrlRequest(string Url, string? Code = null)
├── Endpoints/ShortUrlEndpoints.cs     # ALTERAR: CreateAsync (validação, 409, catch de unicidade); .ProducesProblem(409)
├── Services/CustomCodeValidator.cs    # NOVO: ICustomCodeValidator + CustomCodeValidator
├── Program.cs                         # ALTERAR: AddSingleton<ICustomCodeValidator, CustomCodeValidator>()
├── EncurtaUrl.Api.http                # ALTERAR: cenários novos
├── Models/ShortUrl.cs                 # sem mudança
└── Data/AppDbContext.cs               # sem mudança
README.md                              # ALTERAR: descrição do POST /urls e estrutura
```

**Structure Decision**: projeto único existente; a feature só toca os arquivos marcados acima.

### Fluxo do `CreateAsync` após a mudança

1. Montar o dicionário de erros: `url` inválida → mensagem atual; se `Code` não for nulo/vazio,
   `validator.Validate(Code)` → mensagens na chave `code`.
2. Se houver erros → `ValidationProblem` (400).
3. Se `Code` informado: `AnyAsync` pelo código → existe? `Problem` 409. Senão, usa o código.
   Se não informado: laço de geração atual, inalterado.
4. `Add` + `SaveChangesAsync`. Com código customizado, `catch (DbUpdateException) when` violação
   de índice único (`SqliteErrorCode` 19 / `SqliteExtendedErrorCode` 2067) → `Problem` 409.
5. `Created` com a resposta atual.

## Complexity Tracking

Sem violações da constituição a justificar.
