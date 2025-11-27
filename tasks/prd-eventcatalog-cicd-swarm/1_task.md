---
status: done
parallelizable: true
blocked_by: []
---

<task_context>
<domain>configuration</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>low</complexity>
<dependencies>none</dependencies>
<unblocks>["2.0"]</unblocks>
</task_context>

# Tarefa 1.0: Criar arquivo de configuração centralizada de serviços (services.json) ✅ CONCLUÍDA

## Visão Geral
Criar um arquivo de configuração centralizado `eventcatalog/services.json` que listará todos os serviços e seus endpoints AsyncAPI. Este arquivo substituirá a configuração hardcoded ou baseada em variáveis de ambiente dispersas, facilitando a adição de novos serviços.

## Requisitos
- O arquivo deve estar localizado em `eventcatalog/services.json`.
- Deve seguir uma estrutura JSON válida.
- Deve listar os serviços iniciais: `music-service` e `video-enricher`.
- Deve conter o nome do serviço e a URL relativa ou absoluta para o endpoint AsyncAPI.

## Subtarefas
- [x] 1.1 Criar arquivo `eventcatalog/services.json` com a estrutura base.
- [x] 1.2 Adicionar configuração para `music-service` (endpoint: `http://music-service:8080/springwolf/docs`).
- [x] 1.3 Adicionar configuração para `video-enricher` (endpoint: `http://video-enricher:80/asyncapi/docs`).

## Sequenciamento
- Bloqueado por: N/A
- Desbloqueia: 2.0
- Paralelizável: Sim

## Detalhes de Implementação
Conforme definido na Tech Spec:
```json
{
  "services": [
    {
      "name": "music-service",
      "url": "http://music-service:8080/springwolf/docs"
    },
    {
      "name": "video-enricher",
      "url": "http://video-enricher:80/asyncapi/docs"
    }
  ]
}
```

## Critérios de Sucesso
- Arquivo `services.json` criado e validado.
- Contém os dois serviços iniciais configurados corretamente.

---

## Validação de Conclusão

- [x] 1.0 Criar arquivo de configuração centralizada de serviços (services.json) ✅ CONCLUÍDA
  - [x] 1.1 Implementação completada
  - [x] 1.2 Definição da tarefa, PRD e tech spec validados
  - [x] 1.3 Análise de regras e conformidade verificadas
  - [x] 1.4 Revisão de código completada
  - [x] 1.5 Pronto para deploy

**Revisão:** [1_task_review.md](./1_task_review.md)
