---
status: pending
parallelizable: true
blocked_by: ["2.0"]
---

<task_context>
<domain>infra</domain>
<type>implementation</type>
<scope>configuration</scope>
<complexity>low</complexity>
<dependencies>none</dependencies>
<unblocks>["5.0"]</unblocks>
</task_context>

# Tarefa 3.0: Simplificar Dockerfile e entrypoint do EventCatalog

## Visão Geral
Como a sincronização das specs será feita em tempo de build (ou pré-build no CI), o Dockerfile e o entrypoint do EventCatalog não precisam mais executar o script de sincronização nem aguardar pelos serviços backend. Esta tarefa visa limpar esses arquivos para tornar o container mais leve e rápido na inicialização.

## Requisitos
- Remover a chamada ao `sync-asyncapi.js` do `docker-entrypoint.sh` ou `CMD`.
- Remover lógica de `wait-for-it` ou loops de espera pelos serviços.
- Garantir que o build do Docker (`npm run build`) utilize as specs que já estarão presentes no diretório `specs/` (copiadas pelo CI ou presentes no contexto).

## Subtarefas
- [ ] 3.1 Editar `eventcatalog/Dockerfile` para remover dependências de runtime desnecessárias.
- [ ] 3.2 Editar `eventcatalog/scripts/docker-entrypoint.sh` para remover a sincronização.
- [ ] 3.3 Verificar se o comando de build do EventCatalog gera o site estático corretamente com as specs locais.

## Sequenciamento
- Bloqueado por: 2.0 (pois depende da nova lógica de sync estar pronta para testar o build)
- Desbloqueia: 5.0
- Paralelizável: Sim (com 4.0)

## Detalhes de Implementação
- O novo entrypoint deve apenas iniciar o servidor web (ex: `npm start` ou `serve`).
- O Dockerfile deve assumir que a pasta `specs/` já está populada.

## Critérios de Sucesso
- Container do EventCatalog sobe em < 5 segundos.
- Build da imagem Docker passa com sucesso.
