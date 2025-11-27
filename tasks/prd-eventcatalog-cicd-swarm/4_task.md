---
status: pending
parallelizable: true
blocked_by: ["2.0"]
---

<task_context>
<domain>infra/ci</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>medium</complexity>
<dependencies>external_apis</dependencies>
<unblocks>["5.0"]</unblocks>
</task_context>

# Tarefa 4.0: Criar workflow de sincronização de AsyncAPI specs (GitHub Actions)

## Visão Geral
Criar um workflow do GitHub Actions que automatiza a coleta das specs AsyncAPI. Este workflow subirá os serviços necessários temporariamente, executará o script de sincronização e commitará as alterações de volta ao repositório.

## Requisitos
- Arquivo: `.github/workflows/sync-asyncapi.yml`.
- Trigger: `push` na branch `main` (com paths filter para evitar loops) e `workflow_dispatch`.
- Steps:
    1. Checkout do código.
    2. Setup Node.js.
    3. Subir serviços (`docker compose up -d music-service video-enricher`).
    4. Executar `sync-asyncapi.js`.
    5. Verificar mudanças no git.
    6. Commitar e dar push das novas specs (se houver mudanças).

## Subtarefas
- [ ] 4.1 Criar arquivo do workflow.
- [ ] 4.2 Configurar steps de serviço (Docker Compose ou Service Containers).
- [ ] 4.3 Configurar step de execução do script de sync.
- [ ] 4.4 Configurar step de commit automático (usando `stefanzweifel/git-auto-commit-action` ou similar).

## Sequenciamento
- Bloqueado por: 2.0
- Desbloqueia: 5.0
- Paralelizável: Sim (com 3.0)

## Detalhes de Implementação
- Usar `GITHUB_TOKEN` ou um PAT (`GIT_COMMIT_TOKEN`) para o push.
- Mensagem de commit padronizada: `chore(eventcatalog): sync asyncapi specs`.

## Critérios de Sucesso
- Workflow executa com sucesso no GitHub Actions.
- Specs são atualizadas no repositório após a execução.
