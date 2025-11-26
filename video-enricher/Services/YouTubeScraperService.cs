using System.Net;
using System.Text.RegularExpressions;

namespace VideoEnricher.Services;

/// <summary>
/// Serviço de scraping do YouTube para buscar vídeos e extrair metadados.
/// </summary>
public partial class YouTubeScraperService : IYouTubeScraperService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<YouTubeScraperService> _logger;
    
    private const string YouTubeSearchBaseUrl = "https://www.youtube.com/results";
    private const int TimeoutSeconds = 10;

    public YouTubeScraperService(HttpClient httpClient, ILogger<YouTubeScraperService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        
        // Configurar timeout
        _httpClient.Timeout = TimeSpan.FromSeconds(TimeoutSeconds);
    }

    public async Task<VideoScrapingResult?> SearchVideoAsync(string artist, string title, CancellationToken cancellationToken = default)
    {
        try
        {
            var searchQuery = $"{artist} {title}";
            var encodedQuery = WebUtility.UrlEncode(searchQuery);
            var searchUrl = $"{YouTubeSearchBaseUrl}?search_query={encodedQuery}";

            _logger.LogInformation("Buscando vídeo no YouTube: {Query}", searchQuery);

            using var request = new HttpRequestMessage(HttpMethod.Get, searchUrl);
            
            // Adicionar User-Agent para evitar bloqueios
            request.Headers.Add("User-Agent", 
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
            request.Headers.Add("Accept-Language", "pt-BR,pt;q=0.9,en-US;q=0.8,en;q=0.7");

            var response = await _httpClient.SendAsync(request, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Resposta não-sucesso do YouTube: {StatusCode}", response.StatusCode);
                return null;
            }

            var html = await response.Content.ReadAsStringAsync(cancellationToken);
            
            return ExtractVideoData(html);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogWarning("Timeout ao buscar vídeo no YouTube para: {Artist} - {Title}", artist, title);
            return null;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Erro HTTP ao buscar vídeo no YouTube para: {Artist} - {Title}", artist, title);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao buscar vídeo no YouTube para: {Artist} - {Title}", artist, title);
            return null;
        }
    }

    private VideoScrapingResult? ExtractVideoData(string html)
    {
        try
        {
            // Extrair o primeiro ID de vídeo do HTML
            // O YouTube retorna dados em formato JSON embutido no HTML
            var videoId = ExtractVideoId(html);
            
            if (string.IsNullOrEmpty(videoId))
            {
                _logger.LogWarning("Nenhum ID de vídeo encontrado no HTML");
                return null;
            }

            var videoUrl = $"https://www.youtube.com/watch?v={videoId}";
            
            // Tentar extrair views (pode não estar disponível na página de resultados)
            var views = ExtractViewCount(html, videoId);

            _logger.LogInformation("Vídeo encontrado: {VideoUrl} com {Views} views", videoUrl, views);

            return new VideoScrapingResult
            {
                VideoUrl = videoUrl,
                Views = views
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao extrair dados do HTML do YouTube");
            return null;
        }
    }

    private string? ExtractVideoId(string html)
    {
        // Padrão 1: Buscar videoId em JSON (ytInitialData)
        var jsonVideoIdMatch = VideoIdJsonRegex().Match(html);
        if (jsonVideoIdMatch.Success)
        {
            return jsonVideoIdMatch.Groups[1].Value;
        }

        // Padrão 2: Buscar em href="/watch?v=..."
        var hrefMatch = VideoIdHrefRegex().Match(html);
        if (hrefMatch.Success)
        {
            return hrefMatch.Groups[1].Value;
        }

        // Padrão 3: Buscar videoId em qualquer contexto JSON
        var genericMatch = VideoIdGenericRegex().Match(html);
        if (genericMatch.Success)
        {
            return genericMatch.Groups[1].Value;
        }

        return null;
    }

    private long ExtractViewCount(string html, string videoId)
    {
        try
        {
            // Tentar encontrar contagem de views próximo ao videoId
            // O YouTube usa formatos como "1.234.567 views" ou "1,234,567 views" ou "1M views"
            
            // Padrão para views no JSON: "viewCountText":{"simpleText":"1,234,567 views"}
            var viewCountMatch = ViewCountJsonRegex().Match(html);
            if (viewCountMatch.Success)
            {
                var viewText = viewCountMatch.Groups[1].Value;
                return ParseViewCount(viewText);
            }

            // Padrão alternativo: "short_view_count_text":{"simpleText":"1.2M views"}
            var shortViewMatch = ShortViewCountRegex().Match(html);
            if (shortViewMatch.Success)
            {
                var viewText = shortViewMatch.Groups[1].Value;
                return ParseViewCount(viewText);
            }

            return 0;
        }
        catch
        {
            return 0;
        }
    }

    private static long ParseViewCount(string viewText)
    {
        // Remover texto não numérico exceto pontuação e sufixos
        var cleanText = viewText.ToUpperInvariant()
            .Replace("VIEWS", "")
            .Replace("VIEW", "")
            .Replace("VISUALIZAÇÕES", "")
            .Replace("VISUALIZAÇÃO", "")
            .Trim();

        // Verificar se tem sufixo (K, M, B)
        double multiplier = 1;
        if (cleanText.EndsWith('K'))
        {
            multiplier = 1_000;
            cleanText = cleanText[..^1];
        }
        else if (cleanText.EndsWith('M'))
        {
            multiplier = 1_000_000;
            cleanText = cleanText[..^1];
        }
        else if (cleanText.EndsWith('B') || cleanText.EndsWith("BI"))
        {
            multiplier = 1_000_000_000;
            cleanText = cleanText.TrimEnd('B', 'I');
        }

        // Remover separadores e converter
        cleanText = cleanText.Replace(".", "").Replace(",", ".").Trim();
        
        if (double.TryParse(cleanText, System.Globalization.NumberStyles.Any, 
            System.Globalization.CultureInfo.InvariantCulture, out var number))
        {
            return (long)(number * multiplier);
        }

        return 0;
    }

    // Regex compilados para melhor performance
    [GeneratedRegex(@"""videoId""\s*:\s*""([a-zA-Z0-9_-]{11})""")]
    private static partial Regex VideoIdJsonRegex();

    [GeneratedRegex(@"href=""/watch\?v=([a-zA-Z0-9_-]{11})""")]
    private static partial Regex VideoIdHrefRegex();

    [GeneratedRegex(@"videoId['""]?\s*[=:]\s*['""]?([a-zA-Z0-9_-]{11})")]
    private static partial Regex VideoIdGenericRegex();

    [GeneratedRegex(@"""viewCountText""\s*:\s*\{[^}]*""simpleText""\s*:\s*""([^""]+)""")]
    private static partial Regex ViewCountJsonRegex();

    [GeneratedRegex(@"""short_view_count_text""\s*:\s*\{[^}]*""simpleText""\s*:\s*""([^""]+)""")]
    private static partial Regex ShortViewCountRegex();
}
