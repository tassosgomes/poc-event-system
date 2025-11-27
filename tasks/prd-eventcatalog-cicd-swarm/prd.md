# PRD: Automação do EventCatalog com CI/CD e Deploy em Docker Swarm

## Visão Geral

O EventCatalog atualmente sincroniza as especificações AsyncAPI dos serviços em tempo de execução, através do script `sync-asyncapi.js` executado no entrypoint do container. Isso cria uma dependência runtime entre o EventCatalog e os serviços (`music-service` e `video-enricher`), causando delays no startup e potenciais falhas quando os serviços não estão disponíveis.

Este PRD define a evolução para um fluxo automatizado onde:
1. **Desenvolvedores** adicionam novos serviços com documentação AsyncAPI de forma padronizada
2. **GitHub Actions** sincroniza as specs, builda as imagens e publica no GitHub Container Registry (GHCR)
3. **Portainer** monitora a stack no repositório e atualiza o Docker Swarm automaticamente
4. **Traefik** atua como proxy reverso, roteando requisições para os serviços via subdomínios

O resultado é um processo DevOps completo que reduz intervenção manual, melhora a confiabilidade e estabelece um padrão claro para adicionar novos serviços ao catálogo.

## Objetivos

| Objetivo | Métrica de Sucesso |
|----------|-------------------|
| Automatizar sincronização de AsyncAPI specs | 100% das specs atualizadas automaticamente em cada push para main |
| Eliminar dependência runtime do EventCatalog | Tempo de startup do EventCatalog < 30 segundos (sem aguardar serviços) |
| Padronizar adição de novos serviços | Documentação e templates disponíveis; novo serviço adicionado em < 15 minutos |
| Deploy automatizado em Docker Swarm | Stack atualizada automaticamente via Portainer após push para main |
| Publicar imagens no GHCR | Todas as imagens versionadas e disponíveis no GitHub Container Registry |

## Histórias de Usuário

### Desenvolvedor Backend
> Como **desenvolvedor backend**, eu quero **adicionar meu novo serviço ao EventCatalog seguindo um processo documentado** para que **a documentação de eventos seja publicada automaticamente sem intervenção manual**.

**Critérios de Aceite:**
- Existe documentação clara do processo de adição
- O desenvolvedor configura apenas o endpoint AsyncAPI do seu serviço
- Após merge na main, a spec aparece no EventCatalog em até 10 minutos

### DevOps Engineer
> Como **engenheiro DevOps**, eu quero **que o deploy da stack seja automatizado via GitHub Actions e Portainer** para que **eu não precise executar comandos manuais no servidor**.

**Critérios de Aceite:**
- Push na main dispara build e push das imagens para GHCR
- Portainer detecta mudanças na stack e atualiza o Swarm
- Logs de deploy disponíveis no GitHub Actions

### Arquiteto de Software
> Como **arquiteto de software**, eu quero **visualizar todos os eventos e serviços no EventCatalog sempre atualizado** para que **eu possa tomar decisões de design baseadas em documentação confiável**.

**Critérios de Aceite:**
- EventCatalog reflete o estado atual das APIs em produção
- Specs AsyncAPI versionadas no repositório (auditabilidade)
- Catálogo acessível via URL pública/interna

## Funcionalidades Principais

### F1. Pipeline de Sincronização de AsyncAPI Specs

**O que faz:** Automatiza a coleta das especificações AsyncAPI dos serviços e persiste no repositório.

**Por que é importante:** Remove a necessidade de sincronização em runtime, garantindo que as specs estejam sempre disponíveis e versionadas.

**Como funciona (alto nível):**
- GitHub Action é disparada em push para main
- Workflow sobe os serviços temporariamente via docker-compose no runner
- Aguarda healthchecks dos serviços
- Script `sync-asyncapi.js` coleta as specs de cada serviço configurado
- Specs são commitadas automaticamente no diretório `eventcatalog/specs/`
- Serviços temporários são encerrados

**Requisitos Funcionais:**
- **RF1.1:** O workflow deve executar automaticamente em cada push para a branch main
- **RF1.2:** O workflow deve suportar adição de novos serviços via configuração (arquivo ou variáveis)
- **RF1.3:** Specs sincronizadas devem ser commitadas no repositório com mensagem padronizada
- **RF1.4:** Falha na sincronização de um serviço não deve bloquear os demais
- **RF1.5:** Workflow deve gerar log detalhado de quais specs foram atualizadas
- **RF1.6:** Serviços devem ser iniciados temporariamente no runner do GitHub Actions para coleta

### F2. Build e Publicação de Imagens Docker

**O que faz:** Constrói as imagens Docker de todos os serviços e publica no GitHub Container Registry.

**Por que é importante:** Centraliza as imagens em um registry confiável e integrado ao GitHub, facilitando o deploy.

**Requisitos Funcionais:**
- **RF2.1:** Imagens devem ser buildadas para: `music-service`, `video-enricher`, `frontend`, `eventcatalog`
- **RF2.2:** Imagens devem ser tagueadas com SHA do commit e `latest`
- **RF2.3:** Imagens devem ser publicadas no GHCR (`ghcr.io/tassosgomes/poc-event-system/*`)
- **RF2.4:** Build deve utilizar cache para otimizar tempo de execução
- **RF2.5:** Credenciais do GHCR devem ser gerenciadas via GitHub Secrets

### F3. Stack Docker Swarm para Portainer

**O que faz:** Define a stack de produção em formato compatível com Docker Swarm e Portainer.

**Por que é importante:** Permite que o Portainer monitore o repositório e aplique atualizações automaticamente.

