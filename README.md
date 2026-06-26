# ATS — Applicant Tracking System API

> REST API para gerenciamento de processos seletivos, construída com ASP.NET Core 10, Domain-Driven Design e observabilidade de produção.

[![CI](https://github.com/marlongreis91/ATS.Solution/actions/workflows/ci.yml/badge.svg)](https://github.com/marlongreis91/ATS.Solution/actions/workflows/ci.yml)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com)
[![MongoDB](https://img.shields.io/badge/MongoDB-7.0-47A248)](https://www.mongodb.com)
[![BDD](https://img.shields.io/badge/BDD-Gherkin-brightgreen)](docs/bdd/README.md)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

---

> **📋 Documentação de regras de negócio em BDD/Gherkin:** [`docs/bdd/`](docs/bdd/README.md)  
> ~101 cenários cobrindo Candidatos, Vagas, Candidaturas, Paginação e Tratamento de Erros — prontos para automação com Reqnroll/SpecFlow.

---

## Sumário

- [Objetivo](#objetivo)
- [Funcionalidades](#funcionalidades)
- [Tecnologias](#tecnologias)
- [Arquitetura](#arquitetura)
- [Estrutura de diretórios](#estrutura-de-diretórios)
- [Pré-requisitos](#pré-requisitos)
- [Execução local (.NET CLI)](#execução-local-net-cli)
- [Execução via Docker Compose](#execução-via-docker-compose)
- [Variáveis de ambiente](#variáveis-de-ambiente)
- [Banco de dados](#banco-de-dados)
- [Swagger / OpenAPI](#swagger--openapi)
- [Endpoints da API](#endpoints-da-api)
- [Testes](#testes)
- [Cobertura de código](#cobertura-de-código)
- [Observabilidade](#observabilidade)
- [Decisões técnicas](#decisões-técnicas)
- [Convenções de desenvolvimento](#convenções-de-desenvolvimento)
- [Tratamento de erros](#tratamento-de-erros)
- [CI/CD](#cicd)
- [**Documentação BDD**](#documentação-bdd) — [📋 Abrir docs/bdd/](docs/bdd/README.md)
- [Roadmap](#roadmap)
- [Licença](#licença)
- [Autor](#autor)

---

## Objetivo

O ATS é um sistema de rastreamento de candidatos (Applicant Tracking System) desenvolvido como desafio técnico para demonstrar a aplicação de boas práticas de engenharia de software em projetos .NET. A API gerencia os três agregados centrais de um processo seletivo: **Candidatos**, **Vagas** e **Candidaturas**, com transições de estado controladas por regras de domínio.

---

## Funcionalidades

| Área | Operações |
|------|-----------|
| **Candidatos** | Cadastro, consulta, listagem paginada, atualização de contato, upload de currículo |
| **Vagas** | Publicação, consulta, listagem paginada, atualização, encerramento, exclusão |
| **Candidaturas** | Criação, consulta, listagem por vaga, aprovação, reprovação, cancelamento |
| **Observabilidade** | Logs estruturados (JSON), tracing distribuído (OTLP/Jaeger), métricas Prometheus, health checks |

---

## Tecnologias

| Camada | Tecnologia |
|--------|-----------|
| Runtime | [.NET 10](https://dotnet.microsoft.com) / ASP.NET Core 10 |
| Banco de dados | [MongoDB 7.0](https://www.mongodb.com) via [MongoDB.Driver](https://www.nuget.org/packages/MongoDB.Driver) |
| Logs | [Serilog](https://serilog.net) + Compact JSON Formatter |
| Tracing | [OpenTelemetry](https://opentelemetry.io) (OTLP exporter, Jaeger) |
| Métricas | OpenTelemetry Metrics + [Prometheus](https://prometheus.io) |
| Testes | [xUnit](https://xunit.net) + [Moq](https://github.com/moq/moq4) |
| Cobertura | [Coverlet](https://github.com/coverlet-coverage/coverlet) + [ReportGenerator](https://github.com/danielpalme/ReportGenerator) |
| Containerização | [Docker](https://www.docker.com) + Docker Compose |
| CI | [GitHub Actions](https://github.com/features/actions) |

---

## Arquitetura

O projeto segue a arquitetura em camadas do **Domain-Driven Design (DDD)**, com separação estrita de responsabilidades:

```
┌─────────────────────────────────────────────────────────────┐
│                        ATS.API                              │
│  Controllers · Middlewares · Observability · Program.cs     │
└────────────────────────────┬────────────────────────────────┘
                             │ depende de
┌────────────────────────────▼────────────────────────────────┐
│                     ATS.Application                         │
│  Commands · Queries · Handlers · DTOs · Metrics             │
└────────┬───────────────────────────────────────┬────────────┘
         │ depende de                            │ depende de
┌────────▼───────────┐              ┌────────────▼────────────┐
│    ATS.Domain      │              │    ATS.Infrastructure    │
│  Entities · VOs    │◄─interfaces──│  Repositories · MongoDB  │
│  Aggregates        │              │  Mappings · Health Checks │
│  Domain Events     │              └─────────────────────────┘
│  DomainException   │
└────────────────────┘
```

### Responsabilidades por camada

**`ATS.Domain`** — núcleo da aplicação, sem dependências externas.
- Entidades (`Candidato`, `Vaga`, `Candidatura`) e regras de negócio via métodos de domínio
- Value Objects imutáveis (`Email`, `Telefone`, `Salario`, `Curriculo`) com validação encapsulada
- Enums de estado (`StatusCandidatura`, `StatusVaga`)
- Eventos de domínio (`CandidatoCriadoEvent`, `VagaPublicadaEvent`, etc.)
- Interfaces de repositório (`ICandidatoRepository`, `IVagaRepository`, `ICandidaturaRepository`)
- `DomainException` como mecanismo de sinalização de invariantes violadas

**`ATS.Application`** — orquestração dos casos de uso.
- Handlers de Commands (operações de escrita) e Queries (operações de leitura) por agregado
- DTOs para projeção de dados entre camadas
- `PagedResult<T>` para respostas paginadas
- `AtsMetrics` com contadores OpenTelemetry para métricas de negócio
- Sem referência a frameworks web ou de persistência

**`ATS.Infrastructure`** — implementações de infraestrutura.
- Implementações concretas dos repositórios usando `MongoDB.Driver`
- `MongoDbContext` e mapeamento BSON via `BsonClassMap`
- `MongoDbHealthCheck` para o health check `ready`
- `DependencyInjection` — registro de todos os serviços e handlers no container IoC

**`ATS.API`** — camada de entrada HTTP.
- Controllers com rotas versionadas (`/api/v1/...`) e documentação XML
- `ExceptionHandlingMiddleware` — traduz exceções para `ProblemDetails` (RFC 7807)
- `ObservabilityExtensions` — configura Serilog, OpenTelemetry e Prometheus
- Cabeçalhos de segurança (CSP, X-Frame-Options, etc.)
- Health checks (`/health/live`, `/health/ready`)
- `Program.cs` com configuração de CORS, `ForwardedHeaders` e Swagger

**`tests/`** — estratégia de testes em cinco projetos:
- `ATS.Domain.Tests` — regras de domínio, value objects e invariantes
- `ATS.Application.Tests` — handlers com repositórios mockados (MockBehavior.Strict)
- `ATS.Infrastructure.Tests` — repositórios contra MongoDB em memória/real
- `ATS.API.Tests` — controllers, middlewares e extensões de observabilidade
- `ATS.E2E.Tests` — fluxos completos com Testcontainers + MongoDB real

---

## Estrutura de diretórios

```
ATS.Solution/
├── src/
│   ├── ATS.API/
│   │   ├── Controllers/
│   │   │   ├── CandidatosController.cs
│   │   │   ├── VagasController.cs
│   │   │   └── CandidaturasController.cs
│   │   ├── Middlewares/
│   │   │   └── ExceptionHandlingMiddleware.cs
│   │   ├── Observability/
│   │   │   ├── ObservabilityExtensions.cs
│   │   │   └── ObservabilitySettings.cs
│   │   ├── Requests/
│   │   ├── appsettings.json
│   │   ├── appsettings.Development.json
│   │   ├── Dockerfile
│   │   └── Program.cs
│   │
│   ├── ATS.Application/
│   │   ├── Candidatos/
│   │   │   ├── Commands/          # CreateCandidato, UpdateCandidato, DeleteCandidato, AddCurriculo
│   │   │   ├── Queries/           # GetCandidatoById, ListCandidatos
│   │   │   └── DTOs/
│   │   ├── Vagas/
│   │   │   ├── Commands/          # CreateVaga, UpdateVaga, DeleteVaga, FecharVaga
│   │   │   ├── Queries/           # GetVagaById, ListVagas
│   │   │   └── DTOs/
│   │   ├── Candidaturas/
│   │   │   ├── Commands/          # CandidatarSe, AprovarCandidatura, ReprovarCandidatura, CancelarCandidatura
│   │   │   ├── Queries/           # GetCandidaturaById, ListCandidatosPorVaga
│   │   │   └── DTOs/
│   │   ├── Common/
│   │   │   └── Pagination/        # PagedResult<T>
│   │   └── Observability/
│   │       └── AtsMetrics.cs
│   │
│   ├── ATS.Domain/
│   │   ├── Candidatos/
│   │   │   ├── Entities/          # Candidato
│   │   │   ├── Events/            # CandidatoCriadoEvent, CurriculoAdicionadoEvent
│   │   │   ├── Repositories/      # ICandidatoRepository
│   │   │   └── ValueObjects/      # Email, Telefone, Curriculo
│   │   ├── Vagas/
│   │   │   ├── Entities/          # Vaga
│   │   │   ├── Enums/             # StatusVaga
│   │   │   ├── Events/            # VagaPublicadaEvent
│   │   │   ├── Repositories/      # IVagaRepository
│   │   │   └── ValueObjects/      # Salario
│   │   ├── Candidaturas/
│   │   │   ├── Entities/          # Candidatura
│   │   │   ├── Enums/             # StatusCandidatura
│   │   │   ├── Events/            # CandidaturaRealizadaEvent
│   │   │   └── Repositories/      # ICandidaturaRepository
│   │   └── Shared/
│   │       ├── AggregateRoot.cs
│   │       ├── Entity.cs
│   │       ├── ValueObject.cs
│   │       ├── IDomainEvent.cs
│   │       └── DomainException.cs
│   │
│   └── ATS.Infrastructure/
│       ├── Health/
│       │   └── MongoDbHealthCheck.cs
│       ├── Persistence/
│       │   ├── Context/           # MongoDbContext, MongoDbSettings
│       │   ├── Mappings/          # BsonClassMap para cada agregado
│       │   └── Repositories/      # CandidatoRepository, VagaRepository, CandidaturaRepository
│       └── DependencyInjection.cs
│
├── tests/
│   ├── ATS.Domain.Tests/
│   ├── ATS.Application.Tests/
│   ├── ATS.Infrastructure.Tests/
│   ├── ATS.API.Tests/
│   └── ATS.E2E.Tests/
│
├── docs/
│   └── bdd/
│       ├── README.md              # Índice, convenções e instruções de automação
│       ├── Candidatos.feature     # Regras de negócio de candidatos (Gherkin)
│       ├── Vagas.feature          # Regras de negócio de vagas (Gherkin)
│       ├── Candidaturas.feature   # Regras de negócio de candidaturas (Gherkin)
│       ├── Paginacao.feature      # Regras de paginação (Gherkin)
│       └── TratamentoDeErros.feature # Comportamento de erros (Gherkin)
├── infra/
│   └── prometheus.yml
├── docker-compose.yml
└── ATS.Solution.slnx
```

---

## Pré-requisitos

| Ferramenta | Versão mínima | Uso |
|------------|--------------|-----|
| [.NET SDK](https://dotnet.microsoft.com/download) | 10.0 | Build e execução local |
| [Docker](https://www.docker.com/get-started) | 24+ | Containerização e serviços auxiliares |
| [Docker Compose](https://docs.docker.com/compose/) | v2 | Orquestração local |
| [MongoDB](https://www.mongodb.com/try/download/community) | 7.0 | Apenas para execução sem Docker |

---

## Execução local (.NET CLI)

### 1. Clonar o repositório

```bash
git clone https://github.com/marlongreis91/ATS.Solution.git
cd ATS.Solution
```

### 2. Configurar o banco de dados

Certifique-se de ter o MongoDB rodando localmente na porta padrão `27017` (sem autenticação).  
O arquivo `appsettings.Development.json` já aponta para `mongodb://localhost:27017` com o banco `AtsDbLocal`.

### 3. Restaurar dependências e executar

```bash
dotnet restore
dotnet run --project src/ATS.API
```

A API ficará disponível em:
- **HTTP**: `http://localhost:5031`
- **Swagger UI**: `http://localhost:5031/swagger`
- **Métricas**: `http://localhost:5031/metrics`
- **Health live**: `http://localhost:5031/health/live`
- **Health ready**: `http://localhost:5031/health/ready`

---

## Execução via Docker Compose

### 1. Criar o arquivo `.env`

```bash
cp .env.example .env  # ou crie manualmente
```

Conteúdo mínimo do `.env`:

```env
MONGO_ROOT_USERNAME=admin
MONGO_ROOT_PASSWORD=changeme_strong_password
MONGO_DATABASE_NAME=AtsDb
API_HTTP_PORT=5031
```

### 2. Subir os serviços principais

```bash
docker compose up -d
```

Serviços iniciados:
- `ats-api` — API na porta configurada em `API_HTTP_PORT` (default: `5031`)
- `ats-mongo` — MongoDB na porta `27017` (acesso local apenas via `127.0.0.1`)

### 3. Subir com ferramentas de observabilidade (opcional)

```bash
docker compose --profile observability up -d
```

Adiciona:
- `ats-jaeger` — UI de tracing em `http://localhost:16686`
- `ats-prometheus` — em `http://localhost:9090`

### 4. Subir com interface de administração do MongoDB (opcional)

```bash
docker compose --profile tools up -d
```

Adiciona:
- `ats-mongo-express` — em `http://localhost:8081` (requer usuário/senha configurados)

### Comandos úteis

```bash
# Ver logs da API em tempo real
docker compose logs -f api

# Parar todos os serviços
docker compose down

# Parar e remover volumes (apaga os dados do MongoDB)
docker compose down -v

# Rebuild da imagem da API
docker compose build api
```

---

## Variáveis de ambiente

| Variável | Padrão | Descrição |
|----------|--------|-----------|
| `ASPNETCORE_ENVIRONMENT` | `Production` | Ambiente (`Development`, `Production`) |
| `MongoDB__ConnectionString` | — | Connection string completa do MongoDB |
| `MongoDB__DatabaseName` | `AtsDb` | Nome do banco de dados |
| `MongoDB__MaxConnectionPoolSize` | `100` | Tamanho máximo do pool de conexões |
| `Cors__AllowedOrigins` | `""` | Origins permitidas (separadas por vírgula) |
| `Observability__ServiceName` | `ats-api` | Nome do serviço no OTLP/Jaeger |
| `Observability__OtlpEndpoint` | `""` | Endpoint OTLP (ex: `http://jaeger:4317`) |
| `Observability__EnablePrometheusEndpoint` | `true` | Habilita `/metrics` |
| `Observability__EnableConsoleExporter` | `false` | Exporta traces e métricas para console |
| `API_HTTP_PORT` | `5031` | Porta local exposta pelo Compose |
| `MONGO_ROOT_USERNAME` | — | **Obrigatório** — usuário root do MongoDB |
| `MONGO_ROOT_PASSWORD` | — | **Obrigatório** — senha root do MongoDB |

> **Segurança:** nunca comite o arquivo `.env` com credenciais reais. O `.gitignore` já exclui esse arquivo.

---

## Banco de dados

O projeto utiliza **MongoDB 7.0** como banco de dados principal. Por ser um banco de documentos, não há migrations no estilo relacional — a estrutura das coleções evolui junto com o código.

### Coleções

| Coleção | Agregado |
|---------|---------|
| `candidatos` | `Candidato` |
| `vagas` | `Vaga` |
| `candidaturas` | `Candidatura` |

### Mapeamento

O mapeamento entre classes C# e documentos BSON é feito via `BsonClassMap` registrado em `ATS.Infrastructure/Persistence/Mappings/`. Isso elimina a dependência de atributos de serialização nas entidades de domínio, mantendo o Domain Model limpo.

### Índices

Os índices são criados automaticamente pelo MongoDB conforme necessário. Para ambientes de produção com alto volume, considere criar índices explícitos (listados no [Roadmap](#roadmap)).

---

## Swagger / OpenAPI

O Swagger UI está disponível **apenas em ambiente Development**:

```
http://localhost:5031/swagger
```

A especificação OpenAPI em JSON está disponível em:

```
http://localhost:5031/swagger/v1/swagger.json
```

Os endpoints são documentados com `[ProducesResponseType]` e comentários XML gerados automaticamente.

---

## Endpoints da API

### Candidatos

```
POST   /api/v1/candidatos              → 201 Created
GET    /api/v1/candidatos              → 200 OK (paginado)
GET    /api/v1/candidatos/{id}         → 200 OK | 404 Not Found
PUT    /api/v1/candidatos/{id}         → 200 OK | 404 Not Found
DELETE /api/v1/candidatos/{id}         → 204 No Content | 404 Not Found
POST   /api/v1/candidatos/{id}/curriculo → 200 OK | 404 Not Found
```

### Vagas

```
POST   /api/v1/vagas                   → 201 Created
GET    /api/v1/vagas                   → 200 OK (paginado)
GET    /api/v1/vagas/{id}              → 200 OK | 404 Not Found
PUT    /api/v1/vagas/{id}              → 200 OK | 404 Not Found
DELETE /api/v1/vagas/{id}              → 204 No Content | 404 Not Found
POST   /api/v1/vagas/{id}/fechar       → 200 OK | 404 Not Found | 409 Conflict
```

### Candidaturas

```
POST   /api/v1/candidaturas                        → 201 Created | 409 Conflict
GET    /api/v1/candidaturas/{id}                   → 200 OK | 404 Not Found
GET    /api/v1/candidaturas/vaga/{vagaId}           → 200 OK (paginado)
POST   /api/v1/candidaturas/{id}/aprovar            → 200 OK | 404 | 409 Conflict
POST   /api/v1/candidaturas/{id}/reprovar           → 200 OK | 404 | 409 Conflict
POST   /api/v1/candidaturas/{id}/cancelar           → 200 OK | 404 | 409 Conflict
```

### Infraestrutura

```
GET    /health/live    → 200 OK | 503 Service Unavailable
GET    /health/ready   → 200 OK | 503 Service Unavailable
GET    /metrics        → texto Prometheus
```

---

## Testes

O projeto possui **645 testes** distribuídos em cinco projetos:

| Projeto | Tipo | Foco |
|---------|------|------|
| `ATS.Domain.Tests` | Unitário | Entidades, Value Objects, invariantes de domínio |
| `ATS.Application.Tests` | Unitário | Handlers com mocks estritos de repositórios |
| `ATS.Infrastructure.Tests` | Integração | Repositórios contra MongoDB |
| `ATS.API.Tests` | Unitário | Controllers, ExceptionHandlingMiddleware, ObservabilityExtensions |
| `ATS.E2E.Tests` | E2E | Fluxos completos com Testcontainers + MongoDB real |

### Executar todos os testes unitários

```bash
dotnet test --filter "FullyQualifiedName!~Integration&FullyQualifiedName!~E2E"
```

### Executar apenas os testes E2E

> Requer Docker em execução (Testcontainers sobe o MongoDB automaticamente).

```bash
dotnet test tests/ATS.E2E.Tests/ATS.E2E.Tests.csproj
```

### Executar a suite completa

```bash
dotnet test
```

---

## Cobertura de código

### Coletar cobertura

```bash
# Limpar resultados anteriores
rm -rf coverage-results coverage-report

# Executar testes com coleta de cobertura
dotnet test \
  --filter "FullyQualifiedName!~Integration&FullyQualifiedName!~E2E" \
  --collect:"XPlat Code Coverage" \
  --results-directory ./coverage-results \
  -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=cobertura \
     DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.ExcludeByAttribute=GeneratedCodeAttribute \
     DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.ExcludeByFile="**/Program.cs,**/Shared/**,**/I*.cs,**/*Command.cs,**/*Dto.cs,**/*Injection.cs,**/*Map.cs,**/*Request.cs"
```

### Gerar relatório HTML

```bash
# Instalar a ferramenta (uma única vez)
dotnet tool install --global dotnet-reportgenerator-globaltool

# Gerar o relatório
reportgenerator \
  -reports:"./coverage-results/**/coverage.cobertura.xml" \
  -targetdir:"./coverage-report" \
  -reporttypes:"Html;Badges;TextSummary"

# Abrir no macOS
open coverage-report/index.html
```

---

## Observabilidade

O projeto implementa os três pilares de observabilidade configurados em `ObservabilityExtensions`:

### Logs estruturados (Serilog)

- **Development**: saída de console com template legível por humanos (`[HH:mm:ss LVL] SourceContext Message`)
- **Production**: saída de console em **JSON compacto** (Compact JSON Formatter) — pronto para ingestão por Elastic, Loki, Datadog, etc.
- Enriquecimento automático: `EnvironmentName`, `MachineName`, `ThreadId`, contexto da requisição
- Logs de requisição via `UseSerilogRequestLogging` com nível `Verbose` para `/health` e `/metrics`

### Tracing distribuído (OpenTelemetry)

- Instrumentação automática de ASP.NET Core e `HttpClient`
- Filtro: ignora spans de `/health` e `/metrics`
- Exporta para OTLP quando `Observability__OtlpEndpoint` está configurado (compatível com Jaeger, Tempo, etc.)

### Métricas (OpenTelemetry + Prometheus)

| Métrica | Tipo | Descrição |
|---------|------|-----------|
| `ats.candidatos.criados` | Counter | Candidatos criados com sucesso |
| `ats.vagas.criadas` | Counter | Vagas publicadas |
| `ats.candidaturas.criadas` | Counter | Candidaturas realizadas |
| Métricas ASP.NET Core | — | Request rate, duration, active connections |
| Métricas .NET Runtime | — | GC, thread pool, alocações |

Endpoint de scraping: `GET /metrics`

### Health Checks

| Endpoint | Verifica | Uso típico |
|----------|----------|-----------|
| `/health/live` | Processo está vivo | Liveness probe (Kubernetes) |
| `/health/ready` | Conectividade com MongoDB | Readiness probe (Kubernetes) |

---

## Decisões técnicas

### Domain-Driven Design (DDD)

Cada agregado (`Candidato`, `Vaga`, `Candidatura`) é a unidade de consistência do domínio. Toda regra de negócio vive dentro das entidades — sem lógica de domínio vazando para handlers ou controllers. Value Objects (`Email`, `Telefone`, `Salario`) encapsulam validação e igualdade estrutural.

### CQRS-style (sem MediatR)

Commands e Queries são separados em pastas distintas dentro de `ATS.Application`. Cada handler é uma classe focada com injeção explícita de dependências via construtor. Essa abordagem foi escolhida por:
- Eliminar a indireção de pipeline genérico para um projeto com escopo bem definido
- Facilitar rastreamento de chamadas em IDEs (Go to Definition funciona diretamente)
- Manter o registro de handlers visível e explícito em `DependencyInjection.cs`

### Repository Pattern

Interfaces definidas no domínio (`IXRepository`), implementadas na infraestrutura. O domínio nunca referencia `MongoDB.Driver` diretamente — a inversão de dependência protege o núcleo do negócio de mudanças tecnológicas.

### Source-generated `[LoggerMessage]`

Todos os logs estruturados nos handlers usam o atributo `[LoggerMessage]` (geração de código em tempo de compilação). Isso elimina boxing de parâmetros, verifica `IsEnabled` antes de construir a string de log e garante performance zero-cost quando o nível de log está desabilitado.

### ExceptionHandlingMiddleware com ProblemDetails

Um único middleware centraliza todo tratamento de exceções e serializa respostas no formato **RFC 7807 (Problem Details)**. O `DomainException` é mapeado para códigos HTTP semanticamente corretos (404 para "não encontrado", 409 para conflito de estado). Detalhes internos de exceções inesperadas nunca vazam para o cliente.

### MongoDB sem ORM

O uso direto do `MongoDB.Driver` com mapeamento explícito via `BsonClassMap` permite:
- Controle total sobre nomes de campos e serialização
- Ausência de "magic" que dificulta debugging
- Manter as entidades de domínio livres de atributos de persistência

---

## Convenções de desenvolvimento

### Nomenclatura

| Elemento | Convenção | Exemplo |
|----------|-----------|---------|
| EventId dos logs | `1xxx` Candidatos, `2xxx` Vagas, `3xxx` Candidaturas, `9xxx` Middleware | `9001`, `1001` |
| Campos de log | Apenas IDs de entidade, nunca PII | `CandidatoId`, `VagaId` |
| Handlers | Sufixo `Handler`, método `HandleAsync` | `CreateCandidatoHandler.HandleAsync` |
| Commands/Queries | Record com propriedades imutáveis | `CreateCandidatoCommand(string Nome, ...)` |
| Testes | `Deve[Resultado]Quando[Condicao]` | `DeveLancarExcecaoQuandoCandidatoNaoExistir` |
| Branches | `feature/`, `fix/`, `chore/`, `docs/` | `feature/autenticacao-jwt` |

### Regras de domínio

- Entidades só se constroem via factory method estático (ex.: `Candidato.Criar(...)`)
- Setters são `private` — toda mutação passa por métodos com nome semântico
- Erros de regra de negócio são `DomainException`, nunca exceções genéricas

### Formatação

```bash
# Verificar formatação
dotnet format --verify-no-changes

# Aplicar formatação
dotnet format
```

Warnings são tratados como erros no build (`TreatWarningsAsErrors=true`).

---

## Tratamento de erros

O `ExceptionHandlingMiddleware` intercepta todas as exceções não tratadas e produz uma resposta `application/problem+json` conforme RFC 7807:

```json
{
  "status": 404,
  "title": "Candidatura não encontrada.",
  "detail": "Candidatura não encontrada.",
  "instance": "/api/v1/candidaturas/abc-123",
  "traceId": "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01"
}
```

| Exceção | Status HTTP | Título |
|---------|-------------|--------|
| `DomainException` (não encontrado) | 404 | Mensagem do domínio |
| `DomainException` (conflito) | 409 | Mensagem do domínio |
| `DomainException` (validação) | 400 | Mensagem do domínio |
| `BadHttpRequestException` | 400 | "Requisição inválida." |
| `ArgumentException` | 400 | "Requisição inválida." |
| `KeyNotFoundException` | 404 | "Recurso não encontrado." |
| Qualquer outra | 500 | "Erro interno no servidor." |

Exceções 5xx são registradas com `LogLevel.Error`; exceções de domínio com `LogLevel.Warning`.  
Detalhes internos **nunca** são expostos em respostas 5xx.

---

## CI/CD

O pipeline de CI roda no **GitHub Actions** (`.github/workflows/ci.yml`) com quatro jobs paralelos após o build:

```
┌─────────┐
│  Build  │
└────┬────┘
     │
     ├──────────────┬──────────────┬────────────────┐
     ▼              ▼              ▼                ▼
┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────────┐
│  Unit    │  │   E2E    │  │   Code   │  │  (futuro:    │
│  Tests   │  │  Tests   │  │ Quality  │  │   Deploy)    │
│ +Coverage│  │          │  │          │  └──────────────┘
└──────────┘  └──────────┘  └──────────┘
```

| Job | O que faz |
|-----|-----------|
| **Build** | Restaura, compila com `TreatWarningsAsErrors=true` |
| **Unit Tests** | Roda tests unitários, coleta cobertura Cobertura XML, valida threshold mínimo de 30%, publica relatório |
| **E2E Tests** | Roda testes E2E (Testcontainers + MongoDB real), publica relatório TRX |
| **Code Quality** | Verifica `dotnet format`, audita pacotes vulneráveis com `dotnet list package --vulnerable` |

Artefatos publicados por run: relatório HTML de cobertura (`coverage-report`), resultados TRX unitários (`test-results`), resultados TRX E2E (`e2e-test-results`).

---

## Documentação BDD

> **Localização:** [`docs/bdd/`](docs/bdd/README.md) — ~101 cenários em Gherkin, pt-BR, prontos para automação com Reqnroll ou SpecFlow.

O diretório [`docs/bdd/`](docs/bdd/) contém a **documentação viva das regras de negócio** no formato **Gherkin / BDD (Behavior-Driven Development)**, escrita em português do Brasil. Os cenários foram extraídos diretamente da implementação — entidades de domínio, handlers, value objects e repositórios — sem comportamentos inventados ou especulativos.

Serve simultaneamente como **especificação executável**, **documentação técnica** e **base de testes automatizados**, legível por desenvolvedores, QA, Product Owners e analistas de negócio.

### Arquivos de funcionalidade

| Arquivo | Contexto de negócio | Cenários |
|---------|--------------------|---------:|
| [Candidatos.feature](docs/bdd/Candidatos.feature) | Cadastro, consulta, atualização, exclusão de candidatos e upload de currículo | 28 |
| [Vagas.feature](docs/bdd/Vagas.feature) | Publicação, consulta, listagem com filtro, atualização e encerramento de vagas | 24 |
| [Candidaturas.feature](docs/bdd/Candidaturas.feature) | Candidatura, aprovação, reprovação, cancelamento e ciclo de vida do processo seletivo | 31 |
| [Paginacao.feature](docs/bdd/Paginacao.feature) | Regras de paginação aplicáveis a todas as listagens | 9 |
| [TratamentoDeErros.feature](docs/bdd/TratamentoDeErros.feature) | Respostas padronizadas de erro (Problem Details — RFC 7807) | 9 |

### Tags para execução seletiva

| Tag | Uso |
|-----|-----|
| `@Smoke` | Cenários essenciais — verificação rápida de sanidade |
| `@Critical` | Regras críticas — devem passar antes de qualquer release |
| `@Candidatos`, `@Vagas`, `@Candidaturas` | Execução por agregado de domínio |
| `@CicloDeVida` | Transições de estado dos agregados |

```bash
# Executar apenas smoke tests (com Reqnroll/SpecFlow)
dotnet test --filter "Category=Smoke"

# Executar cenários críticos
dotnet test --filter "Category=Critical"

# Executar um agregado específico
dotnet test --filter "Category=Candidaturas"
```

Consulte o [docs/bdd/README.md](docs/bdd/README.md) para convenções de escrita, instruções de automação com Reqnroll e notas sobre comportamentos implícitos identificados no código.

---

## Roadmap

Melhorias planejadas para versões futuras:

- [ ] **Autenticação e autorização** — JWT Bearer com roles (Recrutador, Admin)
- [ ] **Criação explícita de índices MongoDB** — índice em `Email.Value` para unicidade de candidatos, índice composto em `CandidatoId + VagaId` para candidaturas
- [ ] **Paginação com cursor** — substituir paginação por offset por paginação baseada em cursor para melhor performance em grandes volumes
- [ ] **Publicação de eventos de domínio** — integração com mensageria (RabbitMQ / Azure Service Bus) para notificações assíncronas de mudança de status
- [ ] **FluentValidation** — validação declarativa de Commands com mensagens de erro detalhadas
- [ ] **Rate limiting** — throttling por IP/usuário nas rotas públicas
- [ ] **Audit log** — registro imutável de todas as transições de estado das candidaturas
- [ ] **Dashboard Grafana** — dashboards pré-configurados para as métricas `ats.*`
- [ ] **Multi-tenancy** — isolamento de dados por empresa/cliente

---

## Licença

Distribuído sob a licença **MIT**. Consulte o arquivo [LICENSE](LICENSE) para mais informações.

---

## Autor

**Marlon Reis**  
[marlongreis91@gmail.com](mailto:marlongreis91@gmail.com)  
[github.com/marlongreis91](https://github.com/marlongreis91)
