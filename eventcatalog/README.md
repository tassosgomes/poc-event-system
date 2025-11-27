# Music Platform EventCatalog

This catalog documents the contracts that connect the `music-service` (Spring Boot) and the `video-enricher` (.NET) services. The AsyncAPI specs are synced from the running services and committed under `specs/` so the UI stays aligned with code.

## Workflow

1. Start the backend stack (or at least the two services and their dependencies):
	```bash
	docker compose up -d rabbitmq postgres-music postgres-video music-service video-enricher
	```
2. Sync the AsyncAPI specs locally so MDX files can reference the latest contracts:
	```bash
	cd eventcatalog
	MUSIC_SERVICE_ASYNCAPI_URL=http://localhost:8080/springwolf/docs \
	VIDEO_ENRICHER_ASYNCAPI_URL=http://localhost:8082/asyncapi/docs \
	npm run sync:asyncapi
	```
	Inside Docker (or once EventCatalog is running in the compose stack) the defaults `http://music-service:8080/...` and `http://video-enricher:8080/...` can be used because the containers share the same network.
3. Build/preview the catalog:
	```bash
	npm run dev   # local preview
	# or
	npm run catalog:build
	```

## Structure

- `domains/MusicPlatform` – Domain description, services, and events for the music workflow.
- `channels/music.song-created` + `channels/music.video-found` – Channel docs derived from the specs.
- `components/messages` – Lightweight schema summaries for quick reference.
- `specs/` – JSON/YAML snapshots created by `npm run sync:asyncapi`.

The old FlowMart samples were removed to keep the navigation focused on the real services that live in this repository.
