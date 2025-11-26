namespace VideoEnricher.Messaging;

public class RabbitMqSettings
{
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
    public string ExchangeName { get; set; } = "music.events";
    public string QueueName { get; set; } = "music.song-created";
    public string RoutingKey { get; set; } = "song.created";
}
