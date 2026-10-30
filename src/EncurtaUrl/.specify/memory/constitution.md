# EncurtaUrl Constitution

Esta constituição registra as convenções que o código de `src/EncurtaUrl.Api` já segue.
Toda feature nova MUST respeitar a arquitetura atual descrita aqui.

## Core Principles

### I. Simplicidade

- A solução é uma única API em .NET 10 (`net10.0`) com Minimal APIs.
- MUST NOT introduzir camadas extras (Application, Domain, Infrastructure etc.), MediatR,
  padrão Repository, Unit of Work ou abstrações equivalentes.
- Os handlers de endpoint MUST receber `AppDbContext` por injeção de parâmetro e usá-lo
  diretamente para consultas e gravações.
- Lógica pequena e local (validação, mapeamento para response) fica como método
  `private static` na própria classe de endpoints (ex.: `IsValidUrl`, `ToResponse`).

**Rationale**: o projeto é pequeno e didático; indireção adicional aumenta o custo de leitura
sem trazer benefício.

### II. Organização do Código

- `Endpoints/`: classes `static` com um método de extensão `Map{Recurso}Endpoints` sobre
  `IEndpointRouteBuilder`, que retorna o próprio `IEndpointRouteBuilder`. As rotas do recurso
  MUST ser agrupadas com `MapGroup(...)` e `.WithTags(...)`; cada rota MUST ter `.WithName(...)`.
  Handlers são métodos `private static async` com sufixo `Async` e recebem `CancellationToken`.
- `Contracts/`: `record`s de request e response (ex.: `CreateShortUrlRequest`,
  `ShortUrlResponse`). Entidades MUST NOT ser expostas diretamente nas respostas.
- `Models/`: entidades do EF Core como classes POCO com propriedades `get; set;`.
- `Data/`: `AppDbContext` (construtor primário) com o mapeamento Fluent em `OnModelCreating`.
- `Services/`: serviços pequenos com interface (`I{Nome}`) e implementação no mesmo arquivo,
  registrados em `Program.cs` (ex.: `AddSingleton<IShortCodeGenerator, ShortCodeGenerator>()`).
- `Program.cs` apenas registra serviços, cria o banco e chama os métodos `Map...Endpoints()`.
- Usar namespaces com escopo de arquivo (`namespace EncurtaUrl.Api.{Pasta};`).

**Rationale**: cada tipo de artefato tem um lugar previsível, o que permite que specs e planos
apontem arquivos sem ambiguidade.

### III. Respostas HTTP Tipadas

- Handlers MUST retornar `TypedResults` e declarar o tipo de retorno como `Results<...>`
  (ou o tipo concreto, como `Ok<T>`, quando houver um único resultado possível).
- Erros de validação de entrada MUST retornar `TypedResults.ValidationProblem(...)` (400),
  com chaves em camelCase correspondentes ao campo do request (ex.: `["url"]`).
- Demais erros MUST retornar `ProblemDetails` com o status correto, via
  `TypedResults.Problem(title: ..., statusCode: ...)`, ou via `TypedResults.NotFound()` etc.,
  que o pipeline (`AddProblemDetails` + `UseStatusCodePages` + `UseExceptionHandler`)
  converte em `ProblemDetails`.
- Criação MUST retornar `TypedResults.Created(location, response)`.

**Rationale**: `Results<...>` documenta no OpenAPI todos os resultados possíveis e é verificado
em tempo de compilação.

### IV. Dados com EF Core e SQLite

- Persistência MUST usar EF Core com o provedor SQLite e a connection string `Default`.
- O banco é criado com `Database.EnsureCreated()` na inicialização; MUST NOT adicionar
  migrations.
- Restrições de schema (obrigatoriedade, tamanho máximo, índices únicos) são declaradas via
  Fluent API em `AppDbContext.OnModelCreating`.
- Consultas somente leitura SHOULD usar `AsNoTracking()`.
- Qualquer mudança de schema (nova tabela, coluna, índice) MUST ser justificada no plano,
  incluindo o impacto em bancos já existentes: como `EnsureCreated` não altera um banco já
  criado, o plano MUST dizer se o arquivo `encurtaurl.db` precisa ser recriado.

