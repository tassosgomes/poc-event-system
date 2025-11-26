---
status: pending
parallelizable: false
blocked_by: ["2.0"]
---

<task_context>
<domain>backend/java</domain>
<type>implementation</type>
<scope>middleware</scope>
<complexity>high</complexity>
<dependencies>rabbitmq,springwolf</dependencies>
<unblocks>["6.0"]</unblocks>
</task_context>

# Tarefa 3.0: Implementação do Music Service (Java/Spring Boot) - Mensageria & AsyncAPI

## Visão Geral
Configurar a integração com RabbitMQ para publicar eventos de criação de música e configurar o SpringWolf para gerar a documentação AsyncAPI automaticamente.

## Requisitos
- Dependências: Spring AMQP (RabbitMQ), SpringWolf.
- Configuração de Exchange e Queue para `song.created`.
- Publicação de evento `SongCreatedEvent` após persistência bem-sucedida.
- Configuração do SpringWolf para expor documentação em `/springwolf/docs`.

## Subtarefas
- [x] 3.1 Adicionar dependências do RabbitMQ e SpringWolf no `pom.xml`/`build.gradle`.
- [x] 3.2 Configurar conexão RabbitMQ no `application.properties`.
- [x] 3.3 Criar classe de evento `SongCreatedEvent` (DTO).
- [x] 3.4 Implementar `EventPublisher` para enviar mensagem ao RabbitMQ.
- [x] 3.5 Integrar publicação do evento no fluxo de `createSong` do Service.
- [x] 3.6 Configurar SpringWolf (Docket/Configuration) para escanear produtores e gerar spec AsyncAPI.
- [x] 3.7 Adicionar anotações `@AsyncPublisher` (ou equivalentes do SpringWolf) no publisher.
- [x] 3.8 Testes de integração usando Testcontainers para validar publicação no RabbitMQ.

## Sequenciamento
- Bloqueado por: 2.0
- Desbloqueia: 6.0
- Paralelizável: Não

## Detalhes de Implementação
- O evento deve conter: `songId`, `title`, `artist`, `releaseDate`, `genre`.
- Garantir que a mensagem seja serializada em JSON.
- Endpoint do SpringWolf deve estar acessível e retornando JSON válido da spec AsyncAPI.

## Critérios de Sucesso
- [x] 3.0 Implementação do Music Service (Java/Spring Boot) - Mensageria & AsyncAPI ✅ CONCLUÍDA
  - [x] 3.1 Implementação completada
  - [x] 3.2 Definição da tarefa, PRD e tech spec validados
  - [x] 3.3 Análise de regras e conformidade verificadas
  - [x] 3.4 Revisão de código completada
  - [x] 3.5 Pronto para deploy
