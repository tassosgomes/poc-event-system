---
status: done
parallelizable: false
blocked_by: ["3.0", "5.0"]
---

<task_context>
<domain>backend/java</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>medium</complexity>
<dependencies>rabbitmq,sse</dependencies>
<unblocks>["7.0"]</unblocks>
</task_context>

# Tarefa 6.0: Implementação do Music Service (Java/Spring Boot) - Consumo & SSE

## Visão Geral
Implementar o consumo do evento de retorno (`VideoFoundEvent`) no serviço Java, atualizar o banco de dados e notificar os clientes conectados via Server-Sent Events (SSE).

## Requisitos
- Consumidor RabbitMQ para `VideoFoundEvent`.
- Atualização da entidade `Song` (URL, Views, Status=COMPLETED).
- Implementação de SSE (`SseEmitter`) para push de atualizações.

## Subtarefas
- [x] 6.1 Criar DTO `VideoFoundEvent` no projeto Java. ✅
- [x] 6.2 Implementar `VideoFoundListener` (`@RabbitListener`). ✅
- [x] 6.3 Atualizar `SongService` para processar o evento e atualizar o DB. ✅
- [x] 6.4 Implementar gerenciador de sessões SSE (lista de Emitters ativos). ✅
- [x] 6.5 Criar endpoint `/songs/stream` no Controller. ✅
- [x] 6.6 Disparar evento SSE para todos os clientes conectados quando uma música for atualizada. ✅
- [x] 6.7 Atualizar documentação SpringWolf para incluir o Listener (consumidor). ✅

## Sequenciamento
- Bloqueado por: 3.0, 5.0 (depende do contrato do evento definido no .NET)
- Desbloqueia: 7.0
- Paralelizável: Não

## Detalhes de Implementação
- SSE: Tratar timeouts e desconexões de clientes (remover da lista).
- Garantir transacionalidade na atualização do banco.

## Critérios de Sucesso
- Fluxo completo (Round-trip): Criação -> Java -> Rabbit -> .NET -> Rabbit -> Java -> DB Atualizado.
- Cliente conectado em `/songs/stream` recebe JSON com dados atualizados da música em tempo real.
