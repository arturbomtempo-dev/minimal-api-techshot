# TuneTrail API: construindo uma Minimal API .NET do zero

Tutorial completo de introdução a **Minimal API** com **.NET 10**, **Entity Framework Core**, **PostgreSQL** e **Docker**.

Ao final você terá uma API REST funcional, organizada em camadas, documentada no Swagger e conectada a um banco PostgreSQL rodando em container.

Este documento é autocontido: se você não assistiu à palestra, consegue seguir do começo ao fim e chegar exatamente no código que está neste repositório.

---

## Índice

- [Antes de começar](#antes-de-começar)
- [Parte 0: o que é uma Minimal API](#parte-0-o-que-é-uma-minimal-api)
- [Parte 1: o projeto TuneTrail](#parte-1-o-projeto-tunetrail)
- [Parte 2: arquitetura e fluxo de uma requisição](#parte-2-arquitetura-e-fluxo-de-uma-requisição)
- [Parte 3: estrutura de pastas do repositório](#parte-3-estrutura-de-pastas-do-repositório)
- [Passo 1: solution e projeto](#passo-1-solution-e-projeto)
- [Passo 2: arquivos da raiz da solution](#passo-2-arquivos-da-raiz-da-solution)
- [Passo 3: docker-compose.yml](#passo-3-docker-composeyml)
- [Passo 4: Program.cs mínimo](#passo-4-programcs-mínimo)
- [Passo 5: Shared, constantes e enums](#passo-5-shared-constantes-e-enums)
- [Passo 6: Entities, a tabela](#passo-6-entities-a-tabela)
- [Passo 7: ModelMapping, a Fluent API](#passo-7-modelmapping-a-fluent-api)
- [Passo 8: DbContext](#passo-8-dbcontext)
- [Passo 9: connection string e DatabaseConfiguration](#passo-9-connection-string-e-databaseconfiguration)
- [Passo 10: primeira migration](#passo-10-primeira-migration)
- [Passo 11: Results, sucesso e erro sem exception](#passo-11-results-sucesso-e-erro-sem-exception)
- [Passo 12: Request, Response e Mapping](#passo-12-request-response-e-mapping)
- [Passo 13: Validators](#passo-13-validators)
- [Passo 14: Contract, as interfaces](#passo-14-contract-as-interfaces)
- [Passo 15: Aggregate, a regra de negócio](#passo-15-aggregate-a-regra-de-negócio)
- [Passo 16: Module, as rotas](#passo-16-module-as-rotas)
- [Passo 17: IoC, injeção de dependência e registro automático](#passo-17-ioc-injeção-de-dependência-e-registro-automático)
- [Passo 18: Program.cs final](#passo-18-programcs-final)
- [Passo 19: rodando e testando](#passo-19-rodando-e-testando)
- [Troubleshooting](#troubleshooting)
- [Recapitulação](#recapitulação)
- [Próximos passos](#próximos-passos)

---

## Antes de começar

### Pré-requisitos

| Ferramenta             | Como verificar                       |
| ---------------------- | ------------------------------------ |
| SDK do .NET 10         | `dotnet --list-sdks`                 |
| Docker Desktop rodando | `docker --version` e `docker ps`     |
| CLI do EF Core         | `dotnet ef --version`                |

O `global.json` trava o **SDK do .NET 10** com `rollForward: latestFeature`, e o projeto tem como alvo o `net10.0`. Instalando o SDK do .NET 10 você já tem tudo o que é necessário para compilar e executar.

Se o `dotnet ef` não estiver instalado:

```bash
dotnet tool install --global dotnet-ef
```

Se já estiver instalado mas desatualizado:

```bash
dotnet tool update --global dotnet-ef
```

### Onde rodar cada comando

Este é o ponto que mais causa confusão, então vale fixar antes de qualquer coisa. O repositório tem **duas raízes diferentes**:

```
minimal-api-techshot/          <-- raiz do repositório Git
├── .gitignore
├── TUTORIAL.md
├── presentation/
└── TuneTrail/                 <-- raiz da SOLUTION
    ├── TuneTrail.slnx
    ├── global.json
    ├── Directory.Build.props
    ├── docker-compose.yml
    └── src/
        └── TuneTrail.Api/
```

**Regra única deste tutorial:** todo comando `dotnet` e todo comando `docker compose` roda a partir de `TuneTrail/`, a raiz da solution.

```bash
cd minimal-api-techshot/TuneTrail
```

Por que ali e não em `src/TuneTrail.Api/`? Porque o `docker-compose.yml`, o `global.json` e o `Directory.Build.props` moram nesse nível. Ficando sempre no mesmo diretório, você nunca precisa adivinhar de onde um caminho relativo parte. Por isso todos os comandos usam `--project src/TuneTrail.Api`.

Mesmo assim, cada bloco de comando abaixo repete de onde ele roda, para você não precisar rolar a página de volta.

> **Nota sobre shell:** os comandos usam sintaxe compatível com `bash`, `zsh` e `PowerShell`. Quando houver diferença, o tutorial mostra as duas versões.

---

## Parte 0: o que é uma Minimal API

### O problema que ela resolve

Até o .NET 5, criar uma API em C# significava obrigatoriamente:

- uma classe `Controller` herdando de `ControllerBase`
- decorada com `[ApiController]` e `[Route("api/[controller]")]`
- métodos decorados com `[HttpGet]`, `[HttpPost]` e afins
- um `Startup.cs` separado do `Program.cs`
- roteamento resolvido por **convenção**, ou seja, o framework adivinha a rota pelo nome da classe

Isso gera muita cerimônia. O iniciante não consegue apontar onde a rota `/api/products/5` foi definida, porque ela não foi definida em lugar nenhum: ela foi _inferida_.

### O que a Minimal API mudou

A partir do .NET 6, você declara a rota **explicitamente**, e o handler é uma função:

```csharp
app.MapGet("/musics/{id}", (Guid id) => $"Music {id}");
```

Pronto. Sem classe, sem atributo, sem convenção. Três características definem uma Minimal API:

1. **Rotas explícitas** via `MapGet`, `MapPost`, `MapPut`, `MapDelete` e `MapGroup`.
2. **Sem `Controller`**: o handler é um lambda ou um método estático comum.
3. **Um único ponto de entrada**, o `Program.cs`, sem `Startup.cs`.

### O mal-entendido mais comum

> "Minimal API é só para projeto pequeno."

Isso está errado, e é o ponto mais importante deste tutorial. O "Minimal" se refere à **cerimônia do framework**, não ao tamanho do projeto. Uma Minimal API escala perfeitamente para dezenas de módulos de rota e dezenas de entidades.

O que acontece é outra coisa: sem Controllers impondo uma estrutura, **você precisa impor a sua**. É exatamente isso que vamos aprender aqui, como organizar uma Minimal API de forma profissional e escalável.

### Comparação lado a lado

|                        | Controller                        | Minimal API                             |
| ---------------------- | --------------------------------- | --------------------------------------- |
| Onde a rota é definida | Atributo mais convenção           | Chamada explícita `MapGet(...)`         |
| Onde o handler mora    | Método de instância na classe     | Lambda ou método estático               |
| Injeção de dependência | Construtor da classe              | Parâmetro do handler (`[FromServices]`) |
| Organização            | Imposta pelo framework            | Escolhida por você                      |
| Overhead de startup    | Maior (descoberta de controllers) | Menor                                   |

---

## Parte 1: o projeto TuneTrail

O TuneTrail é um **diário musical pessoal**. Você cadastra músicas, marca o status de escuta, dá uma nota pessoal e acompanha quantas vezes tocou.

O tema foi escolhido de propósito: é universal, todo mundo entende sem contexto, e continua sendo **uma única tabela**, o que mantém o projeto pequeno o suficiente para ser construído em uma sessão.

### A entidade

| Campo                               | Tipo                     | Observação                                  |
| ----------------------------------- | ------------------------ | ------------------------------------------- |
| `Id`                                | `Guid`                   | Chave primária                              |
| `Title`                             | `string`                 | Obrigatório, até 200 caracteres             |
| `Artist`                            | `string`                 | Obrigatório, até 100 caracteres             |
| `Genre`                             | `MusicGenre` (enum)      | Rock, Pop, Jazz e outros                    |
| `Status`                            | `ListeningStatus` (enum) | WantToListen, Listening, Favorite, Archived |
| `PersonalRating`                    | `int?`                   | Opcional, de 0 a 10                         |
| `PlayCount`                         | `int`                    | Não pode ser negativo                       |
| `CreatedAt`, `UpdatedAt`, `Deleted` | herdados de `BaseEntity` | Auditoria e soft delete                     |

### Endpoints que vamos construir

| Verbo    | Rota           | O que faz                                       |
| -------- | -------------- | ----------------------------------------------- |
| `GET`    | `/musics`      | Lista, com filtro opcional por título e artista |
| `GET`    | `/musics/{id}` | Busca uma música                                |
| `POST`   | `/musics`      | Cria uma música                                 |
| `PUT`    | `/musics/{id}` | Atualiza uma música                             |
| `DELETE` | `/musics/{id}` | Exclui (soft delete)                            |

---

## Parte 2: arquitetura e fluxo de uma requisição

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

A frase que resume a arquitetura inteira:

> O **Module** nunca fala com o banco, e o **Aggregate** nunca sabe o que é HTTP.

O Module só sabe traduzir um resultado em status code. O Aggregate só sabe devolver sucesso ou erro. Nenhum dos dois invade o território do outro, e é essa fronteira que faz o projeto escalar.

### Por que `src/` e uma solution, em vez do projeto solto na raiz

Três motivos concretos:

1. **Separa código de infraestrutura.** Na raiz da solution ficam CI/CD, Docker, documentação e scripts. Em `src/` fica só código de produção.
2. **Prepara para crescer.** Um segundo projeto vira `src/OutroProjeto/` sem reorganizar nada.
3. **Testes têm lugar definido.** Uma pasta `tests/` espelhando `src/` é a convenção que todo dev .NET reconhece.

---

## Parte 3: estrutura de pastas do repositório

Este é o layout final, exatamente como está neste repositório:

```
minimal-api-techshot/                    <-- raiz do repositório Git
├── .gitignore                           <-- gerado por "dotnet new gitignore"
├── TUTORIAL.md
├── presentation/                        <-- slides da apresentação
│
└── TuneTrail/                           <-- raiz da solution
    ├── TuneTrail.slnx                   <-- solution no formato novo (XML)
    ├── global.json                      <-- trava a versão do SDK
    ├── Directory.Build.props            <-- propriedades comuns a todos os projetos
    ├── docker-compose.yml               <-- infraestrutura (PostgreSQL)
    │
    └── src/
        └── TuneTrail.Api/
            ├── TuneTrail.Api.csproj
            ├── Program.cs
            ├── appsettings.json
            ├── appsettings.Development.json
            ├── Properties/
            │   └── launchSettings.json  <-- portas locais de execução
            │
            ├── Shared/
            │   ├── Constants.cs                 <-- limites e mensagens
            │   └── Enums.cs                     <-- MusicGenre, ListeningStatus
            │
            ├── Data/
            │   └── Database/
            │       ├── Entities/
            │       │   ├── Base/BaseEntity.cs   <-- auditoria e soft delete
            │       │   └── Music.cs             <-- a tabela
            │       ├── ModelMapping/
            │       │   ├── Base/BaseEntityTypeConfiguration.cs
            │       │   └── MusicConfiguration.cs
            │       └── Migrations/              <-- gerado pelo EF Core
            │
            ├── Schemas/
            │   ├── Requests/MusicRequest.cs             <-- o que ENTRA
            │   ├── Responses/MusicResponse.cs           <-- o que SAI
            │   ├── Responses/ErrorResponse.cs
            │   ├── Responses/Mapping/MusicResponseMapping.cs
            │   ├── Results/ResultSchema.cs              <-- sucesso e erro sem exception
            │   ├── Results/ResultError.cs               <-- catálogo de erros
            │   └── Validators/MusicRequestValidator.cs
            │
            ├── Contract/
            │   ├── IRegisterModule.cs
            │   └── IMusicAggregate.cs
            │
            ├── Aggregate/
            │   └── MusicAggregate.cs
            │
            ├── Module/
            │   └── MusicModule.cs
            │
            └── IoC/
                ├── Context/TuneTrailDbContext.cs
                ├── Configs/DatabaseConfiguration.cs
                └── Extensions/
                    ├── MinimalExtensions.cs
                    └── ServiceCollectionExtensions.cs
```

### O papel de cada pasta

| Pasta                         | Responsabilidade                                             |
| ----------------------------- | ------------------------------------------------------------ |
| `Shared/`                     | Constantes e enums usados por todas as camadas               |
| `Data/Database/Entities/`     | As classes que viram tabelas no banco                        |
| `Data/Database/ModelMapping/` | Como cada entidade vira tabela (tipos, tamanhos, índices)    |
| `Data/Database/Migrations/`   | Histórico versionado do schema, gerado pelo EF Core          |
| `Schemas/Requests/`           | O formato do JSON que a API recebe                           |
| `Schemas/Responses/`          | O formato do JSON que a API devolve                          |
| `Schemas/Results/`            | O Result Pattern, como o Aggregate comunica sucesso ou falha |
| `Schemas/Validators/`         | Validação de formato e limites de cada campo                 |
| `Contract/`                   | Interfaces, o contrato entre as camadas                      |
| `Aggregate/`                  | Regras de negócio                                            |
| `Module/`                     | As rotas HTTP                                                |
| `IoC/`                        | DbContext, configurações e registro de dependências          |

---

## Passo 1: solution e projeto

### Criando a raiz do repositório

**Rode a partir da pasta onde você guarda seus projetos:**

```bash
mkdir minimal-api-techshot
cd minimal-api-techshot
git init
dotnet new gitignore
```

O `dotnet new gitignore` gera um `.gitignore` já configurado para .NET, ignorando `bin/`, `obj/` e arquivos temporários de IDE. É um detalhe pequeno que evita subir centenas de megabytes de lixo para o GitHub. Ele fica na **raiz do repositório**, valendo para tudo abaixo.

### Criando a solution

**Rode a partir de `minimal-api-techshot/`:**

```bash
mkdir TuneTrail
cd TuneTrail
dotnet new sln --name TuneTrail --format slnx
```

O `--format slnx` gera o formato novo de solution, um XML limpo e legível, em vez do formato `.sln` clássico cheio de GUIDs. Ele é suportado a partir do SDK 9.0.200.

O arquivo `TuneTrail.slnx` gerado (depois de adicionarmos o projeto) fica assim:

```xml
<Solution>
    <Folder Name="/src/">
        <Project Path="src/TuneTrail.Api/TuneTrail.Api.csproj" />
    </Folder>
</Solution>
```

### Criando a API dentro de `src/`

**Rode a partir de `TuneTrail/`:**

```bash
dotnet new web --name TuneTrail.Api --output src/TuneTrail.Api
dotnet sln add src/TuneTrail.Api/TuneTrail.Api.csproj
```

O `dotnet sln add` cria automaticamente a pasta de solution `/src/` no `.slnx`, espelhando o caminho do disco.

**Por que o template `web` e não o `webapi`?**

- `dotnet new webapi` gera um exemplo `WeatherForecast` e, dependendo da versão, Controllers. São coisas que você teria que apagar.
- `dotnet new web` gera o mínimo absoluto: um `Program.cs` com uma rota `/` e nada mais. É o ponto de partida honesto para aprender Minimal API.

### Instalando os pacotes

**Rode a partir de `TuneTrail/`:**

```bash
dotnet add src/TuneTrail.Api package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add src/TuneTrail.Api package Microsoft.EntityFrameworkCore.Design
dotnet add src/TuneTrail.Api package Microsoft.EntityFrameworkCore.Tools
dotnet add src/TuneTrail.Api package Swashbuckle.AspNetCore
```

| Pacote                                  | Para quê                                                              |
| --------------------------------------- | --------------------------------------------------------------------- |
| `Npgsql.EntityFrameworkCore.PostgreSQL` | O provider do EF Core para PostgreSQL, traduz LINQ em SQL do Postgres |
| `Microsoft.EntityFrameworkCore.Design`  | Necessário para o `dotnet ef` gerar migrations                        |
| `Microsoft.EntityFrameworkCore.Tools`   | Comandos de migration dentro do Visual Studio                         |
| `Swashbuckle.AspNetCore`                | Gera o Swagger e a interface OpenAPI                                  |

### Criando as pastas internas

**No bash ou zsh, a partir de `TuneTrail/`:**

```bash
cd src/TuneTrail.Api
mkdir -p Shared Contract Aggregate Module \
         Data/Database/Entities/Base Data/Database/ModelMapping/Base \
         Schemas/Requests Schemas/Responses/Mapping Schemas/Results Schemas/Validators \
         IoC/Context IoC/Configs IoC/Extensions
cd ../..
```

**No PowerShell, a partir de `TuneTrail/`:**

```powershell
"Shared","Contract","Aggregate","Module","Data/Database/Entities/Base","Data/Database/ModelMapping/Base","Schemas/Requests","Schemas/Responses/Mapping","Schemas/Results","Schemas/Validators","IoC/Context","IoC/Configs","IoC/Extensions" | ForEach-Object { New-Item -ItemType Directory -Force -Path "src/TuneTrail.Api/$_" }
```

Repare que você volta para `TuneTrail/` no final. Daqui em diante, todos os comandos rodam desse nível.

---

## Passo 2: arquivos da raiz da solution

Estes três arquivos ficam em `TuneTrail/`, ao lado do `TuneTrail.slnx`.

### `TuneTrail/global.json`

```json
{
  "sdk": {
    "version": "10.0.400",
    "rollForward": "latestFeature"
  }
}
```

Troque `10.0.400` pela versão que apareceu no seu `dotnet --list-sdks`.

**Por que isso importa:** sem o `global.json`, cada pessoa compila com o SDK que tiver instalado. Um colega com SDK diferente pode ter comportamento diferente, ou o CI compila com uma versão e a sua máquina com outra. O `rollForward: latestFeature` permite usar versões mais novas dentro do mesmo major, então não quebra em quem tem um SDK mais recente.

### `TuneTrail/Directory.Build.props`

```xml
<Project>
    <PropertyGroup>
        <Nullable>enable</Nullable>
        <ImplicitUsings>enable</ImplicitUsings>
        <GenerateDocumentationFile>true</GenerateDocumentationFile>
        <NoWarn>$(NoWarn);1591</NoWarn>
    </PropertyGroup>
</Project>
```

O MSBuild importa esse arquivo automaticamente em **todos** os projetos abaixo dele na árvore de pastas. Serve para não repetir configuração em cada `.csproj`. Quando você tiver 5 projetos, muda em um lugar só.

O que cada propriedade faz:

- **`Nullable`**: liga a checagem de nulo do compilador. É o que faz o `= default!` e o `int?` terem significado real.
- **`ImplicitUsings`**: os `using System;`, `using System.Linq;`, `using Microsoft.AspNetCore.Builder;` e outros vêm automáticos. Por isso nossos arquivos têm poucos `using`.
- **`GenerateDocumentationFile`**: gera o XML dos comentários `///`. É isso que faz a descrição aparecer no Swagger UI.
- **`NoWarn 1591`**: desliga o aviso "faltou comentário XML nesse membro público", senão o build enche de warning.

### `TuneTrail/src/TuneTrail.Api/TuneTrail.Api.csproj`

Abra o arquivo e deixe assim. As versões dos pacotes já foram preenchidas pelos comandos `dotnet add package`, então **mantenha as que estiverem no seu arquivo** caso sejam mais novas:

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
    <PropertyGroup>
        <TargetFramework>net10.0</TargetFramework>
        <RootNamespace>TuneTrail.Api</RootNamespace>
    </PropertyGroup>

    <ItemGroup>
        <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.11">
            <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
            <PrivateAssets>all</PrivateAssets>
        </PackageReference>
        <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="10.0.11">
            <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
            <PrivateAssets>all</PrivateAssets>
        </PackageReference>
        <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="10.0.3" />
        <PackageReference Include="Swashbuckle.AspNetCore" Version="10.2.3" />
    </ItemGroup>
</Project>
```

Repare que `Nullable` e `ImplicitUsings` **não estão aqui**: eles vêm do `Directory.Build.props`.

O `<PrivateAssets>all</PrivateAssets>` nos pacotes de EF significa "isso é ferramenta de desenvolvimento, não é dependência de quem consumir esse projeto".

### Verificação

**Rode a partir de `TuneTrail/`:**

```bash
dotnet build
```

Deve compilar sem erros. Se não compilar, pare aqui e resolva antes de seguir.

---

## Passo 3: docker-compose.yml

Este arquivo fica em `TuneTrail/`, na raiz da solution, **não** dentro de `src/`. Ele é infraestrutura da solução inteira, não código da API.

### `TuneTrail/docker-compose.yml`

```yaml
services:
  postgres:
    container_name: tunetrail-db
    image: postgres:17
    restart: unless-stopped
    ports:
      - "5432:5432"
    environment:
      POSTGRES_USER: admin
      POSTGRES_PASSWORD: admin
      POSTGRES_DB: tunetrail-db
    volumes:
      - pg-data:/var/lib/postgresql/data
    networks:
      - tunetrail-network
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U admin -d tunetrail-db"]
      interval: 10s
      timeout: 5s
      retries: 5

networks:
  tunetrail-network:
    driver: bridge

volumes:
  pg-data:
```

### Explicando cada bloco

- **`container_name`**: nome fixo do container. Sem isso o Docker gera um nome como `tunetrail-postgres-1`, e você não consegue prever o nome nos comandos `docker exec`.
- **`ports: "5432:5432"`**: mapeia a porta do container para a sua máquina. O primeiro número é o host, o segundo é o container. É isso que permite a API, que roda fora do Docker, conectar em `localhost:5432`.
- **`environment`**: o Postgres cria o usuário, a senha e o banco no primeiro start usando essas variáveis. Isso **só funciona no primeiro start**. Se você mudar depois, precisa apagar o volume.
- **`volumes: pg-data`**: persiste os dados. Sem isso, todo `docker compose down` apaga tudo.
- **`healthcheck` com `pg_isready`**: o container fica com status "up" **antes** de o Postgres aceitar conexão. Sem o healthcheck você roda a migration e leva um "connection refused".
- **`networks`**: declarar a rede no bloco `networks:` do fim do arquivo **não conecta ninguém nela**. É preciso listar a rede dentro do serviço também, que é o `networks: - tunetrail-network` dentro de `postgres`. Esse é um erro comum de copiar e colar.

### Subindo o banco

**Rode a partir de `TuneTrail/`**, que é onde o `docker-compose.yml` está. O comando `docker compose` procura o arquivo no diretório atual, então rodar de outro lugar dá erro de "no configuration file provided".

```bash
docker compose up -d
docker compose ps
```

Espere aparecer `healthy` na coluna de status. Se ficar em `starting` por muito tempo, veja os logs:

```bash
docker compose logs -f postgres
```

Para sair do acompanhamento de logs, use `Ctrl+C`.

> Na primeira vez o Docker precisa baixar a imagem `postgres:17`, o que pode levar alguns minutos dependendo da conexão. Se você vai apresentar isso, suba o container antes de começar.

---

## Passo 4: Program.cs mínimo

Substitua o conteúdo de `TuneTrail/src/TuneTrail.Api/Program.cs` por:

```csharp
var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/", () => "TuneTrail API is running.");

app.Run();
```

**Rode a partir de `TuneTrail/`:**

```bash
dotnet run --project src/TuneTrail.Api
```

O terminal vai imprimir a URL onde a aplicação subiu. Neste repositório as portas estão definidas em `src/TuneTrail.Api/Properties/launchSettings.json`:

```json
{
  "$schema": "https://json.schemastore.org/launchsettings.json",
  "profiles": {
    "http": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "applicationUrl": "http://localhost:5294",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "https": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "applicationUrl": "https://localhost:7214;http://localhost:5294",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

O template gera portas aleatórias, então **as suas provavelmente serão diferentes**. Abra esse arquivo e anote as suas. Neste repositório são `5294` para HTTP e `7214` para HTTPS.

O `dotnet run` sem argumentos usa o primeiro perfil, que é o `http`. Para rodar em HTTPS:

```bash
dotnet run --project src/TuneTrail.Api --launch-profile https
```

Acesse a URL e você verá o texto `TuneTrail API is running.`.

### Por que essas cinco linhas importam

Esse é o esqueleto **inteiro** de uma Minimal API:

1. `WebApplication.CreateBuilder(args)` cria o _builder_. Aqui você registra serviços (injeção de dependência), configuração e logging.
2. `builder.Build()` congela a configuração e produz a aplicação.
3. `app.MapGet(...)` registra uma rota.
4. `app.Run()` sobe o servidor e começa a escutar.

Guarde esse momento. No fim do tutorial o `Program.cs` vai ter cerca de 55 linhas e ainda vai ser exatamente esse mesmo esqueleto, só com mais coisas penduradas antes do `Build()` e depois dele.

Pare a aplicação com `Ctrl+C`.

---

## Passo 5: Shared, constantes e enums

Comece por aqui: tudo depende disso e é rápido de escrever.

### `src/TuneTrail.Api/Shared/Constants.cs`

```csharp
namespace TuneTrail.Api.Shared;

public static class Constants
{
    public static class CharacterLimits
    {
        public const int ONE_HUNDRED = 100;
        public const int TWO_HUNDRED = 200;
    }

    public static class RatingRange
    {
        public const int MIN = 0;
        public const int MAX = 10;
    }

    public static class MusicValidationMessages
    {
        public const string TITLE_FIELD = "Title is required and must not exceed 200 characters.";
        public const string ARTIST_FIELD = "Artist is required and must not exceed 100 characters.";
        public const string GENRE_FIELD = "Genre must be a valid value.";
        public const string STATUS_FIELD = "Status must be a valid value.";
        public const string PERSONAL_RATING_FIELD = "Personal rating must be between 0 and 10.";
        public const string PLAY_COUNT_FIELD = "Play count cannot be negative.";
    }
}
```

**Por que centralizar:** o `200` do `HasMaxLength` (configuração do banco) e o `200` da validação (regra de negócio) precisam ser o mesmo número. Soltos no código, um dia alguém muda um e esquece o outro: a validação passa e o banco estoura com um erro de truncamento.

### `src/TuneTrail.Api/Shared/Enums.cs`

```csharp
namespace TuneTrail.Api.Shared;

public enum MusicGenre
{
    Rock = 1,
    Pop = 2,
    HipHop = 3,
    Electronic = 4,
    Jazz = 5,
    Classical = 6,
    Mpb = 7,
    Metal = 8,
    Other = 99,
}

public enum ListeningStatus
{
    WantToListen = 1,
    Listening = 2,
    Favorite = 3,
    Archived = 4,
}
```

### Duas decisões que valem entender

**1. Valores numéricos explícitos.** Como o enum é gravado como `int` no banco, se alguém inserir um gênero no meio da lista sem valores explícitos, **todos os registros já salvos mudam de significado silenciosamente**. Imagine 10.000 músicas marcadas como "Jazz" virarem "Classical" porque alguém adicionou "Blues" em ordem alfabética.

**2. Começar em 1, não em 0.** O valor default de um `int` em C# é `0`. Se `Rock = 0`, uma requisição que **não mandou** o campo `Genre` chega no seu código como "Rock", e você nunca vai saber se a pessoa quis dizer Rock ou esqueceu de preencher. Começando em 1, o `0` é sempre "não informado", e o validador do Passo 13 vai rejeitá-lo.

---

## Passo 6: Entities, a tabela

### `src/TuneTrail.Api/Data/Database/Entities/Base/BaseEntity.cs`

```csharp
namespace TuneTrail.Api.Data.Database.Entities.Base;

public abstract class BaseEntity
{
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool Deleted { get; set; }
}
```

Toda entidade herda daqui, então auditoria e soft delete nunca são reescritos.

**O que é soft delete:** em vez de apagar a linha do banco com um `DELETE FROM`, marcamos `Deleted = true` e filtramos em todas as consultas.

**Por quê:** apagar de verdade destrói histórico, quebra relatório e impossibilita auditoria. Se alguém pergunta "por que essa música sumiu?", com soft delete você consegue responder. Com um `DELETE` de verdade, o dado não existe mais em lugar nenhum.

### `src/TuneTrail.Api/Data/Database/Entities/Music.cs`

```csharp
using TuneTrail.Api.Data.Database.Entities.Base;
using TuneTrail.Api.Shared;

namespace TuneTrail.Api.Data.Database.Entities;

/// <summary>
/// Table that stores each music entry of the personal listening log.
/// </summary>
public class Music : BaseEntity
{
    /// <summary>
    /// Unique identifier of the music entry.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Name of the song, e.g. "Bohemian Rhapsody".
    /// </summary>
    public string Title { get; set; } = default!;

    /// <summary>
    /// Performer of the song, e.g. "Queen".
    /// </summary>
    public string Artist { get; set; } = default!;

    /// <summary>
    /// Musical genre of the song.
    /// </summary>
    public MusicGenre Genre { get; set; }

    /// <summary>
    /// Current listening status in the personal log.
    /// </summary>
    public ListeningStatus Status { get; set; }

    /// <summary>
    /// Optional personal score from 0 to 10.
    /// </summary>
    public int? PersonalRating { get; set; }

    /// <summary>
    /// How many times the song was played.
    /// </summary>
    public int PlayCount { get; set; }
}
```

### Dois detalhes de C#

**`= default!`**: com `Nullable` ligado, o compilador reclama de uma `string` não anulável que nunca é inicializada. O `= default!` diz ao compilador "confie, isso vai ser preenchido", e quem preenche é o EF Core ao carregar do banco. Sem isso, você teria um warning em cada propriedade.

**`int?` versus `int`**: o `?` permite nulo. Uma música pode não ter nota ainda (`PersonalRating`), mas sempre tem uma contagem de execuções (`PlayCount`, que começa em 0). Essa diferença vira `NULL` ou `NOT NULL` na coluna do banco.

---

## Passo 7: ModelMapping, a Fluent API

Aqui mora **como** a entidade vira tabela: tipos de coluna, tamanhos, índices e nome da tabela.

**Por que separar da entidade?** A entidade fica sendo só um contêiner de dados, legível de bater o olho. A configuração de banco fica isolada num arquivo próprio. A alternativa, usar atributos como `[MaxLength(200)]` direto na entidade, polui a classe e mistura dois assuntos.

### `src/TuneTrail.Api/Data/Database/ModelMapping/Base/BaseEntityTypeConfiguration.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TuneTrail.Api.Data.Database.Entities.Base;

namespace TuneTrail.Api.Data.Database.ModelMapping.Base;

public abstract class BaseEntityTypeConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : class
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        if (typeof(BaseEntity).IsAssignableFrom(typeof(TEntity)))
        {
            builder.Property(nameof(BaseEntity.CreatedAt)).IsRequired();
            builder.Property(nameof(BaseEntity.UpdatedAt)).IsRequired(false);
            builder.Property(nameof(BaseEntity.Deleted)).IsRequired();
        }
    }
}
```

Configura os campos herdados uma vez só. Quando você tiver 30 entidades, todas ganham auditoria configurada sem repetir nada.

### `src/TuneTrail.Api/Data/Database/ModelMapping/MusicConfiguration.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TuneTrail.Api.Data.Database.Entities;
using TuneTrail.Api.Data.Database.ModelMapping.Base;
using static TuneTrail.Api.Shared.Constants;

namespace TuneTrail.Api.Data.Database.ModelMapping;

public class MusicConfiguration : BaseEntityTypeConfiguration<Music>
{
    public override void Configure(EntityTypeBuilder<Music> builder)
    {
        base.Configure(builder);

        builder.ToTable("Music");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.Title).HasMaxLength(CharacterLimits.TWO_HUNDRED).IsRequired();

        builder.Property(e => e.Artist).HasMaxLength(CharacterLimits.ONE_HUNDRED).IsRequired();

        builder.Property(e => e.Genre).HasConversion<int>().IsRequired();

        builder.Property(e => e.Status).HasConversion<int>().IsRequired();

        builder.Property(e => e.PersonalRating).IsRequired(false);

        builder.Property(e => e.PlayCount).IsRequired();

        builder.HasIndex(e => e.Title);
        builder.HasIndex(e => e.Artist);
    }
}
```

### Pontos importantes

- **`base.Configure(builder)` na primeira linha**: aplica as regras dos campos herdados de `BaseEntity` antes das específicas de `Music`. Esquecer essa linha é um bug silencioso.
- **`using static TuneTrail.Api.Shared.Constants`**: o `static` no using permite escrever `CharacterLimits.TWO_HUNDRED` em vez de `Constants.CharacterLimits.TWO_HUNDRED`.
- **`HasConversion<int>()`**: garante que o enum vá para o Postgres como número. Sem ele, o EF pode gravar como texto, e aí os valores explícitos que definimos no enum não protegem de nada.
- **`HasIndex`**: índice nos campos que vamos usar em filtro, `Title` e `Artist`. Sem índice, filtrar em uma tabela grande faz _full table scan_.

**Rode a partir de `TuneTrail/`:**

```bash
dotnet build
```

---

## Passo 8: DbContext

O `DbContext` é a ponte entre suas classes C# e o banco. Ele:

- expõe as tabelas como `DbSet<T>`
- traduz LINQ em SQL
- rastreia mudanças nos objetos (_change tracking_) para saber o que gravar no `SaveChanges`

### `src/TuneTrail.Api/IoC/Context/TuneTrailDbContext.cs`

```csharp
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using TuneTrail.Api.Data.Database.Entities;
using TuneTrail.Api.Data.Database.Entities.Base;

namespace TuneTrail.Api.IoC.Context;

public class TuneTrailDbContext : DbContext
{
    public TuneTrailDbContext(DbContextOptions<TuneTrailDbContext> options)
        : base(options) { }

    public DbSet<Music> Music { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditInformation();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyAuditInformation()
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }
    }
}
```

### As três partes

**1. `ApplyConfigurationsFromAssembly`**

Uma linha que varre o assembly procurando **todas** as classes que implementam `IEntityTypeConfiguration<T>` e aplica cada uma. Sem isso, você teria que escrever `modelBuilder.ApplyConfiguration(new MusicConfiguration());` para cada entidade, e um dia esqueceria uma. Com 30 entidades, a diferença é gritante.

**2. Auditoria automática no `SaveChangesAsync`**

Sobrescrevemos o `SaveChangesAsync` para preencher `CreatedAt` e `UpdatedAt` sozinho. O `ChangeTracker` sabe quais objetos foram adicionados e quais foram modificados. Assim, **nenhum Aggregate precisa lembrar de setar data**, é impossível esquecer.

Repare no `DateTime.UtcNow`, não `DateTime.Now`. O Npgsql exige UTC para colunas `timestamp with time zone`. Usar `DateTime.Now` gera uma exceção em runtime, e é um erro clássico de quem vem de SQL Server.

**3. `DbSet<Music> Music`**

Expõe a tabela. O nome da propriedade é o que você usaria em `_dbContext.Music`. No nosso Aggregate vamos usar `_dbContext.Set<Music>()`, que é equivalente e funciona mesmo sem declarar o `DbSet`.

---

## Passo 9: connection string e DatabaseConfiguration

### `src/TuneTrail.Api/appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=tunetrail-db;Username=admin;Password=admin"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

Os valores batem exatamente com o `docker-compose.yml`: usuário `admin`, senha `admin`, banco `tunetrail-db`, porta `5432`. Se você mudou alguma coisa lá, mude aqui também.

> **Aviso honesto:** senha em `appsettings.json` só é aceitável porque é um banco local de desenvolvimento. Em produção isso vai para variável de ambiente, Azure Key Vault ou equivalente.

### `src/TuneTrail.Api/appsettings.Development.json`

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  }
}
```

A última linha faz o **SQL gerado pelo EF aparecer no console**. Você chama um endpoint e vê a query real que foi para o Postgres, o que concretiza a ideia de que "LINQ vira SQL".

### `src/TuneTrail.Api/IoC/Configs/DatabaseConfiguration.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using TuneTrail.Api.IoC.Context;

namespace TuneTrail.Api.IoC.Configs;

public static class DatabaseConfiguration
{
    public static IServiceCollection AddDatabaseConfiguration(this WebApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

        builder.Services.AddDbContext<TuneTrailDbContext>(options =>
        {
            options.UseNpgsql(
                connectionString,
                npgsqlOptions =>
                {
                    npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3);
                }
            );
        });

        return builder.Services;
    }
}
```

### Por que um arquivo separado só para isso?

Esse é um dos conceitos mais importantes do tutorial.

Sem Controllers, todo o setup da aplicação acontece no `Program.cs`. Se você jogar tudo lá dentro, em seis meses ele tem 400 linhas e ninguém acha nada. A solução idiomática em Minimal API é o **extension method**: cada assunto (banco, autenticação, CORS, telemetria) vira um método de extensão em um arquivo próprio, e o `Program.cs` fica sendo um **índice legível**:

```csharp
builder.AddDatabaseConfiguration();
builder.AddTelemetryConfiguration();
builder.AddHealthCheckConfiguration();
```

O `EnableRetryOnFailure` faz o EF tentar de novo em falhas transitórias de rede, o que é importante em nuvem, onde conexões caem sozinhas de vez em quando.

### Atualize o `Program.cs`

Ainda é uma versão intermediária, mas agora com o banco registrado. A migration do próximo passo precisa disso para funcionar, porque o `dotnet ef` executa o `Program.cs` para descobrir a connection string.

```csharp
using TuneTrail.Api.IoC.Configs;

var builder = WebApplication.CreateBuilder(args);

builder.AddDatabaseConfiguration();

var app = builder.Build();

app.MapGet("/", () => "TuneTrail API is running.");

app.Run();
```

**Rode a partir de `TuneTrail/`:**

```bash
dotnet build
```

---

## Passo 10: primeira migration

Uma **migration** é um arquivo C# gerado que descreve, em código, a diferença entre o modelo atual e o que existe no banco. É versionado no Git, então o histórico do schema fica junto com o histórico do código.

> O container do Postgres precisa estar rodando e `healthy`. Se você não subiu ainda, volte ao Passo 3.

### Gerando a migration

**Rode a partir de `TuneTrail/`, no bash ou zsh:**

```bash
dotnet ef migrations add InitialCreate \
  --project src/TuneTrail.Api \
  --startup-project src/TuneTrail.Api \
  --output-dir Data/Database/Migrations
```

**No PowerShell**, a quebra de linha usa crase, então prefira uma linha só:

```powershell
dotnet ef migrations add InitialCreate --project src/TuneTrail.Api --startup-project src/TuneTrail.Api --output-dir Data/Database/Migrations
```

Explicando as flags, que existem por causa da pasta `src/`:

- **`--project`**: onde está o `DbContext` e onde a migration será gerada.
- **`--startup-project`**: qual projeto tem o `Program.cs` que configura a injeção de dependência. O EF **executa** o seu `Program.cs` em tempo de design para descobrir a connection string. Aqui é o mesmo projeto, mas em soluções com projeto de infraestrutura separado eles diferem.
- **`--output-dir`**: onde salvar, para acompanhar a organização do resto do projeto.

Isso cria `src/TuneTrail.Api/Data/Database/Migrations/` com três arquivos:

| Arquivo                                 | O que é                                                           |
| --------------------------------------- | ----------------------------------------------------------------- |
| `<timestamp>_InitialCreate.cs`          | O que muda no banco, com `Up` e `Down`                            |
| `<timestamp>_InitialCreate.Designer.cs` | Snapshot do modelo no momento dessa migration                     |
| `TuneTrailDbContextModelSnapshot.cs`    | Snapshot do modelo atual, usado para calcular a próxima migration |

Abra o `<timestamp>_InitialCreate.cs`. Dá para ler o `CreateTable` com todas as colunas que configuramos, os tipos como `character varying(200)`, o `nullable: true` no `PersonalRating` e os índices no fim:

```csharp
migrationBuilder.CreateTable(
    name: "Music",
    columns: table => new
    {
        Id = table.Column<Guid>(type: "uuid", nullable: false),
        Title = table.Column<string>(
            type: "character varying(200)",
            maxLength: 200,
            nullable: false
        ),
        // ...
    },
    constraints: table =>
    {
        table.PrimaryKey("PK_Music", x => x.Id);
    }
);

migrationBuilder.CreateIndex(name: "IX_Music_Artist", table: "Music", column: "Artist");

migrationBuilder.CreateIndex(name: "IX_Music_Title", table: "Music", column: "Title");
```

É aqui que a Fluent API do Passo 7 "vira" banco de dados de forma visível.

### Aplicando no banco

**Rode a partir de `TuneTrail/`:**

```bash
dotnet ef database update --project src/TuneTrail.Api --startup-project src/TuneTrail.Api
```

### Conferindo no Postgres

Entre no `psql` dentro do container:

**Rode a partir de qualquer lugar (é um comando do Docker, não do compose):**

```bash
docker exec -it tunetrail-db psql -U admin -d tunetrail-db
```

Já dentro do `psql`, digite:

```sql
\dt
```

Você verá duas tabelas: `Music` e `__EFMigrationsHistory`. A segunda é como o EF sabe quais migrations já foram aplicadas.

Para ver a estrutura da tabela:

```sql
\d "Music"
```

> As aspas em `"Music"` são obrigatórias. O EF cria a tabela com o nome exatamente como escrevemos, e no Postgres identificadores sem aspas são convertidos para minúsculo. Sem as aspas, o `psql` procura por `music` e não encontra.

Para sair do `psql`:

```sql
\q
```

---

## Passo 11: Results, sucesso e erro sem exception

### O problema

Como o Aggregate avisa o Module que "a música não existe"? Existem três opções:

1. **Lançar exception.** Funciona, mas exception é cara em performance e serve para o _inesperado_. "Música não encontrada" é um resultado perfeitamente esperado, não uma falha do sistema.
2. **Retornar `null`.** O Module não consegue distinguir "não encontrei" de "deu erro de validação" de "o banco caiu".
3. **Retornar um objeto de resultado.** Carrega sucesso ou falha, mensagem, código de erro e o valor. É o que vamos fazer.

Esse padrão se chama **Result Pattern**.

### `src/TuneTrail.Api/Schemas/Results/ResultError.cs`

```csharp
namespace TuneTrail.Api.Schemas.Results;

public readonly struct ResultError
{
    public ResultError(string message, string code)
    {
        Message = message;
        Code = code;
    }

    public string Message { get; }
    public string Code { get; }

    /*
     * Error Code Pattern: E + Operation + Module + Id
     *
     * [ Operation | Code ]        [ Module  | Code ]
     * [ Generic   | 00   ]        [ Generic | 00   ]
     * [ Insert    | 10   ]        [ Music   | 01   ]
     * [ Update    | 20   ]
     * [ Get       | 30   ]        [ Id: 01 to 99 ]
     * [ Delete    | 40   ]
     *
     * Example: E300101 -> Get / Music / first error
     */

    // Generic errors (00)
    public static readonly ResultError UnexpectedError = new(
        "An unexpected error occurred.",
        "E000000"
    );

    public static ResultError RequiredField(string field, string message) =>
        new($"The field {field} is required. {message}", "E100099");

    public static ResultError InvalidField(string field, string message) =>
        new($"The field {field} is invalid. {message}", "E100098");

    // Music errors (01)
    public static readonly ResultError MusicNotFound = new("Music not found.", "E300101");

    public static readonly ResultError DuplicatedMusic = new(
        "There is already a music with this title for this artist.",
        "E100101"
    );

    public static readonly ResultError ErrorOnListingMusics = new(
        "Error listing musics.",
        "E300102"
    );

    public static readonly ResultError ErrorOnCreatingMusic = new(
        "Error creating music.",
        "E100102"
    );

    public static readonly ResultError ErrorOnUpdatingMusic = new(
        "Error updating music.",
        "E200101"
    );

    public static readonly ResultError ErrorOnDeletingMusic = new(
        "Error deleting music.",
        "E400101"
    );
}
```

**Por que um código de erro estruturado?** Quando o suporte recebe um print de tela com "E300101", ele localiza a causa exata em segundos, sem depender da mensagem, que pode ser traduzida ou reescrita. O front-end também pode reagir programaticamente ao código, em vez de comparar strings de mensagem.

> **Por que os erros de falha inesperada não recebem a mensagem da exception?** Seria tentador escrever `ErrorOnCreatingMusic(ex.Message)` e devolver o texto do erro para quem chamou. Não faça isso: a mensagem de uma exception do Npgsql pode expor nome de tabela, nome de constraint, trecho de SQL e até parte da connection string. O detalhe vai para o **log**, onde a sua equipe consegue ver; o cliente recebe uma mensagem estável e o código do erro, que é o suficiente para abrir um chamado. Repare no Passo 15: o `catch` faz `_logger.LogError(ex, ...)` **e** devolve um erro genérico. As duas coisas, sempre.

### `src/TuneTrail.Api/Schemas/Results/ResultSchema.cs`

```csharp
namespace TuneTrail.Api.Schemas.Results;

public class ResultSchema
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string Message { get; }
    public string Code { get; }

    protected ResultSchema(bool isSuccess, string message, string code)
    {
        IsSuccess = isSuccess;
        Message = message;
        Code = code;
    }

    public static ResultSchema Success() => new(true, string.Empty, string.Empty);

    public static ResultSchema Fail(ResultError error) => new(false, error.Message, error.Code);
}

public class ResultSchema<T> : ResultSchema
    where T : class
{
    private readonly T? _value;

    private ResultSchema(T? value, bool isSuccess, string message, string code)
        : base(isSuccess, message, code)
    {
        _value = value;
    }

    public T Value => _value!;

    public static ResultSchema<T> Success(T value) => new(value, true, string.Empty, string.Empty);

    public static new ResultSchema<T> Fail(ResultError error) =>
        new(default, false, error.Message, error.Code);
}
```

São duas versões: `ResultSchema` para operações que não devolvem dado, como o delete, e `ResultSchema<T>` que carrega o valor.

O `new` no `Fail` do genérico é porque ele **esconde** o `Fail` da classe base: mesma assinatura, tipo de retorno diferente.

---

## Passo 12: Request, Response e Mapping

### Por que não usar a entidade direto na API?

Quatro motivos:

1. **Segurança.** Se o endpoint recebe a entidade, alguém pode mandar `{"deleted": true}` ou `{"id": "..."}` no corpo e sobrescrever campos que não deveria. Com um `Request` que só tem os campos editáveis, isso é impossível por construção.
2. **Acoplamento.** Renomear uma coluna do banco quebraria o contrato da API para todos os clientes.
3. **Vazamento de dados.** A entidade pode ter campos internos que não devem ir para o cliente.
4. **Formatos diferentes.** O que entra e o que sai raramente são iguais. O `Request` não tem `Id`, que vem da rota, nem `CreatedAt`, que é do sistema; o `Response` tem os dois.

### `src/TuneTrail.Api/Schemas/Requests/MusicRequest.cs`

```csharp
using TuneTrail.Api.Shared;

namespace TuneTrail.Api.Schemas.Requests;

public class MusicRequest
{
    /// <summary>
    /// Name of the song, e.g. "Bohemian Rhapsody".
    /// </summary>
    public string Title { get; set; } = default!;

    /// <summary>
    /// Performer of the song, e.g. "Queen".
    /// </summary>
    public string Artist { get; set; } = default!;

    /// <summary>
    /// Musical genre of the song.
    /// </summary>
    public MusicGenre Genre { get; set; }

    /// <summary>
    /// Current listening status in the personal log.
    /// </summary>
    public ListeningStatus Status { get; set; }

    /// <summary>
    /// Optional personal score from 0 to 10.
    /// </summary>
    public int? PersonalRating { get; set; }

    /// <summary>
    /// How many times the song was played.
    /// </summary>
    public int PlayCount { get; set; }
}
```

**Repare no que não está aqui:** `Id`, `CreatedAt`, `UpdatedAt` e `Deleted`. O `Id` vem da rota `/musics/{id}`, as datas são do sistema e o `Deleted` é controlado pelo endpoint de exclusão. Essa ausência é uma decisão de segurança, não um esquecimento.

### `src/TuneTrail.Api/Schemas/Responses/MusicResponse.cs`

```csharp
using TuneTrail.Api.Shared;

namespace TuneTrail.Api.Schemas.Responses;

public class MusicResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = default!;
    public string Artist { get; set; } = default!;
    public MusicGenre Genre { get; set; }
    public ListeningStatus Status { get; set; }
    public int? PersonalRating { get; set; }
    public int PlayCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

Note que `Deleted` **não** aparece aqui. É um detalhe interno de implementação, o cliente não precisa saber que usamos soft delete.

### `src/TuneTrail.Api/Schemas/Responses/ErrorResponse.cs`

```csharp
using TuneTrail.Api.Schemas.Results;

namespace TuneTrail.Api.Schemas.Responses;

public class ErrorResponse
{
    public string Code { get; set; }
    public string Message { get; set; }

    public ErrorResponse(ResultSchema result)
    {
        Code = result.Code;
        Message = result.Message;
    }
}
```

Um formato **único** de erro para toda a API. O cliente sempre recebe `{ "code": "...", "message": "..." }`, não importa qual endpoint falhou. Consistência é o que separa uma API profissional de uma amadora.

### `src/TuneTrail.Api/Schemas/Responses/Mapping/MusicResponseMapping.cs`

```csharp
using TuneTrail.Api.Data.Database.Entities;

namespace TuneTrail.Api.Schemas.Responses.Mapping;

public static class MusicResponseMapping
{
    public static MusicResponse? MapToResponse(Music? music)
    {
        if (music is null)
            return null;

        return new MusicResponse
        {
            Id = music.Id,
            Title = music.Title,
            Artist = music.Artist,
            Genre = music.Genre,
            Status = music.Status,
            PersonalRating = music.PersonalRating,
            PlayCount = music.PlayCount,
            CreatedAt = music.CreatedAt,
            UpdatedAt = music.UpdatedAt,
        };
    }
}
```

A conversão é **manual**, de propósito. Existem bibliotecas como o AutoMapper que fazem isso por reflexão, mas em um projeto de aprendizado o mapeamento explícito é melhor: você vê exatamente o que sai, e um campo novo na entidade não vaza para a API sem alguém decidir.

---

## Passo 13: Validators

### `src/TuneTrail.Api/Schemas/Validators/MusicRequestValidator.cs`

```csharp
using TuneTrail.Api.Schemas.Requests;
using TuneTrail.Api.Schemas.Results;
using static TuneTrail.Api.Shared.Constants;

namespace TuneTrail.Api.Schemas.Validators;

public static class MusicRequestValidator
{
    public static List<ResultError> ValidationErrors(this MusicRequest request)
    {
        var errors = new List<ResultError>();

        if (
            string.IsNullOrWhiteSpace(request.Title)
            || request.Title.Length > CharacterLimits.TWO_HUNDRED
        )
        {
            errors.Add(
                ResultError.RequiredField(
                    nameof(request.Title),
                    MusicValidationMessages.TITLE_FIELD
                )
            );
        }

        if (
            string.IsNullOrWhiteSpace(request.Artist)
            || request.Artist.Length > CharacterLimits.ONE_HUNDRED
        )
        {
            errors.Add(
                ResultError.RequiredField(
                    nameof(request.Artist),
                    MusicValidationMessages.ARTIST_FIELD
                )
            );
        }

        if (!Enum.IsDefined(request.Genre))
        {
            errors.Add(
                ResultError.InvalidField(nameof(request.Genre), MusicValidationMessages.GENRE_FIELD)
            );
        }

        if (!Enum.IsDefined(request.Status))
        {
            errors.Add(
                ResultError.InvalidField(
                    nameof(request.Status),
                    MusicValidationMessages.STATUS_FIELD
                )
            );
        }

        if (
            request.PersonalRating.HasValue
            && (
                request.PersonalRating < RatingRange.MIN || request.PersonalRating > RatingRange.MAX
            )
        )
        {
            errors.Add(
                ResultError.InvalidField(
                    nameof(request.PersonalRating),
                    MusicValidationMessages.PERSONAL_RATING_FIELD
                )
            );
        }

        if (request.PlayCount < 0)
        {
            errors.Add(
                ResultError.InvalidField(
                    nameof(request.PlayCount),
                    MusicValidationMessages.PLAY_COUNT_FIELD
                )
            );
        }

        return errors;
    }
}
```

### Em qual camada a validação deve ficar?

Existem três lugares possíveis, e cada um tem um papel:

| Camada                                | O que valida                                                      | Exemplo                                                                   |
| ------------------------------------- | ----------------------------------------------------------------- | ------------------------------------------------------------------------- |
| **Module**                            | Nada de negócio. No máximo o formato que o framework já converteu | Um `{id}` que não é Guid nem chega no handler, o roteamento rejeita antes |
| **Validator**, chamado pelo Aggregate | Formato e limites do campo                                        | Título vazio, nota fora de 0 a 10, enum inválido                          |
| **Aggregate**                         | Regras que precisam do banco ou de contexto                       | "Já existe essa música desse artista"                                     |

**A regra de ouro:** validação de campo **não pode ficar no Module**. Se ficasse, você teria que repetir a mesma validação no POST e no PUT, e qualquer endpoint novo que criasse uma música poderia esquecer de validar. Colocando no Aggregate, via Validator, é **impossível** criar uma música sem passar pela validação, porque não existe outro caminho até o banco.

### Dois detalhes técnicos

**Extension method.** O `ValidationErrors()` é declarado com `this MusicRequest request`, o que permite escrever `request.ValidationErrors()` em vez de `MusicRequestValidator.ValidationErrors(request)`. Fica lendo como uma frase.

**`Enum.IsDefined`.** Verifica se o número recebido corresponde a um valor declarado no enum. Sem isso, mandar `"genre": 42` no JSON criaria uma música com um gênero que não existe, e o banco aceitaria numa boa. Esse é o motivo de termos começado os enums em 1: um campo não enviado chega como `0`, e o `Enum.IsDefined` rejeita.

---

## Passo 14: Contract, as interfaces

### `src/TuneTrail.Api/Contract/IRegisterModule.cs`

```csharp
namespace TuneTrail.Api.Contract;

public interface IRegisterModule
{
    void RegisterModule(WebApplication app);
}
```

Um contrato simples: "toda classe de rotas sabe se registrar sozinha numa `WebApplication`". No Passo 17 vamos usar isso para descobrir e registrar todos os módulos automaticamente.

### `src/TuneTrail.Api/Contract/IMusicAggregate.cs`

```csharp
using TuneTrail.Api.Schemas.Requests;
using TuneTrail.Api.Schemas.Responses;
using TuneTrail.Api.Schemas.Results;

namespace TuneTrail.Api.Contract;

public interface IMusicAggregate
{
    Task<ResultSchema<MusicResponse>> GetMusicById(Guid musicId);

    Task<ResultSchema<IEnumerable<MusicResponse>>> ListMusics(
        string? title = null,
        string? artist = null
    );

    Task<ResultSchema<MusicResponse>> CreateMusic(MusicRequest request);

    Task<ResultSchema<MusicResponse>> UpdateMusic(Guid musicId, MusicRequest request);

    Task<ResultSchema> DeleteMusic(Guid musicId);
}
```

### Por que uma interface se só existe uma implementação?

Três motivos concretos:

1. **Testabilidade.** No teste do Module, você troca o Aggregate por um dublê que devolve o que quiser, sem precisar de banco.
2. **Inversão de dependência.** O Module depende de `IMusicAggregate`, uma abstração, e não de `MusicAggregate`, uma implementação. Trocar a implementação não toca no Module.
3. **Documentação executável.** Basta abrir a pasta `Contract/` e ver todas as operações do domínio, sem ler implementação.

Repare também que a interface é **assíncrona**, com `Task<...>`. Toda operação que toca o banco é I/O, e I/O em .NET é assíncrono para não bloquear a thread enquanto espera a resposta. É isso que permite a API atender muitas requisições simultâneas com poucas threads.

---

## Passo 15: Aggregate, a regra de negócio

Este é o arquivo mais longo e o mais importante do projeto.

### `src/TuneTrail.Api/Aggregate/MusicAggregate.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using TuneTrail.Api.Contract;
using TuneTrail.Api.Data.Database.Entities;
using TuneTrail.Api.IoC.Context;
using TuneTrail.Api.Schemas.Requests;
using TuneTrail.Api.Schemas.Responses;
using TuneTrail.Api.Schemas.Responses.Mapping;
using TuneTrail.Api.Schemas.Results;
using TuneTrail.Api.Schemas.Validators;

namespace TuneTrail.Api.Aggregate;

public class MusicAggregate : IMusicAggregate
{
    private readonly TuneTrailDbContext _dbContext;
    private readonly ILogger<MusicAggregate> _logger;

    public MusicAggregate(TuneTrailDbContext dbContext, ILogger<MusicAggregate> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<ResultSchema<MusicResponse>> GetMusicById(Guid musicId)
    {
        try
        {
            var music = await _dbContext
                .Set<Music>()
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == musicId && !m.Deleted);

            if (music is null)
                return ResultSchema<MusicResponse>.Fail(ResultError.MusicNotFound);

            var response = MusicResponseMapping.MapToResponse(music);
            if (response is null)
                return ResultSchema<MusicResponse>.Fail(ResultError.UnexpectedError);

            return ResultSchema<MusicResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting music. Id: {MusicId}", musicId);
            return ResultSchema<MusicResponse>.Fail(ResultError.UnexpectedError);
        }
    }

    public async Task<ResultSchema<IEnumerable<MusicResponse>>> ListMusics(
        string? title = null,
        string? artist = null
    )
    {
        try
        {
            var query = _dbContext.Set<Music>().AsNoTracking().Where(m => !m.Deleted);

            if (!string.IsNullOrWhiteSpace(title))
                query = query.Where(m => EF.Functions.ILike(m.Title, $"%{title}%"));

            if (!string.IsNullOrWhiteSpace(artist))
                query = query.Where(m => EF.Functions.ILike(m.Artist, $"%{artist}%"));

            var musics = await query.OrderByDescending(m => m.CreatedAt).ToListAsync();

            var response = musics
                .Select(MusicResponseMapping.MapToResponse)
                .OfType<MusicResponse>()
                .ToList();

            return ResultSchema<IEnumerable<MusicResponse>>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error listing musics. Title: {Title} | Artist: {Artist}",
                title,
                artist
            );

            return ResultSchema<IEnumerable<MusicResponse>>.Fail(ResultError.ErrorOnListingMusics);
        }
    }

    public async Task<ResultSchema<MusicResponse>> CreateMusic(MusicRequest request)
    {
        try
        {
            var errors = request.ValidationErrors();
            if (errors.Count != 0)
                return ResultSchema<MusicResponse>.Fail(errors[0]);

            var title = request.Title.Trim();
            var artist = request.Artist.Trim();

            var alreadyExists = await _dbContext
                .Set<Music>()
                .AnyAsync(m =>
                    EF.Functions.ILike(m.Title, title)
                    && EF.Functions.ILike(m.Artist, artist)
                    && !m.Deleted
                );

            if (alreadyExists)
                return ResultSchema<MusicResponse>.Fail(ResultError.DuplicatedMusic);

            var music = BuildNewMusic(request);

            _dbContext.Add(music);
            await _dbContext.SaveChangesAsync();

            var response = MusicResponseMapping.MapToResponse(music);
            if (response is null)
                return ResultSchema<MusicResponse>.Fail(ResultError.UnexpectedError);

            return ResultSchema<MusicResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating music. Title: {Title}", request.Title);
            return ResultSchema<MusicResponse>.Fail(ResultError.ErrorOnCreatingMusic);
        }
    }

    public async Task<ResultSchema<MusicResponse>> UpdateMusic(Guid musicId, MusicRequest request)
    {
        try
        {
            var errors = request.ValidationErrors();
            if (errors.Count != 0)
                return ResultSchema<MusicResponse>.Fail(errors[0]);

            var music = await _dbContext
                .Set<Music>()
                .FirstOrDefaultAsync(m => m.Id == musicId && !m.Deleted);

            if (music is null)
                return ResultSchema<MusicResponse>.Fail(ResultError.MusicNotFound);

            UpdateMusicData(music, request);

            await _dbContext.SaveChangesAsync();

            var response = MusicResponseMapping.MapToResponse(music);
            if (response is null)
                return ResultSchema<MusicResponse>.Fail(ResultError.UnexpectedError);

            return ResultSchema<MusicResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating music. Id: {MusicId}", musicId);
            return ResultSchema<MusicResponse>.Fail(ResultError.ErrorOnUpdatingMusic);
        }
    }

    public async Task<ResultSchema> DeleteMusic(Guid musicId)
    {
        try
        {
            var music = await _dbContext
                .Set<Music>()
                .FirstOrDefaultAsync(m => m.Id == musicId && !m.Deleted);

            if (music is null)
                return ResultSchema.Fail(ResultError.MusicNotFound);

            music.Deleted = true;

            await _dbContext.SaveChangesAsync();

            return ResultSchema.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting music. Id: {MusicId}", musicId);
            return ResultSchema.Fail(ResultError.ErrorOnDeletingMusic);
        }
    }

    #region Helper methods

    /// <summary>
    /// Creates a new Music instance from the request values.
    /// </summary>
    private static Music BuildNewMusic(MusicRequest request) =>
        new()
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            Artist = request.Artist.Trim(),
            Genre = request.Genre,
            Status = request.Status,
            PersonalRating = request.PersonalRating,
            PlayCount = request.PlayCount,
        };

    /// <summary>
    /// Copies the request values into an existing Music instance.
    /// </summary>
    private static void UpdateMusicData(Music music, MusicRequest request)
    {
        music.Title = request.Title.Trim();
        music.Artist = request.Artist.Trim();
        music.Genre = request.Genre;
        music.Status = request.Status;
        music.PersonalRating = request.PersonalRating;
        music.PlayCount = request.PlayCount;
    }

    #endregion Helper methods
}
```

### O padrão que se repete em todo método

```
try
    1. valida o request            -> devolve Fail se inválido
    2. consulta o banco            -> devolve Fail se não encontrou
    3. aplica a regra de negócio
    4. persiste (SaveChangesAsync)
    5. mapeia para Response
    6. devolve Success
catch
    loga o erro com contexto
    devolve Fail genérico
```

Essa previsibilidade é o que faz um projeto com dezenas de módulos ser navegável.

### Nove detalhes que valem entender

**1. Injeção de dependência pelo construtor.** O `TuneTrailDbContext` e o `ILogger<MusicAggregate>` chegam prontos. Quem os cria é o container de injeção de dependência, que vamos configurar no Passo 17. Você nunca dá `new` em nada disso.

**2. `AsNoTracking()` nas leituras.** Por padrão o EF guarda uma cópia de cada objeto lido para detectar mudanças. Em uma consulta só de leitura isso é desperdício de memória e CPU. Repare que ele **não** aparece no update e no delete, porque ali precisamos do tracking.

**3. E, como há tracking, não existe `_dbContext.Update(...)`.** Este é o outro lado da moeda do item anterior, e é o erro mais comum de quem está começando com EF Core. No `UpdateMusic` e no `DeleteMusic` a entidade veio de uma consulta **sem** `AsNoTracking()`, então o `ChangeTracker` já está observando aquele objeto: basta alterar as propriedades e chamar `SaveChangesAsync()`. Chamar `Update()` ali não só é redundante, como **piora** o SQL, porque marca todas as propriedades como modificadas e o EF passa a escrever todas as colunas. Sem ele, com o log do Passo 9 ligado, um `PUT` que muda só a nota gera:

```sql
UPDATE "Music" SET "PersonalRating" = @p0, "PlayCount" = @p1, "Status" = @p2, "UpdatedAt" = @p3
WHERE "Id" = @p4;
```

Só as colunas que realmente mudaram. `Update()` existe para o caso oposto: uma entidade _desanexada_, que você recebeu pronta e o EF nunca viu.

**4. `Add`, e não `AddAsync`.** `AddAsync` só é necessário para geradores de valor especiais que precisam ir ao banco, como o `HiLo`. Fora desse caso a versão assíncrona não traz benefício algum: o `Add` apenas marca a entidade no `ChangeTracker`, sem I/O. Quem vai ao banco é o `SaveChangesAsync()`.

**5. Filtro `!m.Deleted` em toda consulta.** É o soft delete em ação. Uma música excluída continua no banco, mas nunca aparece.

**6. A query é montada por partes.** Os `if` adicionam `Where` condicionalmente e **nada vai ao banco ainda**, porque `IQueryable` é _lazy_. Só quando chamamos `ToListAsync()` o EF gera **uma única query SQL** com todos os filtros. Com o log de SQL ligado no Passo 9, chame `/musics?artist=Queen` e veja o `WHERE` aparecer no console.

**7. `EF.Functions.ILike` no lugar de `Contains`.** O caminho intuitivo seria `m.Title.Contains(title)`, e ele funciona, mas o EF traduz isso para um `LIKE`, que no PostgreSQL é **sensível a maiúsculas e minúsculas**. Na prática, buscar por `?artist=queen` não acharia `"Queen"`, o que é um filtro de busca quebrado. O `ILike` é o operador `ILIKE` do Postgres, a versão que ignora a caixa:

```sql
WHERE m."Artist" ILIKE @__Format_1 AND NOT (m."Deleted")
```

Repare que `EF.Functions` é a porta de entrada para funções que só existem no banco: você ganha o recurso do Postgres sem escrever SQL na mão e sem perder a tipagem. O preço é o acoplamento ao provider, um trade-off consciente aqui.

**8. Regra de negócio que precisa do banco, e a normalização que ela exige.** O `alreadyExists` do `CreateMusic` é o exemplo perfeito de validação que **não** poderia estar no Validator: ela precisa consultar o banco. Validador cuida do campo; Aggregate cuida da regra. Note que a checagem usa `.Trim()` e `ILike` **pelo mesmo motivo**: se ela comparasse com `==`, o texto `"  bohemian rhapsody "` passaria como música nova e você teria duas linhas para a mesma canção. A regra de unicidade tem que enxergar o dado do mesmo jeito que a busca enxerga.

**9. Log estruturado.** Em `_logger.LogError(ex, "... Id: {MusicId}", musicId)` os valores vão como **campos separados**, não concatenados na string. Em uma ferramenta de observabilidade você consegue filtrar por `MusicId`. Concatenar com `$"..."` perde essa capacidade. E, como vimos no Passo 11, é aqui que o detalhe da exception fica: no log, nunca na resposta HTTP.

**Rode a partir de `TuneTrail/`:**

```bash
dotnet build
```

---

## Passo 16: Module, as rotas

Chegamos na camada que dá nome ao tutorial.

### `src/TuneTrail.Api/Module/MusicModule.cs`

```csharp
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using TuneTrail.Api.Contract;
using TuneTrail.Api.Schemas.Requests;
using TuneTrail.Api.Schemas.Responses;
using TuneTrail.Api.Schemas.Results;

namespace TuneTrail.Api.Module;

public class MusicModule : IRegisterModule
{
    public void RegisterModule(WebApplication app)
    {
        var musicGroup = app.MapGroup("/musics").WithTags("Musics");

        musicGroup
            .MapGet("/{id:guid}", GetMusicById)
            .WithName("GetMusicById")
            .WithDescription("Gets a single music entry by its unique identifier.");

        musicGroup
            .MapGet("/", ListMusics)
            .WithName("ListMusics")
            .WithDescription("Lists music entries, optionally filtering by title and artist.");

        musicGroup
            .MapPost("/", CreateMusic)
            .WithName("CreateMusic")
            .WithDescription("Creates a new music entry.");

        musicGroup
            .MapPut("/{id:guid}", UpdateMusic)
            .WithName("UpdateMusic")
            .WithDescription("Updates an existing music entry.");

        musicGroup
            .MapDelete("/{id:guid}", DeleteMusic)
            .WithName("DeleteMusic")
            .WithDescription("Soft deletes an existing music entry.");
    }

    public static async Task<
        Results<Ok<MusicResponse>, NotFound<ErrorResponse>, BadRequest<ErrorResponse>>
    > GetMusicById([FromRoute] Guid id, [FromServices] IMusicAggregate musicAggregate)
    {
        var result = await musicAggregate.GetMusicById(id);

        if (result.IsFailure)
        {
            if (result.Code == ResultError.MusicNotFound.Code)
                return TypedResults.NotFound(new ErrorResponse(result));

            return TypedResults.BadRequest(new ErrorResponse(result));
        }

        return TypedResults.Ok(result.Value);
    }

    public static async Task<
        Results<Ok<IEnumerable<MusicResponse>>, BadRequest<ErrorResponse>>
    > ListMusics(
        [FromQuery] string? title,
        [FromQuery] string? artist,
        [FromServices] IMusicAggregate musicAggregate
    )
    {
        var result = await musicAggregate.ListMusics(title, artist);

        if (result.IsFailure)
            return TypedResults.BadRequest(new ErrorResponse(result));

        return TypedResults.Ok(result.Value);
    }

    public static async Task<
        Results<Created<MusicResponse>, BadRequest<ErrorResponse>>
    > CreateMusic([FromBody] MusicRequest request, [FromServices] IMusicAggregate musicAggregate)
    {
        var result = await musicAggregate.CreateMusic(request);

        if (result.IsFailure)
            return TypedResults.BadRequest(new ErrorResponse(result));

        return TypedResults.Created($"/musics/{result.Value.Id}", result.Value);
    }

    public static async Task<
        Results<Ok<MusicResponse>, NotFound<ErrorResponse>, BadRequest<ErrorResponse>>
    > UpdateMusic(
        [FromRoute] Guid id,
        [FromBody] MusicRequest request,
        [FromServices] IMusicAggregate musicAggregate
    )
    {
        var result = await musicAggregate.UpdateMusic(id, request);

        if (result.IsFailure)
        {
            if (result.Code == ResultError.MusicNotFound.Code)
                return TypedResults.NotFound(new ErrorResponse(result));

            return TypedResults.BadRequest(new ErrorResponse(result));
        }

        return TypedResults.Ok(result.Value);
    }

    public static async Task<
        Results<NoContent, NotFound<ErrorResponse>, BadRequest<ErrorResponse>>
    > DeleteMusic([FromRoute] Guid id, [FromServices] IMusicAggregate musicAggregate)
    {
        var result = await musicAggregate.DeleteMusic(id);

        if (result.IsFailure)
        {
            if (result.Code == ResultError.MusicNotFound.Code)
                return TypedResults.NotFound(new ErrorResponse(result));

            return TypedResults.BadRequest(new ErrorResponse(result));
        }

        return TypedResults.NoContent();
    }
}
```

### Anatomia de uma rota

```csharp
var musicGroup = app.MapGroup("/musics")    // prefixo comum a todas as rotas
    .WithTags("Musics");                    // agrupa no Swagger UI

musicGroup
    .MapGet("/{id:guid}", GetMusicById)     // GET /musics/{id} chama o método GetMusicById
    .WithName("GetMusicById")               // nome único, usado para gerar links
    .WithDescription("...");                // aparece no Swagger
```

**`MapGroup` é o recurso que salva projetos grandes.** Sem ele, você repetiria `/musics` em cada rota. Com ele, o prefixo é declarado uma vez e, mais importante, você pode aplicar autenticação, rate limit ou filtros **ao grupo inteiro** com uma linha só.

**E o `:guid` no fim do parâmetro?** É uma _route constraint_: ela diz ao roteamento que aquele segmento só casa se for um Guid válido. Sem ela, `GET /musics/abc` seria roteado até o handler, e só então o binding falharia com um `400` genérico e sem corpo. Com ela, a rota simplesmente não casa e a resposta é um `404`, que é a leitura correta: `/musics/abc` não é um recurso que exista nesta API. É a mesma ideia da tabela do Passo 13, "um `{id}` que não é Guid nem chega no handler" — o `:guid` é o que torna aquela frase verdadeira. De quebra, a constraint aparece no OpenAPI, então o Swagger já documenta o formato esperado.

### `TypedResults` e o tipo de retorno gigante

```csharp
Task<Results<Ok<MusicResponse>, NotFound<ErrorResponse>, BadRequest<ErrorResponse>>>
```

Parece assustador, mas é literalmente uma frase: _"este endpoint devolve ou um 200 com `MusicResponse`, ou um 404 com `ErrorResponse`, ou um 400 com `ErrorResponse`"_.

Por que vale a pena:

1. **O compilador te obriga.** Se você tentar retornar um `Conflict` que não está declarado, o build quebra. Com o `IResult` genérico, isso passaria e você descobriria em produção.
2. **O Swagger fica correto de graça.** Todos os status codes possíveis aparecem documentados sem escrever um único `[ProducesResponseType]`.
3. **É autodocumentado.** Basta bater o olho na assinatura para saber tudo que o endpoint pode responder.

O `TypedResults.Ok(...)` é a versão tipada de `Results.Ok(...)`. Sempre prefira `TypedResults` em Minimal API, porque é o que dá a checagem em tempo de compilação.

### A escolha entre 404 e 400

Repare no `if` dentro dos handlers:

```csharp
if (result.Code == ResultError.MusicNotFound.Code)
    return TypedResults.NotFound(new ErrorResponse(result));

return TypedResults.BadRequest(new ErrorResponse(result));
```

O Aggregate devolveu só um código de erro. **Quem decide o status code HTTP é o Module**, e isso é exatamente a fronteira entre as camadas: o Aggregate não sabe o que é 404.

### Os atributos `[From...]`

| Atributo         | De onde vem o valor                    | Exemplo                  |
| ---------------- | -------------------------------------- | ------------------------ |
| `[FromRoute]`    | Da URL                                 | `/musics/{id}`           |
| `[FromQuery]`    | Da query string                        | `/musics?title=Bohemian` |
| `[FromBody]`     | Do corpo JSON                          | O `MusicRequest` do POST |
| `[FromServices]` | Do container de injeção de dependência | O `IMusicAggregate`      |

**Esse é o coração da injeção de dependência em Minimal API.** Em um Controller, as dependências entram pelo construtor da classe. Aqui, entram como **parâmetro do handler**. O `[FromServices]` diz "não procure isso na requisição, pegue do container".

Na prática o ASP.NET Core infere a maior parte disso sozinho: tipos complexos vão para o body, tipos registrados na injeção de dependência vêm dos serviços. Mesmo assim, seja explícito. Em código que outras pessoas vão ler, explícito ganha de esperto.

### Por que os handlers são métodos estáticos, e não lambdas?

Você pode escrever tudo inline:

```csharp
musicGroup.MapGet("/{id}", async (Guid id, IMusicAggregate agg) => { /* ... */ });
```

Funciona, e é o que você vê na maioria dos tutoriais de Minimal API. Mas com 5 endpoints o `RegisterModule` viraria um paredão de 150 linhas. Extraindo para métodos estáticos:

- o `RegisterModule` vira um **índice** legível, você bate o olho e vê todas as rotas
- cada handler fica testável isoladamente
- o `static` deixa explícito que o handler não guarda estado

### O Module não sabe o que é banco

Releia qualquer handler: não existe `_dbContext`, não existe `Where`, não existe SQL. Ele faz três coisas e só:

1. Recebe os parâmetros da requisição
2. Chama o Aggregate
3. Traduz `ResultSchema` em status code HTTP

E o inverso também vale: releia o Aggregate e você não encontra `404`, `TypedResults` ou qualquer traço de HTTP.

---

## Passo 17: IoC, injeção de dependência e registro automático

### `src/TuneTrail.Api/IoC/Extensions/ServiceCollectionExtensions.cs`

```csharp
using TuneTrail.Api.Aggregate;
using TuneTrail.Api.Contract;

namespace TuneTrail.Api.IoC.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterServices(this IServiceCollection services)
    {
        services.AddScoped<IMusicAggregate, MusicAggregate>();

        return services;
    }
}
```

Uma linha por Aggregate. Em um projeto grande esse arquivo tem dezenas de registros, e a arquitetura continua a mesma: o que muda é a quantidade de linhas.

### Os três tempos de vida

```csharp
services.AddScoped<IMusicAggregate, MusicAggregate>();     // 1 instância por requisição HTTP
services.AddSingleton<IFoo, Foo>();                        // 1 instância para a app inteira
services.AddTransient<IBar, Bar>();                        // 1 instância a cada injeção
```

**Por que `Scoped` para o Aggregate?** Porque o `DbContext` é `Scoped`, já que o `AddDbContext` o registra assim. Todo o trabalho de uma requisição compartilha o mesmo `DbContext`, e no fim da requisição ele é descartado, liberando a conexão.

**Armadilha clássica:** registrar um serviço como `Singleton` quando ele depende do `DbContext`, que é `Scoped`. O `DbContext` ficaria vivo para sempre, acumulando objetos rastreados até estourar a memória. O container detecta isso e lança erro no startup, mas é bom saber o porquê.

### `src/TuneTrail.Api/IoC/Extensions/MinimalExtensions.cs`

```csharp
using System.Reflection;
using TuneTrail.Api.Contract;

namespace TuneTrail.Api.IoC.Extensions;

public static class MinimalExtensions
{
    public static void RegisterModules(this WebApplication app)
    {
        var moduleDefinitions = Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(t =>
                t.IsAssignableTo(typeof(IRegisterModule)) && !t.IsAbstract && !t.IsInterface
            )
            .Select(Activator.CreateInstance)
            .Cast<IRegisterModule>();

        foreach (var module in moduleDefinitions)
        {
            module.RegisterModule(app);
        }
    }
}
```

### O truque mais elegante do padrão

Esse método faz o seguinte, por **reflexão**, que é a capacidade do .NET de inspecionar o próprio código em tempo de execução:

1. Pega o assembly da aplicação
2. Lista **todos** os tipos dentro dele
3. Filtra os que implementam `IRegisterModule` e são classes concretas
4. Instancia cada um
5. Chama `RegisterModule(app)` em cada um

**O resultado:** para adicionar um `ArtistModule` amanhã, você cria a classe implementando `IRegisterModule` e pronto, as rotas aparecem. Nenhuma linha no `Program.cs`. É impossível esquecer de registrar um módulo.

Compare com a alternativa manual:

```csharp
new MusicModule().RegisterModule(app);
new ArtistModule().RegisterModule(app);
new PlaylistModule().RegisterModule(app);
// e um dia alguém esquece uma linha e passa uma hora procurando
// por que a rota dá 404
```

---

## Passo 18: Program.cs final

Agora sim, a versão completa.

### `src/TuneTrail.Api/Program.cs`

```csharp
using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.OpenApi;
using TuneTrail.Api.IoC.Configs;
using TuneTrail.Api.IoC.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddDatabaseConfiguration();

builder.Services.RegisterServices();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Version = "v1",
            Title = "TuneTrail API",
            Description = "Personal music listening log built with .NET Minimal API.",
        }
    );

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.DocumentTitle = "TuneTrail API";
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "TuneTrail API v1");
    });
}

app.UseHttpsRedirection();

app.RegisterModules();

app.Run();
```

### O que cada bloco faz

| Bloco                                       | Papel                                            |
| ------------------------------------------- | ------------------------------------------------ |
| `builder.AddDatabaseConfiguration()`        | Registra o `DbContext` apontando para o Postgres |
| `builder.Services.RegisterServices()`       | Registra os Aggregates na injeção de dependência |
| `ConfigureHttpJsonOptions`                  | Faz os enums irem e voltarem como texto no JSON  |
| `AddEndpointsApiExplorer` e `AddSwaggerGen` | Montam o documento OpenAPI                       |
| `IncludeXmlComments`                        | Puxa os comentários `///` para dentro do Swagger |
| `app.UseSwagger` e `UseSwaggerUI`           | Servem o JSON e a interface, só em Development   |
| `app.UseHttpsRedirection()`                 | Redireciona HTTP para HTTPS                      |
| `app.RegisterModules()`                     | Registra as rotas de todos os `IRegisterModule`  |

### Volte ao Passo 4 e compare

A anatomia é idêntica:

```
builder -> configurações -> Build() -> pipeline -> rotas -> Run()
```

Tudo que cresceu foram configurações penduradas antes e depois do `Build()`. Esse é o argumento final contra o mito de que "Minimal API não escala": um projeto com dezenas de módulos tem exatamente essa mesma anatomia no `Program.cs`.

### As duas metades do arquivo

Há uma fronteira rígida no `var app = builder.Build();`:

- **Antes**, com `builder.Services...`, você **registra** o que existe. Nada roda ainda.
- **Depois**, com `app.Use...`, você **monta o pipeline**, que é a fila de middlewares por onde cada requisição passa.

Tentar registrar um serviço depois do `Build()` lança exceção, porque a configuração está congelada.

### Sobre a ordem do pipeline

O pipeline é uma fila, e a ordem importa. Se o `UseHttpsRedirection` viesse depois do registro das rotas, uma requisição HTTP seria processada antes de ser redirecionada para HTTPS. Em projetos maiores a ordem típica é `UseRouting`, `UseCors`, autenticação, autorização e por último as rotas.

### Enums como texto

O `JsonStringEnumConverter` faz a API aceitar e devolver `"Rock"` em vez de `1`. A diferença na experiência de quem consome é grande:

```json
{ "genre": "Rock", "status": "Favorite" }
```

em vez de

```json
{ "genre": 1, "status": 3 }
```

O conversor também aceita o número na entrada, então clientes antigos não quebram.

**Rode a partir de `TuneTrail/`:**

```bash
dotnet build
```

---

## Passo 19: rodando e testando

### Subindo tudo

**Rode a partir de `TuneTrail/`**, nesta ordem:

```bash
docker compose up -d
```

```bash
dotnet ef database update --project src/TuneTrail.Api --startup-project src/TuneTrail.Api
```

```bash
dotnet run --project src/TuneTrail.Api --launch-profile https
```

Deixe esse último terminal aberto: ele é o log da aplicação, e é onde o SQL do EF Core vai aparecer.

### Abrindo o Swagger

Com o perfil `https` deste repositório, o endereço é:

```
https://localhost:7214/swagger
```

Com o perfil `http` (que é o que o `dotnet run` sem argumentos usa):

```
http://localhost:5294/swagger
```

Confirme as suas portas em `src/TuneTrail.Api/Properties/launchSettings.json` ou na saída do console.

Você deve ver os 5 endpoints agrupados sob a tag **Musics**, cada um com a sua descrição.

**Um ajuste que vale fazer agora.** Os dois perfis têm `launchBrowser: true`, e no Passo 4 isso funcionava porque existia um `MapGet("/")` respondendo na raiz. Esse endpoint foi removido no Passo 18, então hoje o navegador abre em `/` e você recebe um 404 em branco toda vez que roda o projeto. Acrescente um `launchUrl` aos **dois** perfis do `launchSettings.json`:

```json
"http": {
  "commandName": "Project",
  "dotnetRunMessages": true,
  "launchBrowser": true,
  "launchUrl": "swagger",
  "applicationUrl": "http://localhost:5294",
  "environmentVariables": {
    "ASPNETCORE_ENVIRONMENT": "Development"
  }
}
```

Agora o `dotnet run` abre o navegador direto na documentação. É um detalhe pequeno, e é exatamente o tipo de coisa que decide se quem clonou o seu repositório vê a API funcionando em cinco segundos ou fica achando que quebrou.

### Roteiro de teste, nesta ordem

**1. `POST /musics`, criando com sucesso**

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

Resposta esperada: **201 Created**, com o `id` gerado e o `createdAt` preenchido automaticamente pelo `DbContext`.

Olhe o terminal onde a API está rodando: o `INSERT` do EF aparece lá, graças ao log configurado no `appsettings.Development.json`.

Copie o `id` da resposta, você vai usar nos próximos passos.

**2. `POST /musics` de novo, com o mesmo título e artista**

Desta vez mande com outra caixa e com espaços sobrando, de propósito:

```json
{
  "title": "  bohemian rhapsody ",
  "artist": "queen",
  "genre": "Rock",
  "status": "Favorite",
  "personalRating": 10,
  "playCount": 42
}
```

Resposta esperada: **400 Bad Request** com

```json
{
  "code": "E100101",
  "message": "There is already a music with this title for this artist."
}
```

Duas coisas acontecendo aqui. A primeira: essa validação não poderia estar no Validator, porque precisou consultar o banco. Por isso mora no Aggregate. A segunda: ela reconheceu a duplicata mesmo com a caixa e os espaços diferentes, graças ao `.Trim()` e ao `ILike` do Passo 15. Se a comparação fosse `==`, você teria acabado de criar uma segunda linha para a mesma música.

**3. `POST /musics` com nota inválida**

```json
{
  "title": "Test",
  "artist": "Test",
  "genre": "Pop",
  "status": "Listening",
  "personalRating": 99,
  "playCount": 0
}
```

Resposta esperada: **400** com o código `E100098` e a mensagem sobre a faixa de 0 a 10. Essa mora no Validator, porque é só o campo.

**4. `POST /musics` com título vazio**

Mesma ideia, mas com o código `E100099`.

**5. `GET /musics`**

Devolve a lista. Em seguida chame `GET /musics?artist=queen`, tudo em minúsculo, e confirme que a música do Queen volta mesmo assim. No console, o `WHERE` com `ILIKE` aparece no log do EF Core.

**6. `GET /musics/{id}` com um Guid aleatório**

Resposta esperada: **404 Not Found** com `E300101`. Repare que é 404 e não 400: quem decide isso é o Module, olhando o código do erro.

**6b. `GET /musics/abc`, um id que nem é Guid**

Resposta esperada: **404 Not Found**, com o corpo vazio. Aqui o Aggregate nem chegou a ser chamado: a _route constraint_ `{id:guid}` do Passo 16 fez a rota não casar, e a requisição parou no roteamento.

**7. `PUT /musics/{id}`**

Atualize a nota da música que você criou. Repare que o `updatedAt` saiu de `null` e o `createdAt` não mudou.

**8. `DELETE /musics/{id}`**

Resposta esperada: **204 No Content**.

**9. Confirmando o soft delete**

Chame `GET /musics` de novo: a música sumiu da lista.

Agora vá no banco. **Rode em outro terminal:**

```bash
docker exec -it tunetrail-db psql -U admin -d tunetrail-db
```

Dentro do `psql`:

```sql
SELECT "Title", "Artist", "Deleted" FROM "Music";
```

A linha ainda está lá, com `Deleted = true`. Saia com `\q`.

### Parando tudo

Pare a API com `Ctrl+C` no terminal em que ela roda.

Para parar o banco, **a partir de `TuneTrail/`:**

```bash
docker compose down
```

Se quiser apagar também os dados:

```bash
docker compose down -v
```

---

## Troubleshooting

### `Npgsql.NpgsqlException: Connection refused`

O container não está de pé ou não terminou de subir.

**Rode a partir de `TuneTrail/`:**

```bash
docker compose ps
docker compose logs postgres
```

O status precisa estar `healthy`.

### `no configuration file provided: not found` ao rodar `docker compose`

Você está no diretório errado. O `docker-compose.yml` está em `TuneTrail/`.

### `Cannot write DateTime with Kind=Local to PostgreSQL type 'timestamp with time zone'`

Você usou `DateTime.Now` em algum lugar. Troque por `DateTime.UtcNow`.

### `No DbContext was found` ao rodar `dotnet ef`

Faltou o `--project` ou o `--startup-project`, ou o `Program.cs` não chama o `AddDatabaseConfiguration()`.

**Rode a partir de `TuneTrail/`:**

```bash
dotnet ef migrations add InitialCreate --project src/TuneTrail.Api --startup-project src/TuneTrail.Api --output-dir Data/Database/Migrations
```

### `relation "Music" does not exist`

A migration foi criada mas não aplicada.

**Rode a partir de `TuneTrail/`:**

```bash
dotnet ef database update --project src/TuneTrail.Api --startup-project src/TuneTrail.Api
```

### O Swagger não abre

Três causas comuns:

1. **Você está em `/`, e não em `/swagger`.** A partir do Passo 18 não existe mais rota na raiz, então `/` responde 404. Confira se o `launchUrl` do Passo 19 está nos dois perfis do `launchSettings.json`.
2. **A aplicação não está em ambiente Development**, e o `UseSwagger` está dentro do `if (app.Environment.IsDevelopment())`. Os perfis do `launchSettings.json` já definem `ASPNETCORE_ENVIRONMENT=Development`, então isso costuma acontecer só quando você roda a DLL publicada direto.
3. **Você está tentando a porta errada.** Confira o `launchSettings.json` e a saída do console.

### Aviso "Failed to determine the https port for redirect"

Acontece quando você roda com o perfil `http`, que não expõe uma porta HTTPS, e o `UseHttpsRedirection` não sabe para onde redirecionar. Não quebra nada: o Swagger continua funcionando em `http://localhost:5294/swagger`. Para eliminar o aviso, rode com o perfil `https`.

**Rode a partir de `TuneTrail/`:**

```bash
dotnet run --project src/TuneTrail.Api --launch-profile https
```

### As descrições dos endpoints não aparecem no Swagger

Falta o `GenerateDocumentationFile` no `Directory.Build.props`, ou o arquivo XML não foi encontrado. Confira se existe `src/TuneTrail.Api/bin/Debug/net10.0/TuneTrail.Api.xml` depois do build.

### Preciso recomeçar o banco do zero

**Rode a partir de `TuneTrail/`:**

```bash
docker compose down -v
docker compose up -d
dotnet ef database update --project src/TuneTrail.Api --startup-project src/TuneTrail.Api
```

O `-v` apaga o volume, ou seja, todos os dados.

### A porta 5432 já está em uso

Você tem outro Postgres rodando, local ou em outro container. Mude o mapeamento no `docker-compose.yml` para `"5433:5432"` e ajuste a connection string do `appsettings.json` para `Port=5433`.

### `dotnet build` reclama do formato `.slnx`

Seu SDK é anterior ao 9.0.200. Atualize o SDK, ou gere a solution no formato clássico com `dotnet new sln --name TuneTrail` (sem o `--format slnx`).

---

## Recapitulação

Percorra o caminho de uma requisição real, nomeando os arquivos:

```
POST /musics
     |
     v
MusicModule.CreateMusic()              <- recebe o MusicRequest do corpo JSON
     |                                    e pega o IMusicAggregate da injeção de dependência
     v
MusicAggregate.CreateMusic()           <- 1. request.ValidationErrors()  (campos)
     |                                    2. AnyAsync(...)              (regra de negócio)
     |                                    3. BuildNewMusic(request)
     v
TuneTrailDbContext.SaveChangesAsync()  <- preenche CreatedAt e gera o INSERT
     |
     v
PostgreSQL
     |
     v
MusicResponseMapping.MapToResponse()   <- entidade vira DTO de saída
     |
     v
TypedResults.Created(...)              <- 201 mais o header Location
```

### As cinco ideias centrais

1. **Minimal API não é "API pequena".** É API sem a cerimônia de Controllers, e escala para dezenas de módulos.

2. **Sem Controllers, a organização é sua responsabilidade.** O framework não impõe estrutura. O caminho `Module` para `Aggregate` para `DbContext` é uma estrutura que funciona e é fácil de defender.

3. **Cada camada tem uma fronteira que não atravessa.** O Module não fala com o banco. O Aggregate não sabe o que é HTTP. Se você precisar quebrar isso, provavelmente a responsabilidade está na camada errada.

4. **Validação de campo mora no Validator; regra que precisa do banco mora no Aggregate.** Nunca no Module, senão você repete a validação em cada endpoint e um dia esquece.

5. **`Program.cs` é um índice, não um depósito.** Extension methods em `IoC/Configs/` e `IoC/Extensions/` mantêm cada assunto no seu arquivo.

---

## Próximos passos

Se você quiser continuar o projeto depois:

1. **Testes.** Crie `tests/TuneTrail.Api.Tests` com xUnit e adicione ao `.slnx`. A pasta `Contract/` foi feita exatamente para isso: você troca o `IMusicAggregate` por um dublê e testa o Module sem banco.
2. **Paginação** no endpoint de listagem, com `page` e `pageSize`.
3. **`CancellationToken` de ponta a ponta.** Receba um `CancellationToken` em cada handler do Module, repasse pelo `IMusicAggregate` e entregue ao `ToListAsync(ct)`, `FirstOrDefaultAsync(ct)` e `SaveChangesAsync(ct)`. Quando o cliente desiste da requisição, a query é cancelada no banco em vez de seguir consumindo conexão. Ficou de fora aqui para não poluir a leitura das camadas, mas em produção é o padrão.
4. **Health check** com `builder.Services.AddHealthChecks().AddNpgSql(...)` e `app.MapHealthChecks("/health")`.
5. **Uma segunda entidade com relacionamento**, por exemplo `Playlist` contendo várias `Music`. Aqui você exercita `ArtistModule` ou `PlaylistModule` e vê o `RegisterModules()` do Passo 17 pegando o módulo novo sozinho.
6. **Autenticação JWT**, e aí o `BaseEntity` finalmente ganha um `CreatedBy`.
7. **Dockerfile da API**, para subir banco e aplicação com um único `docker compose up`.
8. **README na raiz do repositório**, com a stack, o diagrama de camadas e as instruções de execução. É o que alguém lê primeiro ao abrir o projeto no GitHub.
9. **Arquivo `.http`** dentro de `src/TuneTrail.Api/`, com as chamadas prontas. O VS Code, com a extensão REST Client, e o Visual Studio executam esse arquivo direto do editor, o que é um bom plano B se o Swagger der problema.

Bom workshop.
