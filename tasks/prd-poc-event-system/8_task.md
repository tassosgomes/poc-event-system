---
status: pending
parallelizable: false
blocked_by: ["3.0", "5.0", "7.0"]
---

<task_context>
<domain>documentation</domain>
<type>integration</type>
<scope>governance</scope>
<complexity>medium</complexity>
<dependencies>eventcatalog,docker</dependencies>
<unblocks>[]</unblocks>
</task_context>

# Tarefa 8.0: Configuração e Integração do EventCatalog

## Visão Geral
Configurar o EventCatalog para gerar um portal de documentação unificado, consumindo as definições AsyncAPI expostas pelos serviços Java e .NET.

## Requisitos
- Projeto EventCatalog.
- Plugin `@eventcatalog/generator-asyncapi`.
- Configuração apontando para endpoints internos do Docker.

## Subtarefas
- [ ] 8.1 Inicializar projeto EventCatalog.
- [ ] 8.2 Instalar plugin `@eventcatalog/generator-asyncapi`.
- [ ] 8.3 Configurar `eventcatalog.config.js` com as URLs dos serviços (ex: `http://music-service:8080/springwolf/docs` e `http://video-enricher:80/asyncapi/docs`).
- [ ] 8.4 Criar script de geração/build no `package.json`.
- [ ] 8.5 Adicionar serviço EventCatalog no `docker-compose.yml`.
- [ ] 8.6 Garantir ordem de inicialização (EventCatalog deve rodar geração após serviços estarem up, ou usar script de wait).

## Sequenciamento
- Bloqueado por: 3.0, 5.0 (Endpoints de doc precisam existir), 7.0 (Finalização da stack)
- Desbloqueia: Entrega Final
- Paralelizável: Não

## Detalhes de Implementação
- O container do EventCatalog precisará de Node.js.
- Pode ser necessário um script de entrypoint que aguarda os serviços backend estarem saudáveis antes de rodar o gerador do EventCatalog.

## Critérios de Sucesso
- Portal EventCatalog acessível via navegador.
- Documentação exibe corretamente os eventos `song.created` e `video.found`.
- Diagramas de arquitetura/serviços gerados automaticamente pelo EventCatalog.
