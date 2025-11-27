#!/usr/bin/env node
const fs = require('fs');
const fsp = fs.promises;
const path = require('path');
const { fileURLToPath } = require('url');
const YAML = require('yaml');

const DEFAULT_TIMEOUT = Number(process.env.ASYNCAPI_SYNC_TIMEOUT ?? 30000);

const sources = [
  {
    id: 'music-service',
    displayName: 'Music Service',
    url: process.env.MUSIC_SERVICE_ASYNCAPI_URL ?? 'http://music-service:8080/springwolf/docs'
  },
  {
    id: 'video-enricher',
    displayName: 'Video Enricher',
    url: process.env.VIDEO_ENRICHER_ASYNCAPI_URL ?? 'http://video-enricher:8080/asyncapi/docs'
  }
];

const cwd = path.resolve(__dirname, '..');
const specsRoot = path.join(cwd, 'specs');

function isHttp(url) {
  return url.startsWith('http://') || url.startsWith('https://');
}

function isFileUrl(url) {
  return url.startsWith('file://');
}

async function readFromFile(url) {
  if (isFileUrl(url)) {
    return await fsp.readFile(fileURLToPath(url));
  }

  const resolvedPath = path.isAbsolute(url) ? url : path.resolve(process.cwd(), url);
  return await fsp.readFile(resolvedPath);
}

async function fetchWithTimeout(resource, options = {}) {
  const controller = new AbortController();
  const timeout = setTimeout(() => controller.abort(), DEFAULT_TIMEOUT);
  try {
    const response = await fetch(resource, { ...options, signal: controller.signal });
    clearTimeout(timeout);
    return response;
  } catch (error) {
    clearTimeout(timeout);
    throw error;
  }
}

async function downloadSpec(source) {
  if (!source.url) {
    throw new Error(`Missing URL for ${source.displayName}`);
  }

  if (isHttp(source.url)) {
    const response = await fetchWithTimeout(source.url, {
      headers: { Accept: 'application/json' }
    });

    if (!response.ok) {
      throw new Error(`Failed to download spec for ${source.displayName}: ${response.status} ${response.statusText}`);
    }

    return Buffer.from(await response.arrayBuffer());
  }

  return await readFromFile(source.url);
}

function parseJson(buffer, source) {
  try {
    return JSON.parse(buffer.toString('utf-8'));
  } catch (error) {
    throw new Error(`Invalid JSON returned for ${source.displayName}: ${error.message}`);
  }
}

async function persistSpec(source, document) {
  const targetDir = path.join(specsRoot, source.id);
  await fsp.mkdir(targetDir, { recursive: true });

  const jsonPath = path.join(targetDir, 'asyncapi.json');
  const yamlPath = path.join(targetDir, 'asyncapi.yaml');

  await fsp.writeFile(jsonPath, JSON.stringify(document, null, 2));
  await fsp.writeFile(yamlPath, YAML.stringify(document));

  return { jsonPath, yamlPath };
}

async function main() {
  console.log('[sync-asyncapi] Starting AsyncAPI synchronization');
  const results = [];

  for (const source of sources) {
    try {
      console.log(`[sync-asyncapi] Fetching spec for ${source.displayName} (${source.url})`);
      const payload = await downloadSpec(source);
      const document = parseJson(payload, source);
      const { jsonPath, yamlPath } = await persistSpec(source, document);
      results.push({
        id: source.id,
        displayName: source.displayName,
        jsonPath,
        yamlPath
      });
      console.log(`[sync-asyncapi] ✔ Saved ${source.displayName} specs to ${path.relative(cwd, path.dirname(jsonPath))}`);
    } catch (error) {
      console.error(`[sync-asyncapi] ✖ Failed to sync ${source.displayName}: ${error.message}`);
      throw error;
    }
  }

  console.log('[sync-asyncapi] Completed successfully');
  return results;
}

main().catch((error) => {
  console.error('[sync-asyncapi] Aborting due to error');
  console.error(error);
  process.exit(1);
});
