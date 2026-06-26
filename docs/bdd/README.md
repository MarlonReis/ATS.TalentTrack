# BDD — Documentação de Regras de Negócio (Gherkin)

Este diretório contém a documentação viva das regras de negócio do **ATS (Applicant Tracking System)** no formato **Gherkin / BDD (Behavior-Driven Development)**, escrita em português do Brasil (pt-BR).

A documentação foi extraída diretamente da implementação do código-fonte — entidades de domínio, handlers, value objects e repositórios — e não contém comportamentos inventados ou especulativos.

---

## Índice de funcionalidades

| Arquivo | Contexto de negócio | Cenários |
|---------|--------------------|---------:|
| [Candidatos.feature](Candidatos.feature) | Cadastro e gestão de candidatos | 28 |
| [Vagas.feature](Vagas.feature) | Publicação e gestão de vagas | 24 |
| [Candidaturas.feature](Candidaturas.feature) | Candidaturas e ciclo de vida do processo seletivo | 31 |
| [Paginacao.feature](Paginacao.feature) | Regras de paginação de listagens | 9 |
| [TratamentoDeErros.feature](TratamentoDeErros.feature) | Tratamento padronizado de erros | 9 |

**Total: ~101 cenários documentados**

---

## Descrição dos contextos de negócio

### Candidatos.feature

Cobre todas as operações relacionadas à gestão da base de candidatos:

- **Cadastrar candidato** — campos obrigatórios (nome, e-mail, telefone), unicidade de e-mail, limites de tamanho, normalização de dados (e-mail em minúsculas, remoção de espaços)
- **Consultar candidato** — por identificador único
- **Listar candidatos** — paginação com parâmetros validados
- **Atualizar candidato** — proteção contra e-mail duplicado com outro candidato, manutenção do mesmo e-mail
- **Excluir candidato** — verificação de existência antes da exclusão
- **Adicionar currículo** — formatos aceitos (PDF, DOC, DOCX), substituição do currículo anterior, validações de nome e conteúdo

### Vagas.feature

Cobre o ciclo de vida completo de vagas no processo seletivo:

- **Publicar vaga** — título e descrição obrigatórios, requisitos opcionais, salário ≥ 0, status inicial "Aberta", moeda padrão BRL
- **Consultar vaga** — por identificador único
- **Listar vagas** — paginação e filtro por status (Aberta / Fechada)
- **Atualizar vaga** — proteção contra edição de vaga fechada
- **Fechar vaga** — encerramento com registro de data, proteção contra fechamento duplo
- **Excluir vaga** — verificação de existência antes da exclusão

> **Nota de implementação:** O domínio da Vaga define o método `Reabrir()`, mas não existe um caso de uso (handler) que o exponha. A reabertura de vagas é uma funcionalidade parcialmente implementada no domínio, mas não está disponível operacionalmente na versão atual.

### Candidaturas.feature

Cobre todo o processo seletivo de candidaturas:

- **Realizar candidatura** — verificação de candidato e vaga existentes, vaga deve estar aberta, candidatura duplicada bloqueada por regra de negócio e por índice único de persistência
- **Consultar candidatura** — por identificador único
- **Listar candidatos por vaga** — exibição dos dados detalhados do candidato com indicação de currículo disponível
- **Aprovar candidatura** — somente quando "Em Análise", observações opcionais
- **Reprovar candidatura** — somente quando "Em Análise", observações opcionais
- **Cancelar candidatura** — permitido em qualquer status exceto "Cancelado"
- **Ciclo de vida** — tabelas de transição de estado completas

### Paginacao.feature

Documenta as regras de paginação aplicadas às listagens de candidatos e vagas:

- Página mínima: 1 (zero ou negativo é rejeitado)
- Tamanho por página: entre 1 e 100 (zero, negativo ou superior a 100 é rejeitado)
- Padrão: página 1, tamanho 20
- Campos de metadados: `Total`, `TotalPaginas`, `TemProxima`, `TemAnterior`

### TratamentoDeErros.feature

Documenta o comportamento do middleware de tratamento de erros centralizado:

- **Erros de domínio** — mapeamento de regra de negócio para status HTTP semântico (404, 409, 400)
- **Erros internos** — ocultação de detalhes técnicos; mensagem genérica ao usuário
- **Resposta iniciada** — não sobrescrever resposta HTTP em andamento
- **Formato Problem Details** — RFC 7807, campos obrigatórios e rastreamento via `traceId`

---

## Organização dos arquivos

```
docs/bdd/
├── README.md                  ← este arquivo
├── Candidatos.feature         ← agregado Candidato
├── Vagas.feature              ← agregado Vaga
├── Candidaturas.feature       ← agregado Candidatura + processo seletivo
├── Paginacao.feature          ← regra transversal de paginação
└── TratamentoDeErros.feature  ← comportamento do middleware de erros
```

A separação por arquivo segue os agregados do domínio DDD do projeto. Funcionalidades transversais (paginação, tratamento de erros) receberam arquivos próprios por terem regras independentes aplicáveis a múltiplos contextos.

---

## Convenções adotadas

### Escrita de cenários

| Elemento | Convenção adotada |
|----------|-------------------|
| Idioma | `# language: pt` em todos os arquivos |
| Nomenclatura de funcionalidade | Verbo + substantivo no infinitivo: "Gestão de candidatos" |
| Nomenclatura de cenário | Verbo no infinitivo descrevendo o comportamento: "Cadastrar candidato com dados válidos" |
| Linguagem | Negócio — sem referências a classes, métodos, HTTP, JSON ou banco de dados |
| Dados de exemplo | Nomes e e-mails fictícios em português |
| Esquemas com múltiplos exemplos | `Esquema do Cenário` + `Exemplos` para evitar duplicação |

