using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using VideoEnricher.Domain.Event;

namespace VideoEnricher.Messaging;

/// <summary>
/// Serviço para publicação de eventos no RabbitMQ.
/// </summary>
public class RabbitMqPublisherService : IRabbitMqPublisherService, IAsyncDisposable
{
    private readonly ILogger<RabbitMqPublisherService> _logger;
    private readonly RabbitMqSettings _settings;
    private IConnection? _connection;
    private IChannel? _channel;
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private bool _initialized;

    private const string VideoFoundRoutingKey = "video.found";
    private const string VideoFoundQueueName = "music.video-found";

    public RabbitMqPublisherService(
        ILogger<RabbitMqPublisherService> logger,
        RabbitMqSettings settings)
    {
        _logger = logger;
        _settings = settings;
    }

    public async Task PublishVideoFoundEventAsync(VideoFoundEvent videoFoundEvent, CancellationToken cancellationToken = default)
    {
        await EnsureInitializedAsync(cancellationToken);

        if (_channel == null)
        {
            _logger.LogError("Canal RabbitMQ não inicializado");
            throw new InvalidOperationException("Canal RabbitMQ não disponível");
        }

        try
        {
            var json = JsonSerializer.Serialize(videoFoundEvent, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            
            var body = Encoding.UTF8.GetBytes(json);

            var properties = new BasicProperties
            {
                ContentType = "application/json",
                DeliveryMode = DeliveryModes.Persistent,
                MessageId = Guid.NewGuid().ToString(),
                Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            };

            await _channel.BasicPublishAsync(
                exchange: _settings.ExchangeName,
                routingKey: VideoFoundRoutingKey,
                mandatory: false,
                basicProperties: properties,
                body: body,
                cancellationToken: cancellationToken);

            _logger.LogInformation(
                "VideoFoundEvent publicado: SongId={SongId}, VideoUrl={VideoUrl}, Views={Views}",
                videoFoundEvent.SongId,
                videoFoundEvent.VideoUrl,
                videoFoundEvent.Views);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao publicar VideoFoundEvent para SongId={SongId}", videoFoundEvent.SongId);
            throw;
        }
    }

    private async Task EnsureInitializedAsync(CancellationToken cancellationToken)
    {
        if (_initialized) return;

        await _initLock.WaitAsync(cancellationToken);
        try
        {
            if (_initialized) return;

            var factory = new ConnectionFactory
            {
                HostName = _settings.HostName,
                Port = _settings.Port,
                UserName = _settings.UserName,
                Password = _settings.Password,
                VirtualHost = _settings.VirtualHost
            };

            _connection = await factory.CreateConnectionAsync(cancellationToken);
            _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

            // Declarar exchange (idempotente)
            await _channel.ExchangeDeclareAsync(
                exchange: _settings.ExchangeName,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false,
                cancellationToken: cancellationToken);

            // Declarar fila para video.found
            await _channel.QueueDeclareAsync(
                queue: VideoFoundQueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                cancellationToken: cancellationToken);

            // Vincular fila ao exchange
            await _channel.QueueBindAsync(
                queue: VideoFoundQueueName,
                exchange: _settings.ExchangeName,
                routingKey: VideoFoundRoutingKey,
                cancellationToken: cancellationToken);

            _initialized = true;
            _logger.LogInformation("RabbitMQ Publisher inicializado com sucesso");
        }
        finally
        {
            _initLock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel != null)
        {
            await _channel.CloseAsync();
            _channel.Dispose();
        }

        if (_connection != null)
        {
            await _connection.CloseAsync();
            _connection.Dispose();
        }

        _initLock.Dispose();
        GC.SuppressFinalize(this);
    }
}
