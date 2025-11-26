# Especificação Técnica - PoC Sistema de Eventos Musical

## Resumo Executivo

Esta especificação detalha a implementação de uma arquitetura orientada a eventos para gestão e enriquecimento de dados musicais. A solução utiliza o padrão "Database per Service" com comunicação assíncrona via RabbitMQ entre um serviço Java (Spring Boot) e um serviço .NET. O foco principal é a interoperabilidade, atualização em tempo real via SSE (Server-Sent Events) e uma forte governança de eventos utilizando AsyncAPI (SpringWolf e Neuroglia), Apicurio Registry e EventCatalog, tudo orquestrado via Docker Compose.

## Arquitetura do Sistema

### Visão Geral dos Componentes

A arquitetura é composta por três serviços de aplicação e um conjunto de serviços de infraestrutura:

1.  **Music Service (Java/Spring Boot):** Responsável pelo ciclo de vida das músicas. Expõe API REST para cadastro e SSE para streaming de atualizações. Publica eventos de criação e consome eventos de enriquecimento. Utiliza **SpringWolf** para documentação AsyncAPI.
2.  **Video Enricher Service (.NET/C#):** Responsável por buscar metadados de vídeos no YouTube. Consome eventos de criação de música, realiza scraping e publica eventos de vídeo encontrado. Utiliza **Neuroglia.AsyncAPI** para documentação **AsyncAPI v3**.
3.  **Music Frontend (React):** Interface do usuário para cadastro e listagem em tempo real.
4.  **Infraestrutura de Governança & Mensageria:**
    *   **RabbitMQ:** Broker de mensagens.
    *   **PostgreSQL:** Duas instâncias (ou bancos lógicos) para isolamento de dados.
    *   **Apicurio Registry:** Registro central de schemas (Avro/JSON Schema).
    *   **EventCatalog:** Portal de documentação estática gerado a partir das specs AsyncAPI, consumindo endpoints via plugin oficial `@eventcatalog/generator-asyncapi`.

### Fluxo de Dados

1.  **Cadastro:** Frontend -> `POST /songs` (Music Service) -> DB (Status: PENDING) -> RabbitMQ (`song.created`).
2.  **Enriquecimento:** RabbitMQ (`song.created`) -> Video Enricher Service -> YouTube Scraping -> DB (Video Metadata) -> RabbitMQ (`video.found`).
3.  **Atualização:** RabbitMQ (`video.found`) -> Music Service -> DB (Update: Video URL, Views, Status: COMPLETED).
4.  **Notificação:** Music Service -> SSE Connection -> Frontend (Update UI).
5.  **Documentação:** EventCatalog -> HTTP GET (SpringWolf/Neuroglia Endpoints) -> Generate Static Site.

## Design de Implementação

### Interfaces Principais

#### Music Service (Java)

```java
// Controller
@PostMapping("/songs")
public ResponseEntity<SongResponse> createSong(@RequestBody SongRequest request);

@GetMapping("/songs")
public ResponseEntity<List<SongResponse>> listSongs();

@GetMapping(value = "/songs/stream", produces = MediaType.TEXT_EVENT_STREAM_VALUE)
public SseEmitter streamSongs();

// Event Listener
@RabbitListener(queues = "${app.rabbitmq.queue.video-found}")
public void onVideoFound(VideoFoundEvent event);
```

#### Video Enricher Service (.NET)

```csharp
// Event Consumer
public class SongCreatedConsumer : IConsumer<SongCreatedEvent>
{
    public Task Consume(ConsumeContext<SongCreatedEvent> context);
}

// Scraper Service
public interface IYouTubeScraper
{
    Task<VideoMetadata> SearchVideoAsync(string artist, string title);
}
```

### Modelos de Dados

#### Entidades de Domínio (Simplificado)

**Music Service (Java - JPA Entity):**
*   `id` (UUID)
*   `title` (String)
*   `artist` (String)
*   `releaseDate` (LocalDate - ISO 8601)
*   `genre` (String)
*   `videoUrl` (String, nullable)
*   `views` (Long, nullable)
*   `status` (Enum: PENDING, COMPLETED, NOT_FOUND)

**Video Enricher Service (.NET - EF Core Entity):**
*   `Id` (Guid)
*   `SongId` (Guid) - Referência externa
*   `VideoUrl` (String)
*   `Views` (Long)
*   `ScrapedAt` (DateTime)

#### Eventos (Contratos AsyncAPI)

**SongCreatedEvent:**
```json
{
  "songId": "uuid",
  "title": "string",
  "artist": "string",
  "releaseDate": "ISO8601",
  "genre": "string"
}
```

**VideoFoundEvent:**
```json
{
  "songId": "uuid",
  "videoUrl": "string",
  "views": 1000000
}
```

### Endpoints de API

| Método | Caminho | Descrição |
| :--- | :--- | :--- |
| `POST` | `/api/v1/songs` | Cadastra nova música. Retorna 201 Created com o recurso inicial (status PENDING). |
| `GET` | `/api/v1/songs` | Lista todas as músicas cadastradas. |
| `GET` | `/api/v1/songs/stream` | Endpoint SSE para receber atualizações de status das músicas em tempo real. |
| `GET` | `/springwolf/docs` | Endpoint AsyncAPI (JSON) exposto pelo SpringWolf. |
| `GET` | `/asyncapi/docs` | Endpoint AsyncAPI v3 (JSON) exposto pelo Neuroglia. |

## Pontos de Integração

- **RabbitMQ:** Troca de mensagens assíncronas. Requer configuração de Exchanges (`music.events`) e Queues (`music.song-created`, `music.video-found`).
- **YouTube (Scraping):** Busca via HTTP GET na URL de pesquisa do YouTube. Parseamento de HTML para extrair o primeiro resultado.
    *   *Tratamento de Erros:* Timeout agressivo (5s), User-Agent rotativo simples se necessário. Em caso de falha, não retenta infinitamente.
- **EventCatalog Generator:** O container do EventCatalog deve ter acesso de rede aos containers das APIs para consumir os endpoints de documentação.

## Análise de Impacto

| Componente Afetado | Tipo de Impacto | Descrição & Nível de Risco | Ação Requerida |
| :--- | :--- | :--- | :--- |
| **Infraestrutura** | Novo Recurso | Necessidade de subir múltiplos containers (Rabbit, Postgres, Apicurio, EventCatalog). Risco Médio (Recursos de máquina). | Configurar limites de memória no Docker Compose. |
| **Frontend** | Integração | Necessidade de manter conexão SSE aberta. Risco Baixo. | Implementar reconexão automática no cliente SSE. |

## Abordagem de Testes

### Testes Unitários
- **Java:** JUnit 5 + Mockito. Testar Service Layer (regras de negócio) e Listeners (mapeamento de eventos).
- **.NET:** xUnit + Moq. Testar Consumer (lógica de consumo) e Scraper (parsing de HTML com mocks de resposta HTTP).

### Testes de Integração
- **Testcontainers:** Utilizar Testcontainers (Java e .NET) para subir RabbitMQ e Postgres efêmeros durante os testes de integração dos consumidores e produtores.
- **Fluxo:** Validar que ao salvar no banco, o evento é publicado. Validar que ao receber evento, o banco é atualizado.

## Sequenciamento de Desenvolvimento

### Ordem de Construção

1.  **Infraestrutura Base:** Criar `docker-compose.yml` com RabbitMQ, Postgres e Apicurio.
2.  **Music Service (Java) - Core:** Implementar CRUD básico e persistência.
3.  **Music Service (Java) - Producer:** Implementar publicação no RabbitMQ e documentação SpringWolf.
4.  **Video Enricher (.NET) - Consumer:** Implementar consumo da fila e documentação Neuroglia (AsyncAPI v3).
5.  **Video Enricher (.NET) - Scraper:** Implementar lógica de scraping e publicação de retorno.
6.  **Music Service (Java) - Consumer & SSE:** Implementar atualização do registro e notificação SSE.
7.  **Frontend:** Implementar telas e integração.
8.  **Governança:** Configurar EventCatalog com plugin `@eventcatalog/generator-asyncapi` apontando para os endpoints internos.

### Dependências Técnicas
- Imagens Docker oficiais para RabbitMQ, Postgres, Apicurio.
- Acesso à internet para o container .NET realizar o scraping.
- Node.js no container do EventCatalog para rodar o gerador.

## Monitoramento e Observabilidade

- **Logs:** Estruturados (JSON) em ambos os serviços.
    - Java: SLF4J/Logback.
    - .NET: Serilog.
- **Tracing:** Adicionar `correlationId` nos headers das mensagens RabbitMQ para rastrear o fluxo completo (Java -> .NET -> Java).
- **Health Checks:** Expor endpoints `/actuator/health` (Java) e `/health` (.NET) para o Docker Compose monitorar.

## Considerações Técnicas

### Decisões Principais
- **Scraping vs API:** Decidido pelo scraping para simplificar a PoC (sem chaves de API), aceitando o risco de fragilidade.
- **SSE vs WebSockets:** SSE escolhido por ser unidirecional (Server -> Client) e mais simples de implementar via HTTP padrão para este caso de uso.
- **AsyncAPI Code-First:** Uso de SpringWolf e Neuroglia para gerar documentação a partir do código, garantindo que a doc não fique obsoleta.
- **EventCatalog Integration:** Uso do plugin oficial via URL para garantir sincronia automática sem cópia manual de arquivos.

### Riscos Conhecidos
- **Bloqueio do YouTube:** O IP do container pode ser bloqueado pelo YouTube se houver muitas requisições. *Mitigação:* Apenas para fins didáticos, volume baixo esperado.
- **Complexidade do Docker Compose:** Subir toda a stack pode ser pesado para máquinas locais com pouca RAM.

### Conformidade com Padrões
- **Arquitetura:** Segue Event-Driven Architecture e Database per Service.
- **Interoperabilidade:** Uso de padrões abertos (AMQP, JSON, HTTP).
- **Documentação:** Adoção estrita de AsyncAPI para contratos de eventos.
