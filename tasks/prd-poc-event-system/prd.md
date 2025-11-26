# Documento de Requisitos de Produto (PRD) - PoC Sistema de Eventos Musical

## Visão Geral

Este projeto consiste em uma Prova de Conceito (PoC) para fins didáticos, visando validar uma arquitetura orientada a eventos (Event-Driven Architecture) com interoperabilidade entre diferentes stacks tecnológicos. O sistema gerencia o cadastro de músicas e enriquece automaticamente esses dados com links de vídeos do YouTube e contagem de visualizações.

O sistema é composto por três aplicações principais: uma API em Java (Spring Boot) para gestão de músicas, uma API em .NET (C#) para busca de vídeos e um Frontend em React para interação com o usuário. A comunicação assíncrona é realizada via RabbitMQ e a atualização em tempo real via Server-Sent Events (SSE).

Além da arquitetura base, o projeto foca fortemente em **Documentação e Governança de Eventos**, utilizando padrões como AsyncAPI e ferramentas como SpringWolf, Neuroglia, EventCatalog e Apicurio Registry, tudo orquestrado via Docker Compose.

## Objetivos

- **Validar Interoperabilidade:** Demonstrar a comunicação fluida entre serviços Java e .NET usando RabbitMQ.
- **Testar Arquitetura de Eventos:** Implementar um fluxo completo de produção e consumo de mensagens (Round-trip).
- **Enriquecimento de Dados Assíncrono:** Validar o padrão de enriquecimento de dados onde um serviço complementa a informação iniciada por outro.
- **Feedback em Tempo Real:** Implementar SSE para notificar o frontend assim que o processamento em background for concluído.
- **Governança e Documentação:** Estabelecer um pipeline de documentação de eventos automatizado (AsyncAPI) e centralizado.

## Histórias de Usuário

### 1. Cadastro de Música Detalhado
**Como** usuário do sistema,
**Quero** cadastrar uma música informando título, intérprete, data de lançamento e estilo musical,
**Para que** eu tenha um registro completo da obra.

### 2. Visualização de Músicas e Vídeos
**Como** usuário do sistema,
**Quero** ver uma lista das músicas cadastradas com seus detalhes (lançamento, estilo) e, assim que disponível, o vídeo do YouTube e sua contagem de views,
**Para que** eu possa ouvir a música e saber sua popularidade.

### 3. Atualização em Tempo Real
**Como** usuário na tela de listagem,
**Quero** ver o vídeo e as visualizações aparecerem na lista assim que forem encontrados, sem precisar recarregar a página,
**Para que** eu tenha uma experiência fluida e imediata.

## Funcionalidades Principais

### 1. API de Gestão de Músicas (Java/Spring Boot)
- **Cadastro de Músicas:** Endpoint REST para receber Título, Intérprete, Data de Lançamento e Estilo Musical.
- **Persistência Inicial:** Salvar a música em banco PostgreSQL com status "PENDENTE".
- **Publicação de Evento:** Publicar mensagem `song.created` no RabbitMQ contendo todos os dados cadastrais.
- **Consumo de Retorno:** Consumir evento `video.found` (vindo do .NET) para atualizar o registro com a URL do vídeo, visualizações e status "CONCLUÍDO".
- **Stream de Eventos (SSE):** Endpoint para o Frontend escutar atualizações de status das músicas.
- **Documentação AsyncAPI:** Utilizar **SpringWolf** para gerar a documentação automática (Code-First) dos eventos publicados/consumidos.

### 2. API de Busca de Vídeos (.NET/C#)
- **Consumo de Evento:** Escutar a fila `song.created` no RabbitMQ.
- **Scraping do YouTube:** Realizar uma busca no YouTube utilizando "Intérprete + Título".
- **Extração de Dados:** Extrair o link do primeiro vídeo encontrado e a **quantidade de visualizações**.
- **Persistência de Vídeo:** Salvar os metadados do vídeo em seu próprio banco PostgreSQL.
- **Publicação de Conclusão:** Publicar evento `video.found` no RabbitMQ com o ID da música, URL do vídeo e quantidade de views.
- **Documentação AsyncAPI:** Utilizar **Neuroglia.AsyncAPI** para gerar a documentação automática (Code-First) dos eventos, preferencialmente na versão **AsyncAPI v3** conforme suporte da biblioteca.

### 3. Frontend (React)
- **Página de Cadastro:** Formulário para input de Título, Intérprete, Data de Lançamento e Estilo Musical.
- **Página de Listagem:** Tabela/Lista exibindo todos os campos da música, o player/link do vídeo e o contador de views.
- **Integração SSE:** Conexão persistente com a API Java para receber atualizações em tempo real.

### 4. Infraestrutura e Governança
- **Docker Compose:** Orquestração completa de todos os serviços (APIs, Frontend, Bancos, RabbitMQ, Ferramentas de Governança).
- **Apicurio Registry:** Registro central de schemas para garantir contrato entre os serviços.
- **EventCatalog:** Portal de documentação gerado estaticamente para visualização amigável dos eventos e serviços da arquitetura, ingerindo os arquivos gerados pelo SpringWolf e Neuroglia.

## Experiência do Usuário

### Fluxo Principal
1.  Usuário acessa a **Página de Cadastro**.
2.  Preenche os dados (incluindo Data e Estilo) e clica em "Salvar".
3.  Sistema redireciona para a **Página de Listagem**.
4.  A música aparece na lista com status "Buscando vídeo...".
5.  Automaticamente (via SSE), o status muda e o vídeo e o número de views aparecem na linha correspondente.

## Restrições Técnicas de Alto Nível

- **Broker de Mensagens:** RabbitMQ.
- **Bancos de Dados:** PostgreSQL (Database per Service).
- **Documentação de Eventos (Java):** SpringWolf.
- **Documentação de Eventos (.NET):** Neuroglia.AsyncAPI.
- **Registro de Schemas:** Apicurio Registry.
- **Portal de Documentação:** EventCatalog.
- **Containerização:** Docker Compose obrigatório para subir todo o ambiente com um comando.

## Não-Objetivos (Fora de Escopo)

- **Autenticação/Autorização.**
- **Scraping Robusto:** Tratamento simples de erros de scraping.
- **Validação Complexa de Schemas:** O uso do Apicurio será para registro, não necessariamente para validação bloqueante em tempo de execução (schema validation strict) nesta PoC, a menos que facilitado pelas libs.

## Questões em Aberto

- **Integração EventCatalog:** Utilizar o plugin oficial `@eventcatalog/generator-asyncapi` configurado para consumir os endpoints AsyncAPI expostos pelos serviços (SpringWolf e Neuroglia) via rede interna do Docker Compose.
