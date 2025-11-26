---
status: done
parallelizable: false
blocked_by: ["4.0"]
---

<task_context>
<domain>backend/dotnet</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>high</complexity>
<dependencies>external_apis,neuroglia</dependencies>
<unblocks>["6.0"]</unblocks>
</task_context>

# Tarefa 5.0: Implementação do Video Enricher Service (.NET) - Scraping & AsyncAPI

## Visão Geral
Implementar a lógica de scraping do YouTube, persistência do vídeo encontrado, publicação do evento de conclusão e geração de documentação AsyncAPI v3 com Neuroglia.

## Requisitos
- Serviço de Scraping (HttpClient + HTML Parser).
- Publicação de evento `VideoFoundEvent`.
- Integração com Neuroglia.AsyncAPI para documentação Code-First.

## Subtarefas
- [x] 5.1 Implementar `YouTubeScraperService` (busca HTML e extrai link/views). ✅
- [x] 5.2 Atualizar `SongCreatedConsumer` para chamar o Scraper. ✅
- [x] 5.3 Persistir resultado no banco de dados do .NET. ✅
- [x] 5.4 Criar evento `VideoFoundEvent` e publicar no RabbitMQ após sucesso. ✅
- [x] 5.5 Configurar Neuroglia.AsyncAPI para gerar documentação v3 baseada no código. ✅
- [x] 5.6 Expor endpoint `/asyncapi/docs` com a spec gerada. ✅
- [x] 5.7 Testes unitários para o Scraper (mockando resposta HTTP). ✅

## Sequenciamento
- Bloqueado por: 4.0
- Desbloqueia: 6.0
- Paralelizável: Não

## Detalhes de Implementação
- Scraping: Buscar por `https://www.youtube.com/results?search_query={artist}+{title}`.
- Extrair primeiro `href` de vídeo e contagem de views (se possível via regex ou parser leve).
- Neuroglia: Usar a abordagem fluente ou atributos para definir canais e operações.

## Critérios de Sucesso
- Ao receber `SongCreatedEvent`, o sistema busca no YouTube, salva no banco e publica `VideoFoundEvent`.
- Endpoint `/asyncapi/docs` retorna JSON válido da spec AsyncAPI v3.
- Scraping funciona para casos de teste padrão.
