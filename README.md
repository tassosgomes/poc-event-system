# poc-event-system
Repositório para POC de ferramentas como Springwolf, Neuroglia.AsyncAPI, EventCatalog e Apicurio

## 🚀 Visão Geral

Este projeto demonstra uma arquitetura orientada a eventos (EDA) onde microsserviços se comunicam de forma assíncrona através do RabbitMQ. O sistema gerencia um catálogo de músicas e enriquece automaticamente os dados com informações de vídeos do YouTube.

## 🏗️ Arquitetura e Componentes

O sistema é composto pelas seguintes aplicações:

### 1. Music Service (Java / Spring Boot)
O serviço principal responsável pelo domínio de músicas.
- **Função**: Gerencia o cadastro de músicas e serve os dados para o frontend.
- **Tecnologias**: Java 17, Spring Boot 3, Spring Data JPA, Spring AMQP, Springwolf.
- **Banco de Dados**: PostgreSQL (`music_db`).
- **Comunicação**:
  - **API REST**: Exposta na porta `8080` para criar e listar músicas.
  - **SSE (Server-Sent Events)**: Notifica o frontend em tempo real sobre atualizações.
  - **Mensageria**:
    - Publica o evento `music.song-created` quando uma nova música é cadastrada.
    - Consome o evento `music.video-found` para atualizar a música com a URL do vídeo e contagem de views.
- **Documentação**: Utiliza **Springwolf** para gerar documentação AsyncAPI automaticamente.

### 2. Video Enricher (.NET)
Um worker service focado em enriquecimento de dados.
- **Função**: Escuta eventos de novas músicas, busca o vídeo correspondente no YouTube e publica os metadados encontrados.
- **Tecnologias**: .NET 9, Entity Framework Core, RabbitMQ.Client.
- **Banco de Dados**: PostgreSQL (`video_db`).
- **Comunicação**:
  - **Mensageria**:
    - Consome o evento `music.song-created`.
    - Realiza scraping do YouTube para encontrar o vídeo (baseado em Artista e Título).
    - Publica o evento `music.video-found` com os dados encontrados.

### 3. Frontend (React)
Interface do usuário para interação com o sistema.
- **Função**: Permite cadastrar músicas e visualizar a lista com atualizações em tempo real.
- **Tecnologias**: React, Vite, TypeScript.
- **Integração**: Conecta-se ao `music-service` via REST e SSE.

### 4. EventCatalog
Portal de documentação unificado.
- **Função**: Agrega as especificações AsyncAPI dos serviços para fornecer uma visão visual dos eventos, serviços e domínios.
- **Acesso**: Disponível na porta `8083`.

## 🔄 Fluxo de Funcionamento

1. **Cadastro**: O usuário cadastra uma música (ex: "Bohemian Rhapsody" - "Queen") através do Frontend.
2. **Publicação**: O `music-service` salva a música no banco com status "PENDING" e publica o evento `music.song-created` no RabbitMQ.
3. **Processamento**: O `video-enricher` recebe o evento, busca o vídeo no YouTube e extrai a URL e número de views.
4. **Enriquecimento**: O `video-enricher` salva os metadados e publica o evento `music.video-found`.
5. **Atualização**: O `music-service` recebe o evento `music.video-found`, atualiza o registro da música no banco e notifica o Frontend via SSE.
6. **Visualização**: O usuário vê a música atualizada na tela com o link do vídeo e views, sem precisar recarregar a página.

## 🛠️ Como Executar

```bash
docker compose up -d
```

- **Frontend**: http://localhost:5173 (ou porta configurada)
- **Music Service**: http://localhost:8080
- **EventCatalog**: http://localhost:8083
- **RabbitMQ Admin**: http://localhost:15672 
