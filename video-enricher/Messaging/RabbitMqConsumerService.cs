using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using VideoEnricher.Domain.Event;

namespace VideoEnricher.Messaging;

public class RabbitMqConsumerService : BackgroundService
{
    private readonly ILogger<RabbitMqConsumerService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RabbitMqSettings _settings;
    private IConnection? _connection;
    private IChannel? _channel;

    private const int MaximoTentativas = 10;
    private const int IntervaloTentativaMs = 5000;

    public RabbitMqConsumerService(
        ILogger<RabbitMqConsumerService> logger,
        IServiceScopeFactory scopeFactory,
        RabbitMqSettings settings)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _settings = settings;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await InicializarRabbitMqAsync(stoppingToken);

        if (_channel == null)
        {
            _logger.LogError("Falha ao inicializar canal do RabbitMQ");
            return;
        }

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var mensagem = Encoding.UTF8.GetString(body);
                
                _logger.LogInformation("Mensagem recebida do RabbitMQ: {Mensagem}", mensagem);

                var eventoMusicaCriada = JsonSerializer.Deserialize<SongCreatedEvent>(mensagem, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (eventoMusicaCriada != null)
                {
                    await ProcessarMensagemAsync(eventoMusicaCriada, stoppingToken);
                }

                await _channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar mensagem");
                await _channel.BasicNackAsync(ea.DeliveryTag, false, true, stoppingToken);
            }
        };

        await _channel.BasicConsumeAsync(
            queue: _settings.QueueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        _logger.LogInformation("Consumer RabbitMQ iniciado. Escutando fila: {Fila}", _settings.QueueName);

        // Manter o serviço em execução
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task InicializarRabbitMqAsync(CancellationToken cancellationToken)
    {
        var tentativas = 0;

        while (tentativas < MaximoTentativas)
        {
            try
            {
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

                // Declarar exchange
                await _channel.ExchangeDeclareAsync(
                    exchange: _settings.ExchangeName,
                    type: ExchangeType.Topic,
                    durable: true,
                    autoDelete: false,
                    cancellationToken: cancellationToken);

                // Declarar fila
                await _channel.QueueDeclareAsync(
                    queue: _settings.QueueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    cancellationToken: cancellationToken);

                // Vincular fila ao exchange
                await _channel.QueueBindAsync(
                    queue: _settings.QueueName,
                    exchange: _settings.ExchangeName,
                    routingKey: _settings.RoutingKey,
                    cancellationToken: cancellationToken);

                _logger.LogInformation("Conectado ao RabbitMQ em {Host}:{Port}", 
                    _settings.HostName, _settings.Port);
                return;
            }
            catch (Exception ex)
            {
                tentativas++;
                _logger.LogWarning(ex, "Falha ao conectar ao RabbitMQ. Tentativa {Tentativa}/{Max}", 
                    tentativas, MaximoTentativas);
                
                if (tentativas < MaximoTentativas)
                {
                    await Task.Delay(IntervaloTentativaMs, cancellationToken);
                }
            }
        }

        _logger.LogError("Falha ao conectar ao RabbitMQ após {Max} tentativas", MaximoTentativas);
    }

    private async Task ProcessarMensagemAsync(SongCreatedEvent eventoMusicaCriada, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processando SongCreatedEvent: SongId={SongId}, Title={Title}, Artist={Artist}, Genre={Genre}",
            eventoMusicaCriada.SongId,
            eventoMusicaCriada.Title,
            eventoMusicaCriada.Artist,
            eventoMusicaCriada.Genre);

        // Lógica a ser implementada nas próximas tarefas (scraping, etc.)
        await Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Parando Consumer RabbitMQ...");
        
        if (_channel != null)
        {
            await _channel.CloseAsync(cancellationToken);
        }
        
        if (_connection != null)
        {
            await _connection.CloseAsync(cancellationToken);
        }

        await base.StopAsync(cancellationToken);
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
}
