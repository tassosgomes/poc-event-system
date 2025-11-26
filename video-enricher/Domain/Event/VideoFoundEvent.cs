using System;

namespace VideoEnricher.Domain.Event;

/// <summary>
/// Evento publicado quando um vídeo é encontrado no YouTube.
/// </summary>
public record VideoFoundEvent
{
    /// <summary>
    /// ID da música relacionada.
    /// </summary>
    public Guid SongId { get; init; }
    
    /// <summary>
    /// URL do vídeo encontrado no YouTube.
    /// </summary>
    public string VideoUrl { get; init; } = string.Empty;
    
    /// <summary>
    /// Número de visualizações do vídeo.
    /// </summary>
    public long Views { get; init; }
}
