---
status: pending
parallelizable: false
blocked_by: ["5.0"]
---

<task_context>
<domain>infra/swarm</domain>
<type>implementation</type>
<scope>configuration</scope>
<complexity>high</complexity>
<dependencies>database|http_server</dependencies>
<unblocks>["7.0"]</unblocks>
</task_context>

# Tarefa 6.0: Criar docker-stack.yml para deploy em Docker Swarm

## Visão Geral
Criar o arquivo de definição da stack (`docker-stack.yml`) que será utilizado pelo Portainer para fazer o deploy no Docker Swarm. Este arquivo deve conter todos os serviços, redes, volumes e configurações de labels para o Traefik.

## Requisitos
- Arquivo: `docker-stack.yml` na raiz.
- Serviços: `traefik`, `portainer`, `rabbitmq`, `postgres-music`, `postgres-video`, `apicurio`, `music-service`, `video-enricher`, `frontend`, `eventcatalog`.
- Imagens: Referenciar imagens do GHCR para os serviços da aplicação.
- Labels Traefik: Configurar roteamento por Host (`*.tasso.dev.br`) e porta.
- Secrets: Usar Docker Secrets para senhas e tokens.

## Subtarefas
- [ ] 6.1 Definir serviços de infraestrutura (Traefik, DBs, RabbitMQ).
- [ ] 6.2 Definir serviços da aplicação apontando para GHCR.
- [ ] 6.3 Configurar labels do Traefik para cada serviço (routers, services, ports).
- [ ] 6.4 Definir healthchecks para todos os serviços.

## Sequenciamento
- Bloqueado por: 5.0 (para saber os nomes das imagens corretos)
- Desbloqueia: 7.0
- Paralelizável: Parcialmente (estrutura pode ser feita antes)

## Detalhes de Implementação
- Traefik deve mapear portas 80 e 443 do host.
- Configurar redirecionamento HTTP -> HTTPS.
- Configurar Resolver do Let's Encrypt.

## Critérios de Sucesso
- Arquivo `docker-stack.yml` válido (validar com `docker stack config`).
- Cobre todos os requisitos de roteamento e serviços.
