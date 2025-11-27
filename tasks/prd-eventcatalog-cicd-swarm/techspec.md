# Especificação Técnica: Automação do EventCatalog com CI/CD e Deploy em Docker Swarm

## Resumo Executivo

Esta especificação técnica define a implementação de um pipeline CI/CD automatizado usando GitHub Actions para sincronizar especificações AsyncAPI, buildar imagens Docker e publicar no GitHub Container Registry (GHCR). A stack de produção será orquestrada via Docker Swarm com Traefik como proxy reverso, gerenciada pelo Portainer em modo GitOps.

A arquitetura proposta elimina a dependência runtime entre o EventCatalog e os serviços backend, movendo a sincronização de specs para o tempo de build. O fluxo automatizado garante que qualquer push na branch `main` dispare a coleta das specs, build das imagens e atualização automática da stack em produção via Portainer.

## Arquitetura do Sistema

### Visão Geral dos Componentes

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              GitHub Repository                               │
│  ┌─────────────────┐    ┌──────────────────┐    ┌───────────────────────┐  │
│  │  services.json  │    │  docker-stack.yml │    │  .github/workflows/  │  │
│  │  (config)       │    │  (swarm stack)    │    │  (CI/CD pipelines)   │  │
│  └─────────────────┘    └──────────────────┘    └───────────────────────┘  │
└─────────────────────────────────────────────────────────────────────────────┘
                                     │
                                     ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                           GitHub Actions Runner                              │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │  1. docker-compose up (serviços temporários)                        │   │
│  │  2. sync-asyncapi.js (coleta specs)                                 │   │
│  │  3. commit specs no repositório                                     │   │
│  │  4. build imagens Docker                                            │   │
│  │  5. push para GHCR                                                  │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────────────┘
                                     │
                                     ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                        GitHub Container Registry (GHCR)                      │
│  ┌────────────────────┐  ┌────────────────────┐  ┌─────────────────────┐   │
│  │ music-service:sha  │  │ video-enricher:sha │  │ eventcatalog:sha    │   │
│  │ music-service:latest│  │ video-enricher:latest│ │ eventcatalog:latest │   │
│  └────────────────────┘  └────────────────────┘  └─────────────────────┘   │
└─────────────────────────────────────────────────────────────────────────────┘
                                     │
                                     ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                           Docker Swarm (Single Node)                         │
│                                                                              │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │                         Traefik (Proxy Reverso)                      │   │
│  │                    Portas 80/443 + Let's Encrypt                     │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│         │              │               │                │                   │
│         ▼              ▼               ▼                ▼                   │
│  ┌──────────┐   ┌───────────┐   ┌────────────┐   ┌─────────────┐          │
│  │ frontend │   │music-svc  │   │video-enr   │   │eventcatalog │          │
│  │app.tasso │   │api.tasso  │   │(internal)  │   │eventcatalog │          │
│  │.dev.br   │   │.dev.br    │   │            │   │.tasso.dev.br│          │
│  └──────────┘   └───────────┘   └────────────┘   └─────────────┘          │
│                                                                              │
│  ┌────────────────────────────────────────────────────────────────────┐    │
│  │  Infraestrutura: RabbitMQ, PostgreSQL (x2), Apicurio Registry      │    │
│  └────────────────────────────────────────────────────────────────────┘    │
│                                                                              │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │                    Portainer (GitOps Polling)                        │   │
│  │                    portainer.tasso.dev.br                            │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────────────┘
```

**Componentes principais:**

| Componente | Responsabilidade |
|------------|------------------|
| `services.json` | Arquivo de configuração centralizado com lista de serviços e endpoints AsyncAPI |
| GitHub Actions Workflows | Pipelines de CI/CD para sync, build e push |
| `docker-stack.yml` | Definição da stack Docker Swarm para produção |
| Traefik | Proxy reverso com SSL automático e roteamento por subdomínio |
| Portainer | Gerenciamento de stack com GitOps (polling do repositório) |
| GHCR | Registry de imagens Docker integrado ao GitHub |

## Design de Implementação

### Interfaces Principais

#### Arquivo de Configuração de Serviços

```json
// eventcatalog/services.json
{
  "services": [
    {
      "id": "music-service",
      "displayName": "Music Service",
      "asyncApiEndpoint": "/springwolf/docs",
      "healthEndpoint": "/actuator/health",
      "dockerContext": "./music-service",
      "port": 8080
    },
    {
      "id": "video-enricher",
      "displayName": "Video Enricher",
      "asyncApiEndpoint": "/asyncapi/docs",
      "healthEndpoint": "/health",
      "dockerContext": "./video-enricher",
      "port": 8080
    }
  ]
}
```

#### Script de Sincronização Refatorado

```javascript
// eventcatalog/scripts/sync-asyncapi.js (ajustes necessários)
// - Ler configuração de services.json
// - Suportar modo CI (sem retry infinito)
// - Exit codes apropriados para CI/CD
// - Logs estruturados para GitHub Actions
```

### Modelos de Dados

#### Docker Stack Labels (Traefik)

```yaml
# Padrão de labels para roteamento Traefik
labels:
  - "traefik.enable=true"
  - "traefik.http.routers.{service}.rule=Host(`{subdomain}.tasso.dev.br`)"
  - "traefik.http.routers.{service}.entrypoints=websecure"
  - "traefik.http.routers.{service}.tls.certresolver=letsencrypt"
  - "traefik.http.services.{service}.loadbalancer.server.port={port}"
