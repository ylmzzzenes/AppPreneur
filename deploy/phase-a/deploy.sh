#!/usr/bin/env bash
set -euo pipefail

cd /opt/apppreneur

echo "==> git pull"
git pull origin main

echo "==> Restore server production compose (keeps 127.0.0.1:8080 API binding)"
BACKUP=$(ls -t docker-compose.prod.yml.server.bak.* 2>/dev/null | head -1 || true)
if [ -n "${BACKUP}" ] && [ -f "${BACKUP}" ]; then
  cp "${BACKUP}" docker-compose.prod.yml
else
  echo "No server compose backup found; using repo docker-compose.prod.yml"
fi

echo "==> API build + CORS patch"
docker compose -f docker-compose.prod.yml --env-file .env.production build api
docker compose -f docker-compose.prod.yml --env-file .env.production \
  -f deploy/phase-a/api-cors.override.yml up -d api

echo "==> Web build + start on :3000"
docker compose -f docker-compose.web.prod.yml build
docker compose -f docker-compose.web.prod.yml up -d

echo "==> Open firewall port 3000 if ufw active"
if command -v ufw >/dev/null 2>&1 && ufw status | grep -q "Status: active"; then
  ufw allow 3000/tcp || true
fi

echo "==> Status"
docker ps

echo "==> Health checks"
curl -s http://127.0.0.1/health || curl -s http://127.0.0.1:8080/health
echo
curl -s http://127.0.0.1/ || curl -s http://127.0.0.1:8080/
echo
curl -s -o /dev/null -w "web:%{http_code}\n" http://127.0.0.1:3000/

echo "==> Done"