**Rationale**: sem migrations, mudanças de schema não se propagam sozinhas; torná-las
explícitas evita bancos locais inconsistentes.

### V. Compatibilidade de Contratos

- Contratos existentes (rotas, métodos HTTP, nomes e tipos de campos de request e response,
  códigos de status) MUST NOT quebrar.
- Campos novos em requests MUST ser opcionais (tipo anulável ou com valor padrão), de modo que
  os requests atuais continuem válidos.
- Campos novos em responses são permitidos; campos existentes MUST NOT ser removidos ou
  renomeados.

**Rationale**: clientes atuais da API não devem precisar mudar para continuar funcionando.

### VI. Dependências Controladas

- MUST NOT adicionar pacote NuGet novo sem justificativa explícita no plano (problema resolvido,
  alternativa sem pacote considerada e motivo da rejeição).
- Dependências atuais: `Microsoft.AspNetCore.OpenApi` e `Microsoft.EntityFrameworkCore.Sqlite`.

**Rationale**: cada pacote é custo de manutenção e superfície de ataque.

### VII. Validação Manual e Build Limpo

- Não há projeto de testes automatizados.
- Toda feature MUST adicionar ao arquivo `EncurtaUrl.Api.http` pelo menos um cenário de sucesso
  e um cenário de erro para cada endpoint novo ou alterado, cada um com um título `###` que
  descreve o resultado esperado (ex.: `### URL inválida (deve retornar 400)`).
- `dotnet build` MUST concluir sem erros e sem warnings novos.

**Rationale**: o arquivo `.http` é a especificação executável da API neste projeto.

### VIII. Idioma

- Mensagens exibidas ao usuário (`title`/`detail` de `ProblemDetails`, mensagens de
  `ValidationProblem`) MUST estar em português.
- Código, identificadores e nomes técnicos seguem o padrão atual (inglês); comentários podem
  estar em português, como já ocorre no código.

**Rationale**: o público da API é brasileiro; o código segue as convenções do ecossistema .NET.

## Restrições Técnicas

- Plataforma: .NET 10, `Microsoft.NET.Sdk.Web`, com `Nullable` e `ImplicitUsings` habilitados.
- Pipeline: `AddOpenApi`/`MapOpenApi` (apenas em Development), `AddProblemDetails`,
  `UseExceptionHandler`, `UseStatusCodePages`.
- Rotas de recurso ficam sob grupos (ex.: `/urls`); a rota raiz `/{code}` é reservada para o
  redirecionamento e MUST NOT ser sombreada por novas rotas de um segmento.
- Configuração em `appsettings.json`; segredos MUST NOT ser versionados.

## Fluxo de Desenvolvimento e Quality Gates

- Fluxo Spec Kit: `/speckit-specify` → `/speckit-plan` → `/speckit-tasks` → `/speckit-implement`.
- O "Constitution Check" do plano MUST verificar cada princípio acima. Desvios (novo pacote,
  mudança de schema, nova abstração) MUST constar na seção de complexidade/justificativas do
  plano.
- Antes de concluir uma feature: `dotnet build` sem warnings novos, cenários de sucesso e erro
  adicionados ao `EncurtaUrl.Api.http`, e o `README.md` atualizado se a tabela de endpoints
  ou a estrutura de pastas mudar.

## Governance

- Esta constituição prevalece sobre outras práticas do projeto. Em caso de conflito entre o
  código e a constituição, o conflito MUST ser resolvido explicitamente (corrigir o código ou
  emendar a constituição), nunca ignorado.
- Emendas são feitas via `/speckit-constitution`, com Sync Impact Report e atualização de versão.
- Versionamento semântico: MAJOR para remoção ou redefinição incompatível de princípio; MINOR
  para princípio ou seção nova ou orientação materialmente ampliada; PATCH para esclarecimentos
  e ajustes de redação.
- Toda revisão de plano e de implementação MUST verificar a conformidade com estes princípios.

**Version**: 1.0.0 | **Ratified**: 2026-10-30 | **Last Amended**: 2026-10-30
