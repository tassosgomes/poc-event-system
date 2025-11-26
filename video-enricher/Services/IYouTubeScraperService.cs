using VideoEnricher.Domain;

namespace VideoEnricher.Services;

/// <summary>
/// Interface para o serviço de scraping do YouTube.
/// </summary>
public interface IYouTubeScraperService
{
    /// <summary>
    /// Busca um vídeo no YouTube com base no artista e título da música.
    /// </summary>
    /// <param name="artist">Nome do artista</param>
    /// <param name="title">Título da música</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Metadados do vídeo encontrado ou null se não encontrar</returns>
    Task<VideoScrapingResult?> SearchVideoAsync(string artist, string title, CancellationToken cancellationToken = default);
}

/// <summary>
/// Resultado do scraping do YouTube.
/// </summary>
public record VideoScrapingResult
{
    public required string VideoUrl { get; init; }
    public long Views { get; init; }
}
