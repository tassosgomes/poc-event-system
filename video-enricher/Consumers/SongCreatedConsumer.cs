using MassTransit;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using VideoEnricher.Domain.Event;

namespace VideoEnricher.Consumers
{
    public class SongCreatedConsumer : IConsumer<SongCreatedEvent>
    {
        private readonly ILogger<SongCreatedConsumer> _logger;

        public SongCreatedConsumer(ILogger<SongCreatedConsumer> logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<SongCreatedEvent> context)
        {
            _logger.LogInformation("Received SongCreatedEvent: SongId={SongId}, Title={Title}, Artist={Artist}", 
                context.Message.SongId, context.Message.Title, context.Message.Artist);
            
            // Logic to be implemented in next tasks
            return Task.CompletedTask;
        }
    }
}
