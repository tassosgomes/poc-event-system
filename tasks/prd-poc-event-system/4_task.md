---
status: done
parallelizable: true
blocked_by: ["1.0"]
---

<task_context>
<domain>backend/dotnet</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>medium</complexity>
<dependencies>database,rabbitmq</dependencies>
<unblocks>["5.0"]</unblocks>
</task_context>

# Tarefa 4.0: Implementação do Video Enricher Service (.NET) - Core & Mensageria

## Visão Geral
Inicializar o projeto .NET para o Video Enricher Service, configurar acesso a dados (EF Core) e consumo de mensagens do RabbitMQ (MassTransit ou cliente nativo).

## Requisitos
- Projeto .NET 8+ (Web API ou Worker Service).
- Dependências: EF Core, Npgsql (Postgres), MassTransit (recomendado para RabbitMQ) ou RabbitMQ.Client.
- Entidade `VideoMetadata` para persistência local.
- Consumidor para `SongCreatedEvent`.

## Subtarefas
- [x] 4.1 Inicializar projeto .NET (`dotnet new webapi` ou `worker`) no diretório `video-enricher`. ✅
- [x] 4.2 Configurar EF Core com PostgreSQL (Connection String apontando para container da task 1.0). ✅
- [x] 4.3 Criar entidade `VideoMetadata` e Migrations. ✅
- [x] 4.4 Configurar RabbitMQ.Client para conectar ao RabbitMQ. ✅
- [x] 4.5 Criar classe de evento `SongCreatedEvent` (contrato idêntico ao Java). ✅
- [x] 4.6 Implementar `RabbitMqConsumerService` para ler mensagens da fila. ✅
- [x] 4.7 Adicionar Dockerfile para o serviço .NET. ✅

## Sequenciamento
- Bloqueado por: 1.0
- Desbloqueia: 5.0
- Paralelizável: Sim (com 2.0 e 3.0)

## Detalhes de Implementação
- O consumidor deve ser capaz de deserializar a mensagem JSON enviada pelo Java.
- Inicialmente, o consumidor pode apenas logar a mensagem recebida para validar a conexão.

## Critérios de Sucesso
- Aplicação .NET sobe e conecta ao banco e ao RabbitMQ.
- Migrations do banco aplicadas com sucesso.
- Aplicação recebe e loga mensagens enviadas pelo Music Service (teste de integração manual ou automatizado).