```

#### GitHub Secrets Necessários

| Secret | Descrição |
|--------|-----------|
| `GHCR_TOKEN` | Personal Access Token com permissão `write:packages` |
| `GIT_COMMIT_TOKEN` | Token para push de commits automáticos (specs) |
| `PORTAINER_WEBHOOK_URL` | (Opcional) URL do webhook para trigger manual |

### Endpoints de API

Não há novos endpoints de API. O sistema utiliza endpoints existentes:

| Serviço | Endpoint AsyncAPI | Healthcheck |
|---------|-------------------|-------------|
| music-service | `GET /springwolf/docs` | `GET /actuator/health` |
| video-enricher | `GET /asyncapi/docs` | `GET /health` |

## Pontos de Integração

### GitHub Container Registry (GHCR)

- **Autenticação:** Via `GITHUB_TOKEN` em workflows ou PAT configurado como secret
- **Naming Convention:** `ghcr.io/tassosgomes/poc-event-system/{service}:{tag}`
- **Tags:** `latest` + SHA do commit (`sha-abc1234`)
- **Modo de Falha:** Se push falhar, workflow deve falhar e notificar

### Portainer GitOps

- **Polling Interval:** Configurar para 5 minutos (ou usar webhook)
- **Autenticação:** Deploy key ou PAT com acesso ao repositório
- **Stack Path:** `docker-stack.yml` na raiz do repositório
- **Tratamento de Erros:** Portainer mantém versão anterior em caso de falha

### Let's Encrypt (via Traefik)

- **Challenge Type:** HTTP-01 (porta 80)
- **Email:** Configurar email válido para notificações de expiração
- **Staging:** Usar staging environment durante testes iniciais
- **Storage:** Volume persistente para certificados (`traefik-certs`)

## Análise de Impacto

| Componente Afetado | Tipo de Impacto | Descrição & Nível de Risco | Ação Requerida |
|--------------------|-----------------|----------------------------|----------------|
| `eventcatalog/Dockerfile` | Modificação | Remover sync runtime, usar specs pré-commitadas. Baixo risco. | Simplificar entrypoint |
| `eventcatalog/scripts/docker-entrypoint.sh` | Modificação | Remover espera por serviços externos. Baixo risco. | Simplificar para build + serve |
| `eventcatalog/scripts/sync-asyncapi.js` | Modificação | Adaptar para modo CI, ler de services.json. Médio risco. | Refatorar script |
| `docker-compose.yml` | Adição Paralela | Manter para dev local; criar docker-stack.yml separado. Baixo risco. | Nenhuma alteração |
| Infraestrutura (Swarm) | Nova | Deploy de Traefik, Portainer, configuração de DNS. Alto risco inicial. | Setup manual único |
| Secrets do Repositório | Nova | Configurar GHCR_TOKEN e GIT_COMMIT_TOKEN. Baixo risco. | Configurar via GitHub Settings |

## Abordagem de Testes

### Testes Unitários

**Script sync-asyncapi.js:**
- Teste de parsing de `services.json`
- Teste de tratamento de timeout/falha de serviço
- Mock de fetch para simular respostas AsyncAPI
- Validação de output (arquivos JSON/YAML gerados)

```javascript
// Exemplo de cenário de teste
describe('sync-asyncapi', () => {
  it('should parse services.json correctly', async () => { /* ... */ });
  it('should handle service timeout gracefully', async () => { /* ... */ });
  it('should generate valid AsyncAPI JSON and YAML', async () => { /* ... */ });
  it('should continue on single service failure when configured', async () => { /* ... */ });
});
```

### Testes de Integração

**Pipeline CI/CD:**
- Teste de workflow em branch de feature (dry-run sem push para GHCR)
- Validação de build de todas as imagens Docker
- Teste de healthcheck dos containers após build

**Stack Swarm:**
- Validação de sintaxe do `docker-stack.yml`
- Teste de deploy em ambiente de staging (se disponível)
- Verificação de roteamento Traefik via curl

### Validação de Specs AsyncAPI

```bash
# Comando para validar specs geradas
npx @asyncapi/cli validate eventcatalog/specs/*/asyncapi.json
```

## Sequenciamento de Desenvolvimento

### Ordem de Construção

1. **Fase 1: Configuração Base (2-3 dias)**
   - Criar `services.json` com estrutura de configuração
   - Refatorar `sync-asyncapi.js` para ler configuração centralizada
   - Adaptar script para modo CI (exit codes, logs, timeout configurável)

2. **Fase 2: GitHub Actions - Sync Pipeline (2 dias)**
   - Criar workflow `.github/workflows/sync-asyncapi.yml`
   - Implementar job de subir serviços temporários via docker-compose
   - Implementar commit automático de specs sincronizadas

3. **Fase 3: GitHub Actions - Build & Push (2 dias)**
   - Criar workflow `.github/workflows/build-push.yml`
   - Configurar build com cache para otimização
   - Implementar push para GHCR com tags (SHA + latest)

4. **Fase 4: Docker Stack para Swarm (2-3 dias)**
   - Criar `docker-stack.yml` com todos os serviços
   - Configurar Traefik com labels de roteamento
   - Definir healthchecks e deploy constraints
   - Configurar Docker Swarm Secrets para dados sensíveis

5. **Fase 5: Infraestrutura Swarm (1-2 dias)**
   - Inicializar Docker Swarm no servidor
   - Deploy manual inicial de Traefik
   - Deploy de Portainer com GitOps configurado
   - Configurar DNS para subdomínios

6. **Fase 6: Documentação e Refinamentos (1 dia)**
   - Criar documentação de adição de novos serviços
   - Criar template/checklist para desenvolvedores
   - Ajustes finais e testes end-to-end

### Dependências Técnicas

| Dependência | Tipo | Bloqueante Para |
|-------------|------|-----------------|
| Servidor com Docker instalado | Infraestrutura | Fase 5 |
| DNS configurado (*.tasso.dev.br) | Infraestrutura | Fase 5 (SSL) |
| GitHub PAT com `write:packages` | Credencial | Fase 3 |
| Portainer instalado no Swarm | Infraestrutura | Fase 5 |

## Monitoramento e Observabilidade

### GitHub Actions

- **Métricas:** Tempo de execução de workflows, taxa de sucesso/falha
- **Logs:** Disponíveis nativamente na aba Actions do repositório
- **Alertas:** Configurar notificações de falha via email/Slack

### Docker Swarm / Traefik

- **Métricas Traefik:**
  ```yaml
  # Habilitar métricas Prometheus no Traefik
  - "--metrics.prometheus=true"
  - "--metrics.prometheus.addEntryPointsLabels=true"
  - "--metrics.prometheus.addServicesLabels=true"
  ```

- **Logs:** Centralizar via Docker logging driver ou stdout
- **Healthchecks:** Definidos no stack file para cada serviço

### Logs Principais

| Serviço | Nível | Eventos Críticos |
|---------|-------|------------------|
| Traefik | INFO | Novos certificados, erros de roteamento |
| GitHub Actions | INFO | Início/fim de jobs, falhas de step |
| Portainer | INFO | Atualizações de stack, falhas de deploy |
| EventCatalog | INFO | Build success/failure, startup |

## Considerações Técnicas

### Decisões Principais

| Decisão | Justificativa | Alternativas Rejeitadas |
|---------|---------------|------------------------|
| **Sync em tempo de build (CI)** | Elimina dependência runtime, specs versionadas no Git | Manter sync no entrypoint (atual): causa delays e falhas |
| **GHCR como registry** | Integração nativa com GitHub Actions, sem custo adicional para público | Docker Hub: limites de pull, custos para privado |
| **Portainer GitOps** | UI amigável, polling automático, rollback fácil | Watchtower: menos controle; ArgoCD: overkill para Swarm |
| **Traefik v3** | Suporte nativo a Swarm, Let's Encrypt automático, labels simples | Nginx: requer config manual; Caddy: menos maduro em Swarm |
| **Single workflow vs múltiplos** | Workflows separados (sync → build) permitem re-runs parciais | Monolítico: mais simples mas menos flexível |
| **services.json centralizado** | Facilita adição de novos serviços, evita hardcode | Variáveis de ambiente: menos estruturado, mais difícil de manter |

### Riscos Conhecidos

| Risco | Probabilidade | Impacto | Mitigação |
|-------|---------------|---------|-----------|
| Serviço não disponível durante sync no CI | Média | Alto | Retry com backoff, timeout configurável, alerta |
| Certificado Let's Encrypt rate limit | Baixa | Médio | Usar staging em testes, monitorar expiração |
| Portainer não detecta mudança | Baixa | Médio | Webhook manual como fallback |
| Conflito de commit automático | Baixa | Baixo | Usar token dedicado, pull antes de push |
| Imagem Docker muito grande | Média | Baixo | Multi-stage builds, .dockerignore |

### Requisitos Especiais

**Performance:**
- Build de imagens deve usar cache de layers Docker
- Sync de specs deve ter timeout máximo de 60s por serviço
- Startup do EventCatalog deve ser < 30s (sem dependência de serviços)

**Segurança:**
- Tokens armazenados como GitHub Secrets (encrypted)
- Secrets do Swarm para senhas de banco e RabbitMQ
- Dashboard do Traefik desabilitado ou protegido por autenticação

### Conformidade com Padrões

| Padrão | Conformidade | Notas |
|--------|--------------|-------|
| `git-commit.md` | ✅ Segue | Commits automáticos usarão formato `chore(eventcatalog): sync asyncapi specs` |
| `dotnet-architecture.md` | ✅ Não afetado | Video-enricher não é modificado |
| Container best practices | ✅ Segue | Multi-stage builds, healthchecks, non-root users |
| GitOps principles | ✅ Segue | Stack versionada no Git, Portainer como operador |

## Estrutura de Arquivos Proposta

```
poc-event-system/
├── .github/
│   └── workflows/
│       ├── sync-asyncapi.yml       # Pipeline de sincronização de specs
│       └── build-push.yml          # Pipeline de build e push para GHCR
├── docker-stack.yml                # Stack Docker Swarm para produção
├── docker-compose.yml              # (existente) Para desenvolvimento local
├── eventcatalog/
│   ├── services.json               # Configuração centralizada de serviços
│   ├── Dockerfile                  # (modificado) Sem sync runtime
│   ├── Dockerfile.server           # (opcional) Para server-side rendering
│   └── scripts/
│       ├── sync-asyncapi.js        # (refatorado) Modo CI
│       └── docker-entrypoint.sh    # (simplificado) Sem wait for services
└── docs/
    └── adding-new-service.md       # Documentação para desenvolvedores
```

## Apêndice: Exemplos de Configuração

### Workflow de Sincronização (sync-asyncapi.yml)

```yaml
name: Sync AsyncAPI Specs

on:
  push:
    branches: [main]
    paths:
      - 'music-service/**'
      - 'video-enricher/**'
      - 'eventcatalog/services.json'
  workflow_dispatch:

jobs:
  sync:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Start services
        run: docker compose up -d --wait

      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: '20'

      - name: Sync AsyncAPI specs
        working-directory: ./eventcatalog
        run: |
          npm ci
          npm run sync:asyncapi

      - name: Commit and push specs
        uses: stefanzweifel/git-auto-commit-action@v5
        with:
          commit_message: "chore(eventcatalog): sync asyncapi specs [skip ci]"
          file_pattern: "eventcatalog/specs/**"

      - name: Stop services
        if: always()
        run: docker compose down
```

### Workflow de Build e Push (build-push.yml)

```yaml
name: Build and Push Docker Images

on:
  push:
    branches: [main]
  workflow_run:
    workflows: ["Sync AsyncAPI Specs"]
    types: [completed]

env:
  REGISTRY: ghcr.io
  IMAGE_PREFIX: ghcr.io/tassosgomes/poc-event-system

jobs:
  build-and-push:
    runs-on: ubuntu-latest
    if: ${{ github.event.workflow_run.conclusion == 'success' || github.event_name == 'push' }}
    permissions:
      contents: read
      packages: write

    strategy:
      matrix:
        service:
          - name: music-service
            context: ./music-service
          - name: video-enricher
            context: ./video-enricher
          - name: frontend
            context: ./frontend
          - name: eventcatalog
            context: ./eventcatalog

    steps:
      - uses: actions/checkout@v4

      - name: Log in to GHCR
        uses: docker/login-action@v3
        with:
          registry: ${{ env.REGISTRY }}
          username: ${{ github.actor }}
          password: ${{ secrets.GITHUB_TOKEN }}

      - name: Set up Docker Buildx
        uses: docker/setup-buildx-action@v3

      - name: Build and push
        uses: docker/build-push-action@v5
        with:
          context: ${{ matrix.service.context }}
          push: true
          tags: |
            ${{ env.IMAGE_PREFIX }}/${{ matrix.service.name }}:latest
            ${{ env.IMAGE_PREFIX }}/${{ matrix.service.name }}:sha-${{ github.sha }}
          cache-from: type=gha
          cache-to: type=gha,mode=max
```

### Docker Stack (docker-stack.yml) - Estrutura Base

```yaml
version: "3.8"

services:
  traefik:
    image: traefik:v3.0
    command:
      - "--providers.swarm.endpoint=unix:///var/run/docker.sock"
      - "--providers.swarm.exposedByDefault=false"
      - "--entrypoints.web.address=:80"
      - "--entrypoints.websecure.address=:443"
      - "--certificatesresolvers.letsencrypt.acme.httpchallenge.entrypoint=web"
      - "--certificatesresolvers.letsencrypt.acme.email=gomestasso@gmail.com"
      - "--certificatesresolvers.letsencrypt.acme.storage=/letsencrypt/acme.json"
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - /var/run/docker.sock:/var/run/docker.sock:ro
      - traefik-certs:/letsencrypt
    deploy:
      placement:
        constraints:
          - node.role == manager
    networks:
      - public

  eventcatalog:
    image: ghcr.io/tassosgomes/poc-event-system/eventcatalog:latest
    deploy:
      labels:
        - "traefik.enable=true"
        - "traefik.http.routers.eventcatalog.rule=Host(`eventcatalog.tasso.dev.br`)"
        - "traefik.http.routers.eventcatalog.entrypoints=websecure"
        - "traefik.http.routers.eventcatalog.tls.certresolver=letsencrypt"
        - "traefik.http.services.eventcatalog.loadbalancer.server.port=3000"
    networks:
      - public

  # ... demais serviços seguem o mesmo padrão

networks:
  public:
    driver: overlay
  internal:
    driver: overlay
    internal: true

volumes:
  traefik-certs:
  postgres_music_data:
  postgres_video_data:
```

---

## Checklist de Qualidade

- [x] PRD revisado e requisitos extraídos
- [x] Análise profunda do repositório completada (docker-compose, scripts, Dockerfiles, specs)
- [x] Esclarecimentos técnicos principais definidos
- [x] Tech Spec gerada usando o template padrão
- [x] Arquivo escrito em `./tasks/prd-eventcatalog-cicd-swarm/techspec.md`
- [x] Conformidade com padrões de commit verificada
- [x] Sequenciamento de desenvolvimento definido
- [x] Riscos identificados e mitigações propostas
- [x] Questões abertas resolvidas com stakeholder

---

## Questões Resolvidas

| Questão | Resolução |
|---------|-----------|
| **DNS** | ✅ Acesso confirmado para configurar subdomínios `*.tasso.dev.br` |
| **Servidor** | ✅ VPS atual é suficiente; todos os serviços rodarão com 1 réplica |
| **Email Let's Encrypt** | ✅ `gomestasso@gmail.com` |
| **Portainer** | ✅ Já instalado e operacional no VPS |
