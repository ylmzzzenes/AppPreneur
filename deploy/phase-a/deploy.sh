#!/usr/bin/env bash
set -euo pipefail

cd /opt/apppreneur

echo "==> git pull"
git pull origin main

echo "==> API build + CORS patch"
docker compose -f docker-compose.prod.yml --env-file .env.production build api
docker compose -f docker-compose.prod.yml --env-file .env.production \
  -f deploy/phase-a/api-cors.override.yml up -d api

echo "==> Web build + start on :3000"
docker compose -f docker-compose.web.prod.yml build
docker compose -f docker-compose.web.prod.yml up -d

echo "==> Status"
docker ps

echo "==> Health checks"
curl -s http://127.0.0.1/health
echo
curl -s http://127.0.0.1/
echo
curl -s -o /dev/null -w "web:%{http_code}\n" http://127.0.0.1:3000/

echo "==> Done"
