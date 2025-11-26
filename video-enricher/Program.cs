using Microsoft.EntityFrameworkCore;
using VideoEnricher.Data;
using VideoEnricher.Messaging;

var builder = WebApplication.CreateBuilder(args);

// Configurar DbContext com PostgreSQL
builder.Services.AddDbContext<VideoEnricherDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configurar RabbitMQ a partir do appsettings.json
var rabbitMqSettings = builder.Configuration.GetSection("RabbitMq").Get<RabbitMqSettings>() 
    ?? new RabbitMqSettings();
builder.Services.AddSingleton(rabbitMqSettings);
builder.Services.AddHostedService<RabbitMqConsumerService>();

// Adicionar health checks
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!);

builder.Services.AddOpenApi();

var app = builder.Build();

// Configurar pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Health check endpoint
app.MapHealthChecks("/health");

// Aplicar migrations automaticamente
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<VideoEnricherDbContext>();
    db.Database.Migrate();
}

app.Run();
