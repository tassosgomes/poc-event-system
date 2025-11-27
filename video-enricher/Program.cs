using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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

JsonObject CloneSchema(JsonObject schema) => JsonNode.Parse(schema.ToJsonString())!.AsObject();

JsonObject BuildMessage(string name, string title, string description, JsonObject schema) =>
    new()
    {
        ["name"] = name,
        ["title"] = title,
        ["summary"] = description,
        ["description"] = description,
        ["contentType"] = "application/json",
        ["payload"] = new JsonObject
        {
            ["schemaFormat"] = "application/schema+json",
            ["schema"] = CloneSchema(schema)
        }
    };

// Endpoint para documentação AsyncAPI v3
app.MapGet("/asyncapi/docs", () =>
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

    var videoFoundMessage = BuildMessage(
        "VideoFoundEvent",
        "Evento de Vídeo Encontrado",
        "Evento emitido quando um vídeo do YouTube é encontrado e associado a uma música",
        videoFoundSchema);

    var songCreatedMessage = BuildMessage(
        "SongCreatedEvent",
        "Evento de Música Criada",
        "Evento emitido pelo Music Service quando uma nova música é cadastrada",
        songCreatedSchema);

    var asyncApiDocument = new JsonObject
    {
        ["asyncapi"] = "3.0.0",
        ["id"] = "urn:com:poc-event-system:video-enricher",
        ["info"] = new JsonObject
        {
            ["title"] = "Video Enricher API",
            ["version"] = "1.0.0",
            ["summary"] = "Documentação AsyncAPI do serviço de enriquecimento de vídeos",
            ["description"] = "API de enriquecimento de vídeos que consome eventos de músicas criadas e publica eventos de vídeos encontrados no YouTube",
            ["termsOfService"] = "https://github.com/tassosgomes/poc-event-system",
            ["license"] = new JsonObject
            {
                ["name"] = "Apache 2.0",
                ["url"] = "https://www.apache.org/licenses/LICENSE-2.0"
            }
        },
        ["defaultContentType"] = "application/json",
        ["servers"] = new JsonObject
        {
            ["rabbitmq"] = new JsonObject
            {
                ["host"] = "rabbitmq:5672",
                ["protocol"] = "amqp",
                ["protocolVersion"] = "0.9.1",
                ["description"] = "RabbitMQ Message Broker para comunicação assíncrona entre serviços",
                ["bindings"] = new JsonObject()
            }
        },
        ["channels"] = new JsonObject
        {
            ["songCreated"] = new JsonObject
            {
                ["address"] = "music.song-created",
                ["description"] = "Canal para receber eventos de músicas criadas pelo Music Service",
                ["servers"] = new JsonArray { "rabbitmq" },
                ["messages"] = new JsonObject
                {
                    ["SongCreatedEvent"] = new JsonObject
                    {
                        ["$ref"] = "#/components/messages/SongCreatedEvent"
                    }
                }
            },
            ["videoFound"] = new JsonObject
            {
                ["address"] = "music.video-found",
                ["description"] = "Canal para publicar eventos quando um vídeo é encontrado no YouTube",
                ["servers"] = new JsonArray { "rabbitmq" },
                ["messages"] = new JsonObject
                {
                    ["VideoFoundEvent"] = new JsonObject
                    {
                        ["$ref"] = "#/components/messages/VideoFoundEvent"
                    }
                }
            }
        },
        ["operations"] = new JsonObject
        {
            ["receiveSongCreated"] = new JsonObject
            {
                ["action"] = "receive",
                ["summary"] = "Recebe evento de música criada",
                ["description"] = "Recebe evento quando uma nova música é cadastrada no Music Service",
                ["channel"] = "songCreated",
                ["messages"] = new JsonArray
                {
                    new JsonObject { ["$ref"] = "#/components/messages/SongCreatedEvent" }
                }
            },
            ["sendVideoFound"] = new JsonObject
            {
                ["action"] = "send",
                ["summary"] = "Publica evento de vídeo encontrado",
                ["description"] = "Publica evento quando um vídeo é encontrado no YouTube para uma música",
                ["channel"] = "videoFound",
                ["messages"] = new JsonArray
                {
                    new JsonObject { ["$ref"] = "#/components/messages/VideoFoundEvent" }
                }
            }
        },
        ["components"] = new JsonObject
        {
            ["messages"] = new JsonObject
            {
                ["SongCreatedEvent"] = songCreatedMessage,
                ["VideoFoundEvent"] = videoFoundMessage
            }
        }
    };

    return Results.Json(asyncApiDocument);
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