### Tags utilizadas

| Tag | Significado |
|-----|-------------|
| `@Candidatos` | Cenários do agregado Candidato |
| `@Vagas` | Cenários do agregado Vaga |
| `@Candidaturas` | Cenários do agregado Candidatura |
| `@Paginacao` | Regras de paginação |
| `@TratamentoDeErros` | Comportamento de erros |
| `@Smoke` | Cenários essenciais — executados em verificações rápidas de sanidade |
| `@Critical` | Regras críticas de negócio — devem sempre passar antes de qualquer release |
| `@CadastrarCandidato`, `@AtualizarVaga`, etc. | Tag da operação específica para execução seletiva |
| `@CicloDeVida` | Cenários de transição de estado do agregado |
| `@Integridade` | Regras de integridade de dados com garantia no armazenamento |

### Estrutura dos passos (steps)

```gherkin
Dado    → estado inicial do sistema (pré-condições)
Quando  → ação executada pelo ator (recrutador / sistema)
Então   → resultado esperado (verificações)
E       → continuação do passo anterior (Dado, Quando ou Então)
Mas     → exceção ou alternativa ao passo anterior
```

### Identificação de comportamentos documentados

Cada cenário foi derivado de evidências concretas no código-fonte:

- Lançamentos de `DomainException` em entidades e handlers → cenários de rejeição
- Retornos de dados → cenários de sucesso
- Verificações de existência (`?? throw`) → cenários de "não encontrado"
- Índices únicos no repositório → cenários de integridade
- Condicionais de estado (`if (Status != ...)`) → cenários de transição de estado

---

## Notas sobre comportamentos implícitos ou incompletos

Os comentários nos arquivos `.feature` registram inconsistências e comportamentos parcialmente implementados:

1. **`Vaga.Reabrir()`** — método existe no domínio em `ATS.Domain/Vagas/Entities/Vaga.cs` mas não há handler exposto. O estado `Rascunho` definido em `StatusVaga` também é inacessível via caso de uso (vagas são sempre criadas como `Aberta`). Registrado em `Vagas.feature`.

2. **`CandidaturaRepository.ListarPorCandidatoAsync()`** — método implementado no repositório mas sem handler que o invoque. Não foi documentado como funcionalidade disponível.

3. **Exclusão silenciosa de candidatos removidos** — o `ListCandidatosPorVagaHandler` exclui silenciosamente da listagem candidaturas cujo candidato não existe mais no sistema (sem lançar erro). Documentado em `Candidaturas.feature` como comportamento observado.

4. **Candidatura duplicada** — a regra de unicidade é verificada duas vezes: pela regra de negócio no handler (com mensagem amigável) e pelo índice composto único no MongoDB (como garantia de integridade). Documentado em `Candidaturas.feature`.

---

## Utilização com ferramentas de automação

### Reqnroll (sucessor do SpecFlow para .NET 9+/10)

```bash
# Instalar o pacote no projeto de testes
dotnet add package Reqnroll.xUnit
dotnet add package Reqnroll.Microsoft.Extensions.DependencyInjection

# Os arquivos .feature devem estar em um projeto de teste separado
# ou referenciados via "Additional Files" no .csproj
```

**Estrutura sugerida para automação:**

```
tests/
└── ATS.BDD.Tests/
    ├── ATS.BDD.Tests.csproj
    ├── Features/
    │   ├── Candidatos.feature      ← cópia ou link para docs/bdd/
    │   ├── Vagas.feature
    │   ├── Candidaturas.feature
    │   ├── Paginacao.feature
    │   └── TratamentoDeErros.feature
    └── StepDefinitions/
        ├── CandidatosSteps.cs
        ├── VagasSteps.cs
        ├── CandidaturasSteps.cs
        ├── PaginacaoSteps.cs
        └── TratamentoDeErrosSteps.cs
```

### Execução por tags com Reqnroll/SpecFlow

```bash
# Executar apenas cenários críticos
dotnet test --filter "Category=Critical"

# Executar smoke tests
dotnet test --filter "Category=Smoke"

# Executar cenários de um agregado específico
dotnet test --filter "Category=Candidatos"

# Executar cenários de candidatura e vagas
dotnet test --filter "Category=Candidaturas|Category=Vagas"
```

### Configuração mínima do `.csproj` para Reqnroll

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Reqnroll.xUnit" Version="2.*" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.*" />
    <PackageReference Include="xunit" Version="2.*" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.*" />
  </ItemGroup>

  <ItemGroup>
    <!-- Incluir os feature files como Additional Files para que o gerador de código funcione -->
    <None Update="Features\*.feature">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
  </ItemGroup>
</Project>
```

---

## Manutenção desta documentação

Esta documentação é **viva** e deve evoluir junto com o código-fonte:

- Ao adicionar uma nova regra de negócio → adicionar o cenário correspondente no `.feature` do agregado
- Ao modificar uma regra existente → atualizar o cenário correspondente
- Ao remover uma funcionalidade → remover ou comentar o cenário com explicação
- Ao implementar um handler para um método de domínio existente (ex.: `Vaga.Reabrir()`) → remover a nota de implementação e adicionar os cenários correspondentes

**Responsáveis pela documentação:** equipe de desenvolvimento + Product Owner  
**Revisão recomendada:** a cada sprint ou milestone

---

## Referências

- [Gherkin Reference — Cucumber](https://cucumber.io/docs/gherkin/reference/)
- [Reqnroll — BDD para .NET](https://reqnroll.net/)
- [BDD in Action — John Ferguson Smart](https://www.manning.com/books/bdd-in-action-second-edition)
- [RFC 7807 — Problem Details for HTTP APIs](https://datatracker.ietf.org/doc/html/rfc7807)
