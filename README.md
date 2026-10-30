<img width="100%" alt="Halloween 2026" src="https://baltaio.blob.core.windows.net/static/images/v4/challenges/halloween-2026/banner.jpg" />

## 🎃 Halloween - Desafio 4

Oi, eu sou o Rafael Santiago e este é o espaço onde compartilho minha jornada de aprendizado durante o desafio **Halloween 2026**, realizado pelo [balta.io](https://balta.io). 👻

Aqui você vai encontrar projetos, exercícios e códigos que estou desenvolvendo durante o desafio.

### Sobre este desafio
Neste desafio o objetivo é adicionar o SpecKit a um projeto já existente e adicionar novas funcionalidades a ele.

#### O que foi implementado

- **SpecKit no projeto existente**: configuração do SpecKit (com a extensão de Git) na API [EncurtaUrl](src/EncurtaUrl) (.NET 10, Minimal APIs, EF Core + SQLite).
- **Constituição v1.0.0** ([constitution.md](src/EncurtaUrl/.specify/memory/constitution.md)): 8 princípios extraídos do código que já existia (simplicidade sem camadas extras, organização de pastas, `TypedResults` + `ProblemDetails`, `EnsureCreated` sem migrations, compatibilidade de contratos, controle de pacotes NuGet, validação pelo arquivo `.http` e mensagens em português).
- **Feature 001 – Código customizado** ([specs/001-custom-short-code](src/EncurtaUrl/specs/001-custom-short-code)):
  - `POST /urls` aceita o campo opcional `code` (ex.: `/balta-io` em vez de `/aZ3k9Qx`); sem ele, tudo continua como antes.
  - Regras em `CustomCodeValidator`: 3 a 32 caracteres, apenas letras, números, `-` e `_`, e códigos reservados (`urls`, `openapi`, `scalar`) que conflitariam com rotas da API.
  - Código já em uso retorna **409**, inclusive quando duas requisições tentam o mesmo código ao mesmo tempo (violação do índice único capturada no `SaveChanges`).
  - Erros de validação retornam **400** com as mensagens de `url` e `code` juntas.
  - Sem mudança de schema e sem pacote novo.
- **Feature 002 – Interface de testes Scalar** ([specs/002-scalar-api-reference](src/EncurtaUrl/specs/002-scalar-api-reference)): página interativa em `/scalar`, disponível apenas em Development, gerada a partir do documento OpenAPI.
- **Cenários de teste** no [EncurtaUrl.Api.http](src/EncurtaUrl/src/EncurtaUrl.Api/EncurtaUrl.Api.http) para todos os casos de sucesso e erro, validados com a API rodando.

Neste processo eu aprendi:

* ✅ Em um projeto existente, a constituição deve registrar as convenções que o código **já segue**, para que cada feature nova respeite a arquitetura atual em vez de inventar outra.
* ✅ O fluxo `specify → plan → tasks → implement` separa bem **o quê** (spec, sem detalhes técnicos) do **como** (plano, com decisões e alternativas registradas no `research.md`).
* ✅ A constituição funciona como um *gate*: quando adicionei o Scalar fora do fluxo, o princípio de dependências exigiu uma justificativa, e por isso a mudança virou uma feature própria (002) com spec e plano.
* ✅ Uma rota nova de primeiro nível (`/scalar`) pode "engolir" um código curto (`/{code}`), então toda rota nova precisa entrar na lista de reservados.
* ✅ Checar se o código existe antes de gravar não basta sob concorrência; o índice único do banco é quem garante a regra.
* ✅ Em .NET, o `$` de uma regex aceita um `\n` no final da string; por isso a validação do código usa um laço simples em vez de regex.
* ✅ Os hooks da extensão de Git do SpecKit criam a branch da feature automaticamente, mas o auto-commit vem desligado por padrão.

## Badge
<img src="https://baltaio.blob.core.windows.net/static/images/v4/challenges/halloween-2026/04.png" width="200" />

## Problema
A API EncurtaUrl só gerava códigos aleatórios (ex.: `/aZ3k9Qx`), difíceis de lembrar e de divulgar. Era preciso permitir que o usuário escolhesse um código legível (ex.: `/balta-io`) **sem quebrar** os clientes atuais e **sem mudar** a arquitetura do projeto. Isso trazia alguns riscos:

- códigos duplicados, inclusive em requisições simultâneas;
- códigos que conflitam com rotas da própria API (`/urls`, `/openapi`, `/scalar`) e nunca redirecionariam;
- códigos com caracteres inválidos na URL.

Além disso, não havia uma interface para explorar e testar a API, o que foi resolvido com o Scalar, restrito ao ambiente de desenvolvimento.

## Sobre o Halloween 2026
O desafio **Halloween 2026** consiste em implementar o modelo Spec Driven Development de ponta a ponta, criando apps completas com IA.

### Veja meu progresso no desafio
[balta-io/balta-desafio-halloween-2026-spec-driven-04](https://github.com/balta-io/balta-desafio-halloween-2026-spec-driven-04)
