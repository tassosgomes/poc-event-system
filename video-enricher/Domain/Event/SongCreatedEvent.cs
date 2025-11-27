using System;
using System.Text.Json.Serialization;
using VideoEnricher.Converters;

namespace VideoEnricher.Domain.Event
{
    public record SongCreatedEvent
    {
        public Guid SongId { get; init; }
        public string Title { get; init; } = string.Empty;
        public string Artist { get; init; } = string.Empty;
        
        [JsonConverter(typeof(DateArrayToStringConverter))]
        public string ReleaseDate { get; init; } = string.Empty; // Keeping as string for simplicity with ISO8601, or could be DateTime
        
        public string Genre { get; init; } = string.Empty;
    }
}