**Requisitos Funcionais:**
- **RF3.1:** Stack file deve estar no repositório em local padronizado (ex: `docker-stack.yml`)
- **RF3.2:** Stack deve referenciar imagens do GHCR com tags versionadas
- **RF3.3:** Stack deve incluir todos os serviços: Traefik, RabbitMQ, PostgreSQL (x2), Apicurio, music-service, video-enricher, frontend, eventcatalog
- **RF3.4:** Stack deve definir healthchecks para todos os serviços
- **RF3.5:** Stack deve ser compatível com deploy em single-node Swarm
- **RF3.6:** Secrets sensíveis devem usar Docker Swarm Secrets
- **RF3.7:** Traefik deve rotear requisições via labels nos serviços (subdomínios)
- **RF3.8:** Traefik deve estar configurado para obter certificados SSL via Let's Encrypt

### F4. Processo de Adição de Novos Serviços

**O que faz:** Define o processo e ferramentas para desenvolvedores adicionarem novos serviços ao catálogo.

**Por que é importante:** Garante consistência e reduz a curva de aprendizado para novos contribuidores.

**Requisitos Funcionais:**
- **RF4.1:** Documentação deve descrever passo-a-passo para adicionar novo serviço
- **RF4.2:** Arquivo de configuração centralizado deve listar todos os serviços e seus endpoints AsyncAPI
- **RF4.3:** Template/checklist deve existir para validar que o serviço está pronto para integração
- **RF4.4:** EventCatalog deve gerar páginas automaticamente a partir das specs sincronizadas

### F5. Proxy Reverso com Traefik

**O que faz:** Roteia requisições HTTP/HTTPS para os serviços corretos baseado em subdomínios.

**Por que é importante:** Permite acesso aos serviços via URLs amigáveis, centraliza o gerenciamento de SSL e elimina a necessidade de expor múltiplas portas.

**Requisitos Funcionais:**
- **RF5.1:** Traefik deve ser o único ponto de entrada HTTP/HTTPS (portas 80 e 443)
- **RF5.2:** Roteamento deve ser baseado em labels Docker nos serviços
- **RF5.3:** Cada serviço acessível externamente deve ter subdomínio dedicado
- **RF5.4:** Traefik deve redirecionar HTTP para HTTPS automaticamente
- **RF5.5:** Dashboard do Traefik deve estar disponível para debugging (protegido ou desabilitado em produção)

## Experiência do Usuário

### Persona: Desenvolvedor Backend

**Jornada para adicionar novo serviço:**
1. Desenvolve o serviço com biblioteca AsyncAPI (Springwolf, Neuroglia.AsyncAPI, etc.)
2. Testa localmente que o endpoint `/asyncapi/docs` retorna a spec válida
3. Adiciona entrada no arquivo de configuração de serviços (`services.json` ou similar)
4. Abre PR com as mudanças
5. Após merge, aguarda pipeline executar
6. Verifica no EventCatalog que o serviço aparece corretamente

### Persona: DevOps Engineer

**Jornada para deploy:**
1. Configura Portainer para monitorar o repositório GitHub
2. Configura secrets no GitHub (GHCR token, etc.)
3. Após push na main, monitora execução do workflow
4. Verifica no Portainer que a stack foi atualizada
5. Valida health dos serviços no Swarm

## Restrições Técnicas de Alto Nível

| Categoria | Restrição |
|-----------|-----------|
| **Registry** | GitHub Container Registry (GHCR) é obrigatório |
| **Orquestração** | Docker Swarm em single-node (ambiente POC) |
| **CI/CD** | GitHub Actions como única plataforma de CI/CD |
| **Gerenciamento** | Portainer para visualização e GitOps do Swarm |
| **Proxy Reverso** | Traefik para roteamento via subdomínios e SSL (Let's Encrypt) |
| **Mensageria** | RabbitMQ conforme stack atual |
| **Bancos de Dados** | PostgreSQL conforme stack atual |
| **Documentação** | Serviços devem expor AsyncAPI via HTTP endpoint |
| **Secrets** | Gerenciados via Docker Swarm Secrets |
| **Acesso** | EventCatalog será público (sem autenticação) |

### Subdomínios Sugeridos

Para configurar no DNS e Traefik, a estrutura definida é (domínio base: `tasso.dev.br`):

| Serviço | Subdomínio | Porta Interna |
|---------|------------|---------------|
| Frontend | `app.tasso.dev.br` | 80 |
| EventCatalog | `eventcatalog.tasso.dev.br` | 3000 |
| Music Service API | `api.tasso.dev.br` | 8080 |
| RabbitMQ Management | `rabbitmq.tasso.dev.br` | 15672 |
| Portainer | `portainer.tasso.dev.br` | 9000 |
| Apicurio Registry | `registry.tasso.dev.br` | 8080 |

## Não-Objetivos (Fora de Escopo)

| Item | Justificativa |
|------|---------------|
| **Schema Registry** | Será tratado em PRD separado |
| **Multi-node Swarm** | Ambiente atual é POC com single-node |
| **Ambientes Staging/Prod separados** | POC utiliza ambiente único |
| **Kubernetes** | Fora de escopo; Swarm é a escolha para este POC |
| **Monitoramento avançado** | Prometheus/Grafana podem ser adicionados posteriormente |
| **Backup automatizado** | Não incluso nesta fase |
| **Versionamento de Specs** | Specs vêm diretamente da API, sem versionamento separado |
| **Rollback automatizado** | POC não requer; deploy manual via Portainer se necessário |
| **Autenticação no EventCatalog** | Catálogo será público |

## Questões em Aberto

*Todas as questões foram resolvidas.*
