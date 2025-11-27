# Resumo de Tarefas de Implementação: Automação do EventCatalog com CI/CD e Deploy em Docker Swarm

## Visão Geral

Este documento lista todas as tarefas necessárias para implementar a automação do EventCatalog com CI/CD e deploy em Docker Swarm, conforme definido no PRD e na Especificação Técnica.

**Objetivo Principal:** Eliminar a dependência runtime entre o EventCatalog e os serviços backend, movendo a sincronização de specs para o tempo de build, com deploy automatizado via GitHub Actions e Portainer.

## Tarefas

### Fase 1: Configuração Base
- [ ] 1.0 Criar arquivo de configuração centralizada de serviços (services.json)
- [ ] 2.0 Refatorar script sync-asyncapi.js para modo CI
- [ ] 3.0 Simplificar Dockerfile e entrypoint do EventCatalog

### Fase 2: GitHub Actions
- [ ] 4.0 Criar workflow de sincronização de AsyncAPI specs (GitHub Actions)
- [ ] 5.0 Criar workflow de build e push de imagens Docker (GitHub Actions)

### Fase 3: Docker Swarm Stack
- [ ] 6.0 Criar docker-stack.yml para deploy em Docker Swarm
- [ ] 7.0 Configurar infraestrutura Docker Swarm com Traefik e Portainer

### Fase 4: Documentação
- [ ] 8.0 Criar documentação para adição de novos serviços

## Diagrama de Dependências

```
┌─────────────────────────────────────────────────────────────────────────┐
│                           FASE 1: Configuração Base                      │
│  ┌─────────┐     ┌─────────┐     ┌─────────┐                            │
│  │  1.0    │────▶│  2.0    │────▶│  3.0    │                            │
│  │services │     │sync-api │     │Dockerfile│                           │
│  │.json    │     │refactor │     │simplify │                            │
│  └─────────┘     └─────────┘     └─────────┘                            │
└─────────────────────────────────────────────────────────────────────────┘
         │                │                │
         ▼                ▼                ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                        FASE 2: GitHub Actions                            │
│  ┌─────────────────────────────────────────────────────────────────┐    │
│  │                    ┌─────────┐                                   │    │
│  │                    │  4.0    │                                   │    │
│  │                    │sync-wf  │                                   │    │
│  │                    └────┬────┘                                   │    │
│  │                         │                                        │    │
│  │                         ▼                                        │    │
│  │                    ┌─────────┐                                   │    │
│  │                    │  5.0    │                                   │    │
│  │                    │build-wf │                                   │    │
│  │                    └─────────┘                                   │    │
│  └─────────────────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                      FASE 3: Docker Swarm Stack                          │
│  ┌─────────────────────────────────────────────────────────────────┐    │
│  │                    ┌─────────┐                                   │    │
│  │                    │  6.0    │                                   │    │
│  │                    │stack.yml│                                   │    │
│  │                    └────┬────┘                                   │    │
│  │                         │                                        │    │
│  │                         ▼                                        │    │
│  │                    ┌─────────┐                                   │    │
│  │                    │  7.0    │                                   │    │
│  │                    │infra    │                                   │    │
│  │                    └─────────┘                                   │    │
│  └─────────────────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                        FASE 4: Documentação                              │
│                    ┌─────────┐                                           │
│                    │  8.0    │                                           │
│                    │docs     │                                           │
│                    └─────────┘                                           │
└─────────────────────────────────────────────────────────────────────────┘
```

## Lanes de Execução Paralela

| Lane | Tarefas | Notas |
|------|---------|-------|
| **Lane A** | 1.0 → 2.0 → 4.0 | Configuração e sync workflow |
| **Lane B** | 3.0 (após 2.0) | Dockerfile simplificado |
| **Lane C** | 5.0 (após 4.0) | Build workflow |
| **Lane D** | 6.0, 7.0 (após 5.0) | Stack Swarm |
| **Lane E** | 8.0 (após 7.0) | Documentação final |

**Tarefas paralelizáveis:**
- 3.0 pode ser executada em paralelo com 4.0 (após 2.0 completar)
- 6.0 pode iniciar enquanto 5.0 está em andamento (apenas referências de imagens)

## Estimativa de Esforço

| Tarefa | Complexidade | Estimativa |
|--------|--------------|------------|
| 1.0 | Baixa | 0.5 dia |
| 2.0 | Média | 1-2 dias |
| 3.0 | Baixa | 0.5 dia |
| 4.0 | Média | 1-2 dias |
| 5.0 | Média | 1 dia |
| 6.0 | Alta | 2-3 dias |
| 7.0 | Alta | 1-2 dias |
| 8.0 | Baixa | 0.5 dia |

**Total estimado:** 8-12 dias

## Arquivos de Tarefas Individuais

Cada tarefa possui um arquivo detalhado:
- [1_task.md](./1_task.md) - Configuração centralizada de serviços
- [2_task.md](./2_task.md) - Refatoração do script sync-asyncapi.js
- [3_task.md](./3_task.md) - Simplificação do Dockerfile EventCatalog
- [4_task.md](./4_task.md) - Workflow de sincronização AsyncAPI
- [5_task.md](./5_task.md) - Workflow de build e push Docker
- [6_task.md](./6_task.md) - Docker Stack para Swarm
- [7_task.md](./7_task.md) - Infraestrutura Swarm com Traefik/Portainer
- [8_task.md](./8_task.md) - Documentação para novos serviços
