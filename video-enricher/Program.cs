using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Neuroglia.AsyncApi;
using Neuroglia.AsyncApi.FluentBuilders;
using Neuroglia.AsyncApi.v3;
using VideoEnricher.Data;
using VideoEnricher.Messaging;
using VideoEnricher.Services;

var builder = WebApplication.CreateBuilder(args);

// Configurar DbContext com PostgreSQL
builder.Services.AddDbContext<VideoEnricherDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configurar RabbitMQ a partir do appsettings.json
var rabbitMqSettings = builder.Configuration.GetSection("RabbitMq").Get<RabbitMqSettings>() 
    ?? new RabbitMqSettings();
builder.Services.AddSingleton(rabbitMqSettings);

// Registrar serviços de mensageria
builder.Services.AddSingleton<IRabbitMqPublisherService, RabbitMqPublisherService>();
builder.Services.AddHostedService<RabbitMqConsumerService>();

// Registrar HttpClient e YouTube Scraper Service
builder.Services.AddHttpClient<IYouTubeScraperService, YouTubeScraperService>();

// Configurar AsyncAPI com Neuroglia
builder.Services.AddAsyncApi();

// Adicionar health checks
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!);

builder.Services.AddOpenApi();

var app = builder.Build();

// Configurar pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Health check endpoint
app.MapHealthChecks("/health");

// Endpoint para documentação AsyncAPI v3
app.MapGet("/asyncapi/docs", (IAsyncApiDocumentBuilder documentBuilder) =>
{
    // Schema para SongCreatedEvent
    var songCreatedSchema = new JsonObject
    {
        ["type"] = "object",
        ["properties"] = new JsonObject
        {
            ["songId"] = new JsonObject { ["type"] = "string", ["format"] = "uuid" },
            ["title"] = new JsonObject { ["type"] = "string" },
            ["artist"] = new JsonObject { ["type"] = "string" },
            ["releaseDate"] = new JsonObject { ["type"] = "string", ["format"] = "date" },
            ["genre"] = new JsonObject { ["type"] = "string" }
        },
        ["required"] = new JsonArray { "songId", "title", "artist" }
    };

    // Schema para VideoFoundEvent
    var videoFoundSchema = new JsonObject
    {
        ["type"] = "object",
        ["properties"] = new JsonObject
        {
            ["songId"] = new JsonObject { ["type"] = "string", ["format"] = "uuid" },
            ["videoUrl"] = new JsonObject { ["type"] = "string", ["format"] = "uri" },
            ["views"] = new JsonObject { ["type"] = "integer", ["format"] = "int64" }
        },
        ["required"] = new JsonArray { "songId", "videoUrl", "views" }
    };

    var document = documentBuilder
        .UsingAsyncApiV3()
        .WithTitle("Video Enricher API")
        .WithVersion("1.0.0")
        .WithDescription("API de enriquecimento de vídeos que consome eventos de músicas criadas e publica eventos de vídeos encontrados no YouTube")
        .WithTermsOfService(new Uri("https://github.com/tassosgomes/poc-event-system"))
        .WithLicense("Apache 2.0", new Uri("https://www.apache.org/licenses/LICENSE-2.0"))
        .WithServer("rabbitmq", server => server
            .WithHost("rabbitmq:5672")
            .WithProtocol(AsyncApiProtocol.Amqp, "0.9.1")
            .WithDescription("RabbitMQ Message Broker para comunicação assíncrona entre serviços"))
        .WithChannel("songCreated", channel => channel
            .WithServer("#/servers/rabbitmq")
            .WithAddress("music.song-created")
            .WithDescription("Canal para receber eventos de músicas criadas pelo Music Service"))
        .WithChannel("videoFound", channel => channel
            .WithServer("#/servers/rabbitmq")
            .WithAddress("music.video-found")
            .WithDescription("Canal para publicar eventos quando um vídeo é encontrado no YouTube"))
        .WithOperation("receiveSongCreated", operation => operation
            .WithAction(V3OperationAction.Receive)
            .WithChannel("#/channels/songCreated")
            .WithTitle("Receber Música Criada")
            .WithDescription("Recebe evento quando uma nova música é cadastrada no Music Service")
            .WithMessage("#/components/messages/SongCreatedEvent"))
        .WithOperation("sendVideoFound", operation => operation
            .WithAction(V3OperationAction.Send)
            .WithChannel("#/channels/videoFound")
            .WithTitle("Publicar Vídeo Encontrado")
            .WithDescription("Publica evento quando um vídeo é encontrado no YouTube para uma música")
            .WithMessage("#/components/messages/VideoFoundEvent"))
        .WithMessageComponent("SongCreatedEvent", message => message
            .WithName("SongCreatedEvent")
            .WithTitle("Evento de Música Criada")
            .WithContentType("application/json")
            .WithPayloadSchema(schema => schema
                .WithFormat("application/schema+json")
                .WithSchema(songCreatedSchema))
            .WithDescription("Evento emitido pelo Music Service quando uma nova música é cadastrada"))
        .WithMessageComponent("VideoFoundEvent", message => message
            .WithName("VideoFoundEvent")
            .WithTitle("Evento de Vídeo Encontrado")
            .WithContentType("application/json")
            .WithPayloadSchema(schema => schema
                .WithFormat("application/schema+json")
                .WithSchema(videoFoundSchema))
            .WithDescription("Evento emitido quando um vídeo do YouTube é encontrado e associado a uma música"))
        .Build();

    return Results.Ok(document);
}).WithName("AsyncApiDocs")
  .WithTags("AsyncAPI")
  .WithDescription("Retorna a documentação AsyncAPI v3 do Video Enricher Service");

// Aplicar migrations automaticamente
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<VideoEnricherDbContext>();
    db.Database.Migrate();
}

app.Run();
