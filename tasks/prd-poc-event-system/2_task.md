---
status: pending
parallelizable: true
blocked_by: ["1.0"]
---

<task_context>
<domain>backend/java</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>medium</complexity>
<dependencies>database</dependencies>
<unblocks>["3.0", "7.0"]</unblocks>
</task_context>

# Tarefa 2.0: Implementação do Music Service (Java/Spring Boot) - Core & API

## Visão Geral
Inicializar o projeto Spring Boot para o Music Service, configurar a conexão com o banco de dados PostgreSQL e implementar a API REST básica para cadastro e listagem de músicas.

## Requisitos
- Projeto Java 17+ com Spring Boot 3.x.
- Dependências: Spring Web, Spring Data JPA, PostgreSQL Driver, Lombok.
- Entidade `Song` com campos: id, title, artist, releaseDate, genre, videoUrl, views, status.
- Repository e Service para operações de CRUD.
- Controller REST para `POST /songs` e `GET /songs`.

## Subtarefas
- [ ] 2.1 Inicializar projeto Spring Boot (via Spring Initializr ou manual) no diretório `music-service`.
- [ ] 2.2 Configurar `application.properties`/`yaml` com conexão ao Postgres (container criado na task 1.0).
- [ ] 2.3 Criar entidade JPA `Song` com mapeamento correto (incluindo Enum para Status).
- [ ] 2.4 Criar `SongRepository` (interface JpaRepository).
- [ ] 2.5 Implementar `SongService` com métodos `createSong` e `listSongs`.
- [ ] 2.6 Implementar `SongController` com endpoints REST.
- [ ] 2.7 Criar testes unitários para Service e Controller.
- [ ] 2.8 Adicionar Dockerfile para o serviço Java.

## Sequenciamento
- Bloqueado por: 1.0
- Desbloqueia: 3.0, 7.0
- Paralelizável: Sim (com 4.0)

## Detalhes de Implementação
- Status inicial ao criar música deve ser `PENDING`.
- Validar inputs básicos (título e artista obrigatórios).
- Data de lançamento deve aceitar formato ISO 8601.

## Critérios de Sucesso
- Aplicação sobe e conecta ao banco de dados.
- Endpoint `POST /songs` cria registro no banco com status PENDING.
- Endpoint `GET /songs` retorna lista de músicas cadastradas.
- Testes unitários passando.
