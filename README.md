# 🎵 TuneTrail API 👨‍💻

> [!NOTE]
> Uma **Minimal API em .NET 9** que funciona como diário pessoal de músicas, construída durante um tech shot para mostrar, na prática, como organizar uma Minimal API em camadas sem cair no arquivo único gigante.

<div align="justify">
  A <b>TuneTrail API</b> é uma API REST de registro pessoal de escuta musical: você cadastra as músicas que ouve, marca o gênero, o status de escuta, a sua nota e quantas vezes tocou. Por trás desse domínio simples, o projeto demonstra uma <i>arquitetura completa</i> de Minimal API com <b>.NET 9</b>: rotas organizadas em <i>modules</i> registrados automaticamente por reflection, regras de negócio isoladas em <i>aggregates</i>, <b>Result Pattern</b> no lugar de exceptions para fluxo de erro, validação de schema separada da regra de negócio, persistência com <b>Entity Framework Core</b> e <b>PostgreSQL</b> em container Docker, <i>soft delete</i> com auditoria automática e documentação viva no <b>Swagger</b>. O repositório acompanha um <a href="./TUTORIAL.md">tutorial completo</a> e os <a href="./presentation/minimal-apis-techshot.pdf">slides da apresentação</a>, permitindo reconstruir o projeto do zero, passo a passo.
</div>

---

## 🚧 Status do Projeto

