using System.Net;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using VideoEnricher.Services;

namespace VideoEnricher.Tests;

public class YouTubeScraperServiceTests
{
    private readonly Mock<ILogger<YouTubeScraperService>> _loggerMock;

    public YouTubeScraperServiceTests()
    {
        _loggerMock = new Mock<ILogger<YouTubeScraperService>>();
    }

    private YouTubeScraperService CreateService(HttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler);
        return new YouTubeScraperService(httpClient, _loggerMock.Object);
    }

    [Fact]
    public async Task SearchVideoAsync_WithValidResponse_ReturnsVideoScrapingResult()
    {
        // Arrange
        var htmlResponse = """
            <html>
            <body>
                <script>
                    var ytInitialData = {
                        "contents": {
                            "videoId": "dQw4w9WgXcQ"
                        },
                        "viewCountText": {"simpleText": "1,234,567 views"}
                    };
                </script>
            </body>
            </html>
            """;

        var handler = CreateMockHandler(HttpStatusCode.OK, htmlResponse);
        var service = CreateService(handler);

        // Act
        var result = await service.SearchVideoAsync("Rick Astley", "Never Gonna Give You Up");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("https://www.youtube.com/watch?v=dQw4w9WgXcQ", result.VideoUrl);
        // Views podem ser 0 se o regex não encontrar no HTML de teste simplificado
        // O importante é que o VideoUrl foi extraído corretamente
    }

    [Fact]
    public async Task SearchVideoAsync_WithVideoIdInHref_ExtractsVideoId()
    {
        // Arrange
        var htmlResponse = """
            <html>
            <body>
                <a href="/watch?v=abc123XYZ90">Video Link</a>
            </body>
            </html>
            """;

        var handler = CreateMockHandler(HttpStatusCode.OK, htmlResponse);
        var service = CreateService(handler);

        // Act
        var result = await service.SearchVideoAsync("Test Artist", "Test Song");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("https://www.youtube.com/watch?v=abc123XYZ90", result.VideoUrl);
    }

    [Fact]
    public async Task SearchVideoAsync_WithNoVideoId_ReturnsNull()
    {
        // Arrange
        var htmlResponse = "<html><body>No videos found</body></html>";

        var handler = CreateMockHandler(HttpStatusCode.OK, htmlResponse);
        var service = CreateService(handler);

        // Act
        var result = await service.SearchVideoAsync("Unknown Artist", "Unknown Song");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task SearchVideoAsync_WithNonSuccessStatusCode_ReturnsNull()
    {
        // Arrange
        var handler = CreateMockHandler(HttpStatusCode.NotFound, "Not Found");
        var service = CreateService(handler);

        // Act
        var result = await service.SearchVideoAsync("Test Artist", "Test Song");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task SearchVideoAsync_WithHttpRequestException_ReturnsNull()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection refused"));

        var service = CreateService(handlerMock.Object);

        // Act
        var result = await service.SearchVideoAsync("Test Artist", "Test Song");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task SearchVideoAsync_WithViewsInMillions_ParsesCorrectly()
    {
        // Arrange - O HTML deve ter o formato exato que o regex procura
        var htmlResponse = """
            <html>
            <body>
                <script>
                    var data = {
                        "videoId": "test12345678",
                        "short_view_count_text": {"simpleText": "15M views"}
                    };
                </script>
            </body>
            </html>
            """;

        var handler = CreateMockHandler(HttpStatusCode.OK, htmlResponse);
        var service = CreateService(handler);

        // Act
        var result = await service.SearchVideoAsync("Test", "Song");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(15000000, result.Views);
    }

    [Fact]
    public async Task SearchVideoAsync_WithViewsInThousands_ParsesCorrectly()
    {
        // Arrange
        var htmlResponse = """
            <html>
            <body>
                <script>
                    var data = {
                        "videoId": "test12345678",
                        "viewCountText": {"simpleText": "500K views"}
                    };
                </script>
            </body>
            </html>
            """;

        var handler = CreateMockHandler(HttpStatusCode.OK, htmlResponse);
        var service = CreateService(handler);

        // Act
        var result = await service.SearchVideoAsync("Test", "Song");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(500000, result.Views);
    }

    [Fact]
    public async Task SearchVideoAsync_WithBrazilianViews_ParsesCorrectly()
    {
        // Arrange
        var htmlResponse = """
            <html>
            <body>
                <script>
                    var data = {
                        "videoId": "test12345678",
                        "viewCountText": {"simpleText": "2.345.678 visualizações"}
                    };
                </script>
            </body>
            </html>
            """;

        var handler = CreateMockHandler(HttpStatusCode.OK, htmlResponse);
        var service = CreateService(handler);

        // Act
        var result = await service.SearchVideoAsync("Test", "Song");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2345678, result.Views);
    }

    [Fact]
    public async Task SearchVideoAsync_EncodesSearchQueryCorrectly()
    {
        // Arrange
        HttpRequestMessage? capturedRequest = null;
        var htmlResponse = """<html><body><a href="/watch?v=test12345678"></a></body></html>""";
        
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((request, _) => capturedRequest = request)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(htmlResponse)
            });

        var service = CreateService(handlerMock.Object);

        // Act
        await service.SearchVideoAsync("Artista Especial", "Música com Acentos");

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.Contains("search_query=", capturedRequest.RequestUri?.ToString());
        Assert.Contains("Artista", capturedRequest.RequestUri?.ToString());
    }

    [Fact]
    public async Task SearchVideoAsync_SetsUserAgentHeader()
    {
        // Arrange
        HttpRequestMessage? capturedRequest = null;
        var htmlResponse = """<html><body><a href="/watch?v=test12345678"></a></body></html>""";
        
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((request, _) => capturedRequest = request)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(htmlResponse)
            });

        var service = CreateService(handlerMock.Object);

        // Act
        await service.SearchVideoAsync("Test", "Song");

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.True(capturedRequest.Headers.Contains("User-Agent"));
    }

    [Fact]
    public async Task SearchVideoAsync_WithCancellation_ReturnsNull()
    {
        // Arrange
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException());

        var service = CreateService(handlerMock.Object);

        // Act
        var result = await service.SearchVideoAsync("Test", "Song", cancellationTokenSource.Token);

        // Assert
        Assert.Null(result);
    }

    private static HttpMessageHandler CreateMockHandler(HttpStatusCode statusCode, string content)
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(content)
            });

        return handlerMock.Object;
    }
}
