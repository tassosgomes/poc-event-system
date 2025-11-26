---
status: completed
parallelizable: false
blocked_by: []
---

<task_context>
<domain>infra</domain>
<type>configuration</type>
<scope>configuration</scope>
<complexity>medium</complexity>
<dependencies>docker</dependencies>
<unblocks>["2.0", "4.0"]</unblocks>
</task_context>

# Tarefa 1.0: Configuração da Infraestrutura Base (Docker Compose)

## Visão Geral
Configurar o ambiente de desenvolvimento local utilizando Docker Compose para orquestrar todos os serviços de infraestrutura necessários: RabbitMQ, PostgreSQL (duas instâncias ou bancos), e Apicurio Registry.

## Requisitos
- Criar arquivo `docker-compose.yml` na raiz do projeto.
- Configurar RabbitMQ com Management Plugin habilitado.
- Configurar PostgreSQL para o Music Service (Java).
- Configurar PostgreSQL para o Video Enricher Service (.NET).
- Configurar Apicurio Registry (versão compatível com Kafka/RabbitMQ schemas se aplicável, ou apenas registry genérico).
- Definir redes e volumes para persistência de dados.

## Subtarefas
- [x] 1.1 Definir serviço RabbitMQ (imagem `rabbitmq:3-management`) com portas 5672 e 15672 expostas.
- [x] 1.2 Definir serviço PostgreSQL para Music Service (porta 5432 mapeada ou interna).
- [x] 1.3 Definir serviço PostgreSQL para Video Enricher Service (porta 5433 mapeada ou interna).
- [x] 1.4 Definir serviço Apicurio Registry (imagem `apicurio/apicurio-registry-mem` para dev ou com persistência).
- [x] 1.5 Configurar healthchecks para os serviços de infra.
- [x] 1.6 Validar subida dos containers com `docker-compose up`.

## Sequenciamento
- Bloqueado por: N/A
- Desbloqueia: 2.0, 4.0
- Paralelizável: Não

## Detalhes de Implementação
- Utilizar imagens oficiais sempre que possível.
- Configurar variáveis de ambiente básicas (usuário/senha) no próprio compose ou arquivo `.env`.
- Garantir que todos os serviços estejam na mesma rede docker (`app-network`).

## Critérios de Sucesso
- Todos os containers (RabbitMQ, Postgres x2, Apicurio) iniciam sem erros.
- É possível acessar o RabbitMQ Management UI em `localhost:15672`.
- É possível conectar nos bancos de dados via cliente SQL.
- É possível acessar a UI do Apicurio Registry.

- [x] 1.0 Configuração da Infraestrutura Base (Docker Compose) ✅ CONCLUÍDA
  - [x] 1.1 Implementação completada
  - [x] 1.2 Definição da tarefa, PRD e tech spec validados
  - [x] 1.3 Análise de regras e conformidade verificadas
  - [x] 1.4 Revisão de código completada
  - [x] 1.5 Pronto para deploy
