---
status: pending
parallelizable: false
blocked_by: ["1.0"]
---

<task_context>
<domain>engine</domain>
<type>implementation</type>
<scope>middleware</scope>
<complexity>medium</complexity>
<dependencies>http_server</dependencies>
<unblocks>["3.0", "4.0"]</unblocks>
</task_context>

# Tarefa 2.0: Refatorar script sync-asyncapi.js para modo CI

## Visão Geral
Refatorar o script `eventcatalog/scripts/sync-asyncapi.js` para suportar execução em ambiente de CI/CD. O script deve ler do novo arquivo `services.json`, tentar buscar as specs e falhar adequadamente (com exit codes) se não conseguir, ao invés de tentar infinitamente como no modo runtime atual.

## Requisitos
- Ler configurações de `eventcatalog/services.json`.
- Implementar lógica de retry limitado (ex: 3 tentativas) com backoff.
- Salvar as specs baixadas em `eventcatalog/specs/<service-name>/asyncapi.json`.
- Gerar logs estruturados ou claros para stdout para depuração no GitHub Actions.
- Usar `process.exit(1)` em caso de falha crítica se configurado para tal.

## Subtarefas
- [ ] 2.1 Modificar script para ler `services.json`.
- [ ] 2.2 Implementar lógica de fetch com timeout e retries limitados.
- [ ] 2.3 Implementar gravação dos arquivos de spec no disco.
- [ ] 2.4 Adicionar tratamento de erros e exit codes.

## Sequenciamento
- Bloqueado por: 1.0
- Desbloqueia: 3.0, 4.0
- Paralelizável: Não

## Detalhes de Implementação
- O script deve ser executável via Node.js.
- Deve suportar uma flag ou variável de ambiente `CI=true` para alterar o comportamento de loop infinito para one-off execution.

## Critérios de Sucesso
- Script executa com sucesso localmente lendo `services.json`.
- Script baixa e salva os arquivos `.json` ou `.yaml` das specs corretamente.
- Script retorna erro se um serviço obrigatório não estiver acessível (após retries).
