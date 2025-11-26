---
status: pending
parallelizable: true
blocked_by: ["2.0"]
---

<task_context>
<domain>frontend</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>medium</complexity>
<dependencies>react,sse</dependencies>
<unblocks>["8.0"]</unblocks>
</task_context>

# Tarefa 7.0: Implementação do Frontend (React)

## Visão Geral
Criar aplicação React para cadastro de músicas e listagem com atualizações em tempo real via SSE.

## Requisitos
- Projeto React (Vite ou CRA).
- Página de Cadastro (Formulário).
- Página de Listagem (Tabela com status e link/player).
- Consumo de SSE.

## Subtarefas
- [ ] 7.1 Inicializar projeto React no diretório `frontend`.
- [ ] 7.2 Criar serviço de API (Axios/Fetch) para `POST /songs` e `GET /songs`.
- [ ] 7.3 Implementar Tela de Cadastro (Campos: Título, Artista, Data, Gênero).
- [ ] 7.4 Implementar Tela de Listagem (Exibir todos os campos + Status).
- [ ] 7.5 Implementar consumo de SSE (`new EventSource`) na tela de listagem.
- [ ] 7.6 Atualizar estado da lista localmente ao receber evento SSE.
- [ ] 7.7 Adicionar Dockerfile para o Frontend (Nginx ou Node serve).

## Sequenciamento
- Bloqueado por: 2.0 (API básica necessária), 6.0 (para testar SSE completo)
- Desbloqueia: 8.0
- Paralelizável: Sim (pode começar com mocks e integrar depois)

## Detalhes de Implementação
- UI/UX simples e funcional.
- Mostrar indicador de carregamento enquanto status for PENDING.
- Exibir link clicável ou embed do YouTube quando status for COMPLETED.

## Critérios de Sucesso
- Usuário consegue cadastrar música.
- Lista exibe músicas cadastradas.
- Lista atualiza automaticamente (sem refresh) quando o backend processa o vídeo.
