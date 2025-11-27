#!/usr/bin/env bash
set -euo pipefail

PORT=${PORT:-3000}
WAIT_INTERVAL_SECONDS=${WAIT_INTERVAL_SECONDS:-5}
MAX_WAIT_SECONDS=${MAX_WAIT_SECONDS:-180}
ASYNCAPI_SOURCE_URLS=${ASYNCAPI_SOURCE_URLS:-"http://music-service:8080/springwolf/docs,http://video-enricher:8080/asyncapi/docs"}

trim() {
  local var="$1"
  var="${var#${var%%[![:space:]]*}}"
  var="${var%${var##*[![:space:]]}}"
  printf '%s' "$var"
}

wait_for_endpoint() {
  local url=$(trim "$1")
  if [[ -z "$url" ]]; then
    return
  fi

  echo "[eventcatalog] Waiting for AsyncAPI endpoint: $url"
  local elapsed=0
  until curl -fsS "$url" >/dev/null 2>&1; do
    if (( elapsed >= MAX_WAIT_SECONDS )); then
      echo "[eventcatalog] ERROR: Timeout waiting for $url after ${MAX_WAIT_SECONDS}s" >&2
      exit 1
    fi
    sleep "$WAIT_INTERVAL_SECONDS"
    elapsed=$((elapsed + WAIT_INTERVAL_SECONDS))
  done
  echo "[eventcatalog] Endpoint is available: $url"
}

IFS=',' read -ra URLS <<< "$ASYNCAPI_SOURCE_URLS"
for endpoint in "${URLS[@]}"; do
  wait_for_endpoint "$endpoint"
done

echo "[eventcatalog] Generating catalog content"
npm run catalog:build

echo "[eventcatalog] Starting preview server on port ${PORT}"
exec npx astro preview --host --port ${PORT}
