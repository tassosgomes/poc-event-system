---
status: pending
parallelizable: false
blocked_by: ["3.0", "4.0"]
---

<task_context>
<domain>infra/ci</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>medium</complexity>
<dependencies>none</dependencies>
<unblocks>["6.0"]</unblocks>
</task_context>

# Tarefa 5.0: Criar workflow de build e push de imagens Docker (GitHub Actions)

## Visão Geral
Criar um workflow para construir e publicar as imagens Docker dos serviços no GitHub Container Registry (GHCR). Isso garante que o ambiente de produção (Swarm) sempre tenha acesso às versões mais recentes e versionadas dos serviços.

## Requisitos
- Arquivo: `.github/workflows/build-push.yml`.
- Trigger: `push` na branch `main` (após o sync das specs, ou em paralelo se coordenado).
- Serviços a buildar: `music-service`, `video-enricher`, `frontend`, `eventcatalog`.
- Tags: `latest` e `sha-<commit-hash>`.
- Registry: `ghcr.io`.

## Subtarefas
- [ ] 5.1 Criar arquivo do workflow.
- [ ] 5.2 Configurar login no GHCR.
- [ ] 5.3 Configurar matriz ou steps individuais para buildar cada serviço.
- [ ] 5.4 Configurar cache de layers do Docker para otimizar tempo.

## Sequenciamento
- Bloqueado por: 3.0 (Dockerfile final), 4.0 (Specs atualizadas)
- Desbloqueia: 6.0
- Paralelizável: Não

## Detalhes de Implementação
- Usar `docker/build-push-action`.
- Usar `docker/metadata-action` para tags.
- Permissões do `GITHUB_TOKEN`: `packages: write`.

## Critérios de Sucesso
- Imagens aparecem no GHCR do repositório.
- Imagens funcionam quando baixadas (docker pull).
