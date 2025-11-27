---
status: pending
parallelizable: false
blocked_by: ["6.0"]
---

<task_context>
<domain>infra/swarm</domain>
<type>integration</type>
<scope>infrastructure</scope>
<complexity>high</complexity>
<dependencies>none</dependencies>
<unblocks>["8.0"]</unblocks>
</task_context>

# Tarefa 7.0: Configurar infraestrutura Docker Swarm com Traefik e Portainer

## Visão Geral
Preparar o ambiente de produção (servidor) inicializando o Docker Swarm, configurando o DNS e realizando o deploy inicial da stack via Portainer (GitOps).

## Requisitos
- Servidor com Docker instalado.
- Docker Swarm inicializado (`docker swarm init`).
- DNS configurado para apontar `*.tasso.dev.br` para o IP do servidor.
- Portainer rodando e configurado para monitorar o repositório Git.

## Subtarefas
- [ ] 7.1 Inicializar Swarm no servidor.
- [ ] 7.2 Configurar DNS (Wildcard A record).
- [ ] 7.3 Deploy manual inicial ou setup do Portainer Agent/Server.
- [ ] 7.4 Configurar Stack no Portainer apontando para `docker-stack.yml` no git.
- [ ] 7.5 Configurar variáveis de ambiente e secrets no Portainer/Swarm.

## Sequenciamento
- Bloqueado por: 6.0
- Desbloqueia: 8.0
- Paralelizável: Não

## Detalhes de Implementação
- Garantir que as portas 80 e 443 estejam liberadas no firewall.
- Verificar persistência de dados (volumes).

## Critérios de Sucesso
- Todos os serviços rodando (`docker service ls`).
- Acesso externo funcionando via subdomínios HTTPS.
- Portainer atualiza a stack automaticamente ao detectar mudanças no git.
