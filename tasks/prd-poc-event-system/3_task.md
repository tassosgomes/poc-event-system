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
- [ ] 3.1 Adicionar dependências do RabbitMQ e SpringWolf no `pom.xml`/`build.gradle`.
- [ ] 3.2 Configurar conexão RabbitMQ no `application.properties`.
- [ ] 3.3 Criar classe de evento `SongCreatedEvent` (DTO).
- [ ] 3.4 Implementar `EventPublisher` para enviar mensagem ao RabbitMQ.
- [ ] 3.5 Integrar publicação do evento no fluxo de `createSong` do Service.
- [ ] 3.6 Configurar SpringWolf (Docket/Configuration) para escanear produtores e gerar spec AsyncAPI.
- [ ] 3.7 Adicionar anotações `@AsyncPublisher` (ou equivalentes do SpringWolf) no publisher.
- [ ] 3.8 Testes de integração usando Testcontainers para validar publicação no RabbitMQ.

## Sequenciamento
- Bloqueado por: 2.0
- Desbloqueia: 6.0
- Paralelizável: Não

## Detalhes de Implementação
- O evento deve conter: `songId`, `title`, `artist`, `releaseDate`, `genre`.
- Garantir que a mensagem seja serializada em JSON.
- Endpoint do SpringWolf deve estar acessível e retornando JSON válido da spec AsyncAPI.

## Critérios de Sucesso
- Ao criar uma música, uma mensagem é publicada na fila `music.song-created` (ou exchange configurada).
- Endpoint `/springwolf/docs` retorna a documentação AsyncAPI correta descrevendo o evento `song.created`.
- Testes de integração com Testcontainers passam.