![Status](https://img.shields.io/badge/Status-Concluído-007ec6?style=for-the-badge&logo=checkmarx&logoColor=white) ![.NET](https://img.shields.io/badge/.NET-9.0-007ec6?style=for-the-badge&logo=dotnet&logoColor=white) ![C#](https://img.shields.io/badge/C%23-13-007ec6?style=for-the-badge&logo=csharp&logoColor=white) ![Entity Framework Core](https://img.shields.io/badge/EF_Core-9.0.19-007ec6?style=for-the-badge&logo=dotnet&logoColor=white) ![PostgreSQL](https://img.shields.io/badge/PostgreSQL-17-007ec6?style=for-the-badge&logo=postgresql&logoColor=white) ![Docker](https://img.shields.io/badge/Docker_Compose-007ec6?style=for-the-badge&logo=docker&logoColor=white) ![Swagger](https://img.shields.io/badge/Swagger-OpenAPI-007ec6?style=for-the-badge&logo=swagger&logoColor=white) ![Licença](https://img.shields.io/badge/Licença-MIT-007ec6?style=for-the-badge&logo=opensourceinitiative&logoColor=white)

![GitHub repo size](https://img.shields.io/github/repo-size/arturbomtempo-dev/minimal-api-techshot?style=for-the-badge&logo=files) ![GitHub language count](https://img.shields.io/github/languages/count/arturbomtempo-dev/minimal-api-techshot?style=for-the-badge&logo=dotnet) ![GitHub last commit](https://img.shields.io/github/last-commit/arturbomtempo-dev/minimal-api-techshot?style=for-the-badge&logo=clockify) ![GitHub commit activity](https://img.shields.io/github/commit-activity/m/arturbomtempo-dev/minimal-api-techshot?style=for-the-badge&color=007ec6&logo=gitkraken) ![GitHub stars](https://img.shields.io/github/stars/arturbomtempo-dev/minimal-api-techshot?style=for-the-badge&logo=github) ![GitHub forks](https://img.shields.io/github/forks/arturbomtempo-dev/minimal-api-techshot?style=for-the-badge&logo=git)

---

## 📚 Índice

- [TuneTrail API](#-tunetrail-api-)
  - [Status do Projeto](#-status-do-projeto)
  - [Índice](#-índice)
  - [Links Úteis](#-links-úteis)
  - [Sobre o Projeto](#-sobre-o-projeto)
  - [Funcionalidades Principais](#-funcionalidades-principais)
  - [Tecnologias Utilizadas](#-tecnologias-utilizadas)
    - [Back-end](#️-back-end)
    - [Banco de Dados](#️-banco-de-dados)
    - [Infraestrutura e Ferramentas](#️-infraestrutura-e-ferramentas)
  - [Arquitetura](#-arquitetura)
    - [Fluxo de uma requisição](#fluxo-de-uma-requisição)
    - [Decisões arquiteturais](#decisões-arquiteturais)
  - [Instalação e Execução](#-instalação-e-execução)
    - [Pré-requisitos](#pré-requisitos)
    - [Variáveis de Ambiente](#-variáveis-de-ambiente)
    - [Clonando o Repositório](#-clonando-o-repositório)
    - [Subindo o Banco de Dados](#-subindo-o-banco-de-dados)
    - [Aplicando as Migrations](#️-aplicando-as-migrations)
    - [Executando a Aplicação](#-executando-a-aplicação)
  - [Endpoints da API](#-endpoints-da-api)
    - [Catálogo de erros](#catálogo-de-erros)
  - [Estrutura de Pastas](#-estrutura-de-pastas)
  - [Demonstração](#-demonstração)
  - [Slides e Tutorial](#-slides-e-tutorial)
  - [Documentações utilizadas](#-documentações-utilizadas)
  - [Autor](#-autor)
  - [Contribuição](#-contribuição)
  - [Agradecimentos](#-agradecimentos)
  - [Licença](#-licença)

---

## 🔗 Links Úteis

- 📖 **Tutorial completo:** [TUTORIAL.md](./TUTORIAL.md)
  > Guia autocontido que reconstrói a API do zero, do `dotnet new` até o soft delete conferido no `psql`.
- 🎤 **Slides da apresentação:** [minimal-apis-techshot.pdf](./presentation/minimal-apis-techshot.pdf)
  > Material usado no tech shot sobre Minimal APIs em .NET.
- 🧪 **Documentação interativa da API:** `https://localhost:7214/swagger` ou `http://localhost:5294/swagger`
  > Swagger UI gerado pelo Swashbuckle, disponível apenas no ambiente de desenvolvimento, com a aplicação rodando localmente.
- 💻 **Repositório:** [github.com/arturbomtempo-dev/minimal-api-techshot](https://github.com/arturbomtempo-dev/minimal-api-techshot)

---

## 📝 Sobre o Projeto

A **TuneTrail API** nasceu como projeto de apoio a um tech shot sobre **Minimal APIs no .NET**. A motivação foi resolver uma queixa comum de quem experimenta Minimal APIs pela primeira vez: os exemplos oficiais concentram tudo no `Program.cs`, o que funciona bem para um "Hello World" e desmorona assim que o projeto ganha alguns endpoints, validações e acesso a banco.

O problema que o projeto endereça é, portanto, **mostrar que Minimal API não é sinônimo de código bagunçado**. Ele parte de um domínio propositalmente simples, o registro pessoal de músicas ouvidas, para que toda a atenção fique na organização do código, e não nas regras de negócio.

O contexto é **educacional e demonstrativo**. Cada decisão do repositório existe para ser explicada:

- **Por que separar Module e Aggregate:** o Module nunca fala com o banco, e o Aggregate nunca sabe o que é HTTP.
- **Por que Result Pattern:** erro de negócio previsto não é exception, e o handler HTTP decide o status code olhando o código do erro.
- **Por que registrar modules por reflection:** adicionar um novo recurso não exige tocar no `Program.cs`.
- **Por que validação separada:** o que dá para checar olhando só o campo mora no Validator; o que precisa consultar o banco mora no Aggregate.

O projeto pode ser usado como **ponto de partida para novas APIs em .NET**, como **material de estudo** para quem está aprendendo Minimal APIs e EF Core, ou como **referência de estrutura** para quem já trabalha com Controllers e quer entender o equivalente no mundo minimal.

---

## ✨ Funcionalidades Principais

- 🎧 **CRUD completo de músicas:** criação, consulta individual, listagem, atualização e remoção de registros do diário de escuta.
- 🔎 **Listagem com filtros:** busca por trecho do título e do artista via query string, combináveis entre si.
- 🏷️ **Domínio tipado por enums:** gênero musical (`Rock`, `Pop`, `HipHop`, `Electronic`, `Jazz`, `Classical`, `Mpb`, `Metal`, `Other`) e status de escuta (`WantToListen`, `Listening`, `Favorite`, `Archived`), serializados como texto no JSON.
- ✅ **Validação em duas camadas:** limites de campo no Validator, e regras que dependem do banco, como música duplicada, no Aggregate.
- 🧾 **Result Pattern:** sucesso e falha trafegam como retorno, com um catálogo de erros codificados que o Module traduz em `200`, `201`, `204`, `400` ou `404`.
- 🗑️ **Soft delete:** o `DELETE` marca o registro como removido em vez de apagar a linha, e todas as consultas ignoram os registros deletados.
- 🕒 **Auditoria automática:** `CreatedAt` e `UpdatedAt` são preenchidos pelo `DbContext` no `SaveChanges`, sem código repetido nos handlers.
- 🧩 **Registro automático de rotas:** qualquer classe que implemente `IRegisterModule` é descoberta por reflection e tem suas rotas registradas no startup.
- 📚 **Documentação viva:** Swagger UI alimentado pelos comentários XML das entidades, schemas e endpoints.
- 🐳 **Ambiente reprodutível:** PostgreSQL 17 sobe via Docker Compose, com healthcheck e volume nomeado.

---

## 🛠 Tecnologias Utilizadas

As versões abaixo são as efetivamente usadas no repositório. O SDK fica travado pelo `global.json`, com `rollForward` para a última feature band.

### 🖥️ Back-end

- **Linguagem:** C# 13
- **SDK:** .NET 10 (`10.0.400`), travado pelo `global.json` com `rollForward: latestFeature`
- **Target framework:** `net9.0`, o que exige o runtime do .NET 9 para executar
- **Estilo de API:** ASP.NET Core Minimal APIs, com `MapGroup` e `TypedResults`
- **Documentação:** Swashbuckle.AspNetCore `9.0.6`, com comentários XML habilitados
- **Serialização:** `System.Text.Json` com `JsonStringEnumConverter`
- **Recursos de projeto:** `Nullable` e `ImplicitUsings` habilitados via `Directory.Build.props`

### 🗄️ Banco de Dados

- **SGBD:** PostgreSQL 17
- **ORM:** Entity Framework Core `9.0.19`
- **Provider:** Npgsql.EntityFrameworkCore.PostgreSQL `9.0.4`, com `EnableRetryOnFailure`
- **Mapeamento:** Fluent API via `IEntityTypeConfiguration`
- **Versionamento de schema:** Migrations do EF Core

### ⚙️ Infraestrutura e Ferramentas

- **Containerização:** Docker e Docker Compose
- **Solution:** formato `.slnx`
- **CLI:** `dotnet` e `dotnet-ef`

---

## 🏗 Arquitetura

O projeto adota um **monólito modular em camadas**, com uma fronteira explícita entre o que é HTTP e o que é regra de negócio. A frase que resume a arquitetura inteira:

> O **Module** nunca fala com o banco, e o **Aggregate** nunca sabe o que é HTTP.

### Fluxo de uma requisição

```
HTTP Request
     |
     v
+--------------+
|    Module    |  <- define as rotas (MapGet/MapPost/MapPut/MapDelete)
|  (handlers)  |  <- recebe a requisição, chama o Aggregate, traduz Result em HTTP
+------+-------+
       |  (via Contract/IMusicAggregate, por injeção de dependência)
       v
+--------------+
|  Aggregate   |  <- validações e regras de negócio
+------+-------+
       |
       v
+--------------+
|  DbContext   |  <- acesso ao banco via EF Core
+------+-------+
       |
       v
   PostgreSQL
```

**Papel de cada componente:**

| Componente   | Responsabilidade                                                                  |
| :----------- | :-------------------------------------------------------------------------------- |
| `Module/`    | Declara as rotas do grupo `/musics` e converte `ResultSchema` em status code HTTP  |
| `Contract/`  | Interfaces `IRegisterModule` e `IMusicAggregate`, o contrato entre as camadas      |
| `Aggregate/` | Regras de negócio, orquestração das consultas e tratamento de erro                 |
| `Schemas/`   | Requests, responses, mapeamentos, validadores e o Result Pattern                   |
| `Data/`      | Entidades, configurações de Fluent API e migrations                                |
| `IoC/`       | `DbContext`, configuração de banco e extensões de registro de serviços e modules   |
| `Shared/`    | Constantes, limites de campo e enums usados por todas as camadas                   |

### Decisões arquiteturais

- **Result Pattern no lugar de exceptions:** `ResultSchema` e `ResultSchema<T>` carregam sucesso ou falha. Exceptions ficam reservadas para o que é realmente excepcional, e são logadas antes de virarem um erro genérico.
- **Catálogo de erros codificado:** cada erro tem um código no padrão `E + operação + módulo + id`, por exemplo `E300101` para "Get / Music / primeiro erro". É o código, e não o texto da mensagem, que o Module inspeciona para decidir entre `400` e `404`.
- **Registro de modules por reflection:** `MinimalExtensions.RegisterModules` varre o assembly em busca de implementações de `IRegisterModule` e as registra. Um recurso novo não exige alterar o `Program.cs`.
- **Validação em dois níveis:** `MusicRequestValidator` cuida de obrigatoriedade e limites; a checagem de música duplicada mora no Aggregate, porque depende de uma consulta ao banco.
- **Soft delete com `BaseEntity`:** `CreatedAt`, `UpdatedAt` e `Deleted` são herdados por todas as entidades e preenchidos centralmente no `DbContext`.
- **`src/` com solution própria:** separa código de produção da infraestrutura da raiz e deixa espaço natural para uma pasta `tests/` espelhando `src/`.

**Trade-offs assumidos:** não há camada de repositório, o Aggregate acessa o `DbContext` diretamente. Para um projeto desse porte, uma indireção a mais custaria mais do que entregaria. A listagem também não é paginada, algo que um cenário real exigiria.

---

## 🔧 Instalação e Execução

### Pré-requisitos

| Ferramenta             | Como verificar                   |
| :--------------------- | :------------------------------- |
| SDK do .NET 10         | `dotnet --list-sdks`             |
| Runtime do .NET 9      | `dotnet --list-runtimes`         |
| Docker Desktop rodando | `docker --version` e `docker ps` |
| CLI do EF Core         | `dotnet ef --version`            |

O `global.json` trava o **SDK do .NET 10** para compilar, mas o projeto tem como alvo o `net9.0`, então o **runtime do .NET 9** também precisa estar instalado para a aplicação rodar.

Caso o `dotnet ef` não esteja instalado:

```bash
dotnet tool install --global dotnet-ef
```

Se já estiver instalado, mas desatualizado:

```bash
dotnet tool update --global dotnet-ef
```

---

### 🔑 Variáveis de Ambiente

O projeto funciona sem nenhuma configuração adicional: a connection string de desenvolvimento já vem no `appsettings.json` e aponta para o PostgreSQL do `docker-compose.yml`. As variáveis abaixo existem para sobrescrever esses valores em outros ambientes.

| Variável                               | Descrição                                 | Valor padrão do repositório                                                    |
| :------------------------------------- | :---------------------------------------- | :----------------------------------------------------------------------------- |
| `ConnectionStrings__DefaultConnection`  | String de conexão com o PostgreSQL        | `Host=localhost;Port=5432;Database=tunetrail-db;Username=admin;Password=admin`  |
| `ASPNETCORE_ENVIRONMENT`                | Ambiente de execução, habilita o Swagger  | `Development`                                                                   |
| `ASPNETCORE_URLS`                       | Endereços de escuta da aplicação          | definido pelos perfis do `launchSettings.json`                                  |

Credenciais do container PostgreSQL, definidas no `docker-compose.yml`:

| Variável            | Valor          |
| :------------------ | :------------- |
| `POSTGRES_USER`     | `admin`        |
| `POSTGRES_PASSWORD` | `admin`        |
| `POSTGRES_DB`       | `tunetrail-db` |

> [!WARNING]
> As credenciais acima são de desenvolvimento local e estão versionadas de propósito, para que o repositório rode sem configuração. Em qualquer ambiente real, substitua-as por variáveis de ambiente e nunca versione a connection string de produção.

---

### 📦 Clonando o Repositório

```bash
git clone https://github.com/arturbomtempo-dev/minimal-api-techshot.git
cd minimal-api-techshot/TuneTrail
```

Todos os comandos das próximas seções devem ser executados a partir da pasta `TuneTrail/`, que é a raiz da solution.

Para restaurar as dependências e compilar:

```bash
dotnet restore
dotnet build
```

---

### 💾 Subindo o Banco de Dados

O PostgreSQL sobe via Docker Compose, com healthcheck e volume nomeado para preservar os dados entre reinícios. Certifique-se de que o Docker está em execução.

- **No Mac/Windows:** abra o **Docker Desktop**.
- **No Linux:** inicie o serviço com `sudo systemctl start docker`.

```bash
docker compose up -d
```

Verifique se o container está no ar:

```bash
docker ps
```

Você deve ver o container `tunetrail-db` publicando a porta `5432`.

---

### 🗃️ Aplicando as Migrations

Com o banco no ar, crie o schema aplicando as migrations do EF Core:

```bash
dotnet ef database update --project src/TuneTrail.Api --startup-project src/TuneTrail.Api
```

---

### ⚡ Executando a Aplicação

```bash
dotnet run --project src/TuneTrail.Api --launch-profile https
```

Mantenha esse terminal aberto: é nele que aparecem os logs da aplicação e o SQL gerado pelo EF Core, graças ao nível de log configurado no `appsettings.Development.json`.

Endereços disponíveis conforme o perfil escolhido:

| Perfil  | Endereço                 | Swagger                          |
| :------ | :----------------------- | :------------------------------- |
| `https` | `https://localhost:7214` | `https://localhost:7214/swagger` |
| `http`  | `http://localhost:5294`  | `http://localhost:5294/swagger`  |

> [!NOTE]
> O `dotnet run` sem o parâmetro `--launch-profile` usa o perfil `http`. As portas podem ser conferidas em `src/TuneTrail.Api/Properties/launchSettings.json` ou na própria saída do console.

Para derrubar o ambiente ao final:

```bash
docker compose down
```

Use `docker compose down -v` caso queira remover também o volume com os dados do banco.

---

## 🌐 Endpoints da API

Todos os endpoints ficam sob o grupo `/musics` e aparecem no Swagger sob a tag **Musics**.

| Método   | Rota           | Descrição                                                  | Respostas           |
| :------- | :------------- | :--------------------------------------------------------- | :------------------ |
| `GET`    | `/musics`      | Lista as músicas, com filtros opcionais `title` e `artist`  | `200`, `400`        |
| `GET`    | `/musics/{id}` | Busca uma música pelo identificador                         | `200`, `400`, `404` |
| `POST`   | `/musics`      | Cria um novo registro no diário                             | `201`, `400`        |
| `PUT`    | `/musics/{id}` | Atualiza um registro existente                              | `200`, `400`, `404` |
| `DELETE` | `/musics/{id}` | Remove o registro por soft delete                           | `204`, `400`, `404` |

**Corpo da requisição, para `POST` e `PUT`:**

```json
{
  "title": "Bohemian Rhapsody",
  "artist": "Queen",
  "genre": "Rock",
  "status": "Favorite",
  "personalRating": 10,
  "playCount": 42
}
```

| Campo            | Tipo              | Obrigatório | Regra                                       |
| :--------------- | :---------------- | :---------- | :------------------------------------------ |
| `title`          | `string`          | Sim         | Até 200 caracteres                          |
| `artist`         | `string`          | Sim         | Até 100 caracteres                          |
| `genre`          | `MusicGenre`      | Sim         | Um dos valores do enum de gênero            |
| `status`         | `ListeningStatus` | Sim         | Um dos valores do enum de status de escuta  |
| `personalRating` | `int?`            | Não         | Entre 0 e 10, quando informado              |
| `playCount`      | `int`             | Sim         | Não pode ser negativo                       |

### Catálogo de erros

O corpo de erro segue sempre o mesmo formato, com `code` e `message`:

```json
{
  "code": "E300101",
  "message": "Music not found."
}
```

| Código    | Significado                                          | Status HTTP |
| :-------- | :--------------------------------------------------- | :---------- |
| `E100099` | Campo obrigatório não informado                      | `400`       |
| `E100098` | Campo com valor inválido                             | `400`       |
| `E100101` | Já existe uma música com esse título para o artista  | `400`       |
| `E300101` | Música não encontrada                                | `404`       |
| `E300102` | Erro ao listar as músicas                            | `400`       |
| `E100102` | Erro ao criar a música                               | `400`       |
| `E200101` | Erro ao atualizar a música                           | `400`       |
| `E400101` | Erro ao remover a música                             | `400`       |
| `E000000` | Erro inesperado                                      | `400`       |

---

## 📂 Estrutura de Pastas

```
minimal-api-techshot/                        # 📁 Raiz do repositório Git
├── .gitignore                               # 🧹 Gerado por "dotnet new gitignore"
├── LICENSE.md                               # ⚖️ Licença MIT do projeto
├── README.md                                # 📘 Documentação principal
├── TUTORIAL.md                              # 📖 Tutorial completo, do zero à API rodando
├── presentation/                            # 🎤 Slides da apresentação
│   └── minimal-apis-techshot.pdf
│
└── TuneTrail/                               # 📁 Raiz da solution
    ├── TuneTrail.slnx                       # 🧩 Solution no formato novo (XML)
    ├── global.json                          # 📌 Trava a versão do SDK do .NET
    ├── Directory.Build.props                # ⚙️ Propriedades comuns a todos os projetos
    ├── docker-compose.yml                   # 🐳 Infraestrutura local (PostgreSQL 17)
    │
    └── src/
        └── TuneTrail.Api/                   # 📁 Projeto da API
            ├── TuneTrail.Api.csproj         # 🛠️ Dependências e target framework
            ├── Program.cs                   # 🚀 Composition root e pipeline HTTP
            ├── appsettings.json             # ⚙️ Configuração principal
            ├── appsettings.Development.json # 🧪 Configuração de desenvolvimento
            ├── Properties/
            │   └── launchSettings.json      # 🔌 Portas locais de execução
            │
            ├── Shared/                      # 📦 Código compartilhado entre camadas
            │   ├── Constants.cs             # 📏 Limites de campo e mensagens
            │   └── Enums.cs                 # 🏷️ MusicGenre e ListeningStatus
            │
            ├── Data/
            │   └── Database/
            │       ├── Entities/
            │       │   ├── Base/BaseEntity.cs   # 🕒 Auditoria e soft delete
            │       │   └── Music.cs             # 🎵 A tabela de músicas
            │       ├── ModelMapping/            # 🗺️ Fluent API do EF Core
            │       │   ├── Base/BaseEntityTypeConfiguration.cs
            │       │   └── MusicConfiguration.cs
            │       └── Migrations/              # 📜 Histórico versionado do schema
            │
            ├── Schemas/
            │   ├── Requests/MusicRequest.cs     # ✉️ O JSON que entra
            │   ├── Responses/MusicResponse.cs   # 📤 O JSON que sai
            │   ├── Responses/ErrorResponse.cs   # 💥 Formato padrão de erro
            │   ├── Responses/Mapping/MusicResponseMapping.cs
            │   ├── Results/ResultSchema.cs      # ✅ Result Pattern
            │   ├── Results/ResultError.cs       # 🧾 Catálogo de erros
            │   └── Validators/MusicRequestValidator.cs
            │
            ├── Contract/                    # 🔗 Interfaces entre as camadas
            │   ├── IRegisterModule.cs
            │   └── IMusicAggregate.cs
            │
            ├── Aggregate/                   # ⚙️ Regras de negócio
            │   └── MusicAggregate.cs
            │
            ├── Module/                      # 🎮 Rotas HTTP
            │   └── MusicModule.cs
            │
            └── IoC/                         # 🔧 Injeção de dependência e configuração
                ├── Context/TuneTrailDbContext.cs
                ├── Configs/DatabaseConfiguration.cs
                └── Extensions/
                    ├── MinimalExtensions.cs           # 🔁 Registro automático de modules
                    └── ServiceCollectionExtensions.cs
```

---

## 🎥 Demonstração

Por se tratar de um serviço de back-end, a demonstração acontece pelo **Swagger UI** ou por chamadas diretas via `cURL`. Com a aplicação rodando, acesse `https://localhost:7214/swagger` para ver os cinco endpoints agrupados sob a tag **Musics**, cada um com a sua descrição.

**1. Criando uma música**

```bash
curl -X POST 'https://localhost:7214/musics' \
     -H 'Content-Type: application/json' \
     -d '{
       "title": "Bohemian Rhapsody",
       "artist": "Queen",
       "genre": "Rock",
       "status": "Favorite",
       "personalRating": 10,
       "playCount": 42
     }'
```

**Saída esperada:** `201 Created`

```json
{
  "id": "3f2a6c1e-9d84-4b7a-9a1f-2c0e5d8b7a11",
  "title": "Bohemian Rhapsody",
  "artist": "Queen",
  "genre": "Rock",
  "status": "Favorite",
  "personalRating": 10,
  "playCount": 42,
  "createdAt": "2026-08-18T18:41:06.512Z",
  "updatedAt": null
}
```

Repare que `id` e `createdAt` são preenchidos automaticamente, e que os enums trafegam como texto por causa do `JsonStringEnumConverter`.

**2. Listando com filtro por artista**

```bash
curl -X GET 'https://localhost:7214/musics?artist=Queen'
```

**Saída esperada:** `200 OK`, com o array de músicas ordenado da mais recente para a mais antiga. No terminal onde a API está rodando, o `SELECT` com o `WHERE` correspondente aparece no log do EF Core.

**3. Tentando criar a mesma música de novo**

**Saída esperada:** `400 Bad Request`

```json
{
  "code": "E100101",
  "message": "There is already a music with this title for this artist."
}
```

Essa validação não poderia estar no Validator, porque precisou consultar o banco. Por isso mora no Aggregate.

**4. Buscando um identificador inexistente**

```bash
curl -X GET 'https://localhost:7214/musics/00000000-0000-0000-0000-000000000000'
```

**Saída esperada:** `404 Not Found`

```json
{
  "code": "E300101",
  "message": "Music not found."
}
```

É `404` e não `400` porque o Module inspeciona o código do erro antes de escolher o status.

**5. Conferindo o soft delete**

Depois de um `DELETE /musics/{id}`, que responde `204 No Content`, a música some da listagem, mas continua no banco:

```bash
docker exec -it tunetrail-db psql -U admin -d tunetrail-db
```

```sql
SELECT "Title", "Artist", "Deleted" FROM "Music";
```

A linha continua lá, agora com `Deleted = true`.

---

## 🎓 Slides e Tutorial

Se você quer se aprofundar em **Minimal APIs no .NET**, este repositório traz dois materiais completos:

- 📖 **[TUTORIAL.md](./TUTORIAL.md):** um guia autocontido, em 19 passos, que reconstrói a TuneTrail API do zero. Ele começa no `dotnet new`, passa por entidades, Fluent API, migrations, Result Pattern, validadores, aggregates, modules e injeção de dependência, e termina com um roteiro de testes e uma seção de troubleshooting. Mesmo quem não assistiu à apresentação consegue seguir do começo ao fim e chegar exatamente no código que está aqui.
- 🎤 **[Slides do tech shot](./presentation/minimal-apis-techshot.pdf):** a apresentação usada na palestra, com a visão geral do que são Minimal APIs, quando elas fazem sentido e como se comparam ao modelo de Controllers.

Os dois materiais são complementares: os slides dão o panorama e o porquê, o tutorial dá o passo a passo e o como.

---

## 🔗 Documentações utilizadas

- 📖 **Minimal APIs:** [Visão geral das Minimal APIs no ASP.NET Core](https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis/overview)
- 📖 **Roteamento e handlers:** [Route Handlers em Minimal APIs](https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis/route-handlers)
- 📖 **Respostas tipadas:** [TypedResults e IResult](https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis/responses)
- 📖 **ORM:** [Documentação oficial do Entity Framework Core](https://learn.microsoft.com/ef/core/)
- 📖 **Migrations:** [Migrations no EF Core](https://learn.microsoft.com/ef/core/managing-schemas/migrations/)
- 📖 **Provider PostgreSQL:** [Npgsql Entity Framework Core Provider](https://www.npgsql.org/efcore/)
- 📖 **Banco de dados:** [Documentação oficial do PostgreSQL](https://www.postgresql.org/docs/)
- 📖 **Documentação de API:** [Swashbuckle e Swagger no ASP.NET Core](https://learn.microsoft.com/aspnet/core/tutorials/getting-started-with-swashbuckle)
- 📖 **Containerização:** [Documentação de referência do Docker](https://docs.docker.com/)
- 📖 **Injeção de dependência:** [Dependency Injection no ASP.NET Core](https://learn.microsoft.com/aspnet/core/fundamentals/dependency-injection)
- 📖 **Guia de estilo:** [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/)

---

## 👥 Autor

| 👤 Nome              | 🖼️ Foto                                                                                                               | :octocat: GitHub                                                                                                                                                                                | 💼 LinkedIn                                                                                                                                                                                                | 📤 Gmail                                                                                                                                                                                |
| :------------------- | :-------------------------------------------------------------------------------------------------------------------- | :---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Artur Bomtempo Colen | <div align="center"><img src="https://avatars.githubusercontent.com/u/96635074?v=4" width="70px" height="70px"></div> | <div align="center"><a href="https://github.com/arturbomtempo-dev"><img src="https://arturbomtempo-dev.github.io/arturbomtempo-cdn/assets/icons/github.png" width="50px" height="50px"></a></div> | <div align="center"><a href="https://www.linkedin.com/in/artur-bomtempo/"><img src="https://arturbomtempo-dev.github.io/arturbomtempo-cdn/assets/icons/linkedin.png" width="50px" height="50px"></a></div> | <div align="center"><a href="mailto:arturbcolen@gmail.com"><img src="https://arturbomtempo-dev.github.io/arturbomtempo-cdn/assets/icons/gmail.png" width="50px" height="50px"></a></div> |

---

## 🤝 Contribuição

Sugestões, correções e melhorias são bem-vindas, especialmente por se tratar de um projeto com propósito didático.

1. Faça um `fork` do projeto.
2. Crie uma branch para a sua contribuição (`git checkout -b feature/minha-feature`).
3. Faça o commit das suas mudanças (`git commit -m 'feat: adiciona nova funcionalidade X'`). Utilize [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/).
4. Faça o `push` para a branch (`git push origin feature/minha-feature`).
5. Abra um **Pull Request**.

> [!TIP]
> Se a sua contribuição alterar o comportamento da API, atualize também o [TUTORIAL.md](./TUTORIAL.md), para que o passo a passo continue levando exatamente ao código deste repositório.

---

## 🙏 Agradecimentos

- **A todos que acompanharam o tech shot sobre Minimal APIs**, pelas perguntas e discussões que ajudaram a moldar o conteúdo deste repositório e do tutorial.
- [**Documentação oficial da Microsoft**](https://learn.microsoft.com/aspnet/core/), pela referência de qualidade sobre **ASP.NET Core**, **Minimal APIs** e **Entity Framework Core**.
- [**Comunidade .NET**](https://dotnet.microsoft.com/platform/community), pelos artigos, discussões e exemplos que embasaram as decisões de arquitetura adotadas aqui.
- [**Time do Npgsql**](https://www.npgsql.org/), pelo provider que torna a integração entre .NET e PostgreSQL tão direta.

---

## 📄 Licença

Este projeto é distribuído sob a **[Licença MIT](./LICENSE.md)**.

---
