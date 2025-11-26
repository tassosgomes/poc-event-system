using VideoEnricher.Domain.Event;

namespace VideoEnricher.Messaging;

/// <summary>
/// Interface para publicação de eventos no RabbitMQ.
/// </summary>
public interface IRabbitMqPublisherService
{
    /// <summary>
    /// Publica o evento VideoFoundEvent no RabbitMQ.
    /// </summary>
    /// <param name="videoFoundEvent">Evento a ser publicado</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    Task PublishVideoFoundEventAsync(VideoFoundEvent videoFoundEvent, CancellationToken cancellationToken = default);
}
