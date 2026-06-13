# Phase A — Web on :3000, API unchanged on :80

Bu aşamada mobil API **bozulmaz**. Web ayrı portta çalışır.

| URL | Servis |
|-----|--------|
| `http://49.13.89.74/` | Mevcut API (host nginx → api) |
| `http://49.13.89.74/health` | API health |
| `http://49.13.89.74:3000/` | React web (yeni) |

Web, API'ye `http://49.13.89.74/api/v1/...` ile istek atar (cross-origin).
API container'a sadece CORS origin eklenir; `.env.production` secret'larına dokunulmaz.

## Sunucuda deploy (sırayla)

```bash
cd /opt/apppreneur

# 1. Kodu güncelle (commit/push sonrası)
git pull

# 2. API'yi CORS ile yeniden build + ayağa kaldır
#    (mevcut postgres volume ve .env.production korunur)
docker compose -f docker-compose.prod.yml build api
docker compose -f docker-compose.prod.yml \
  -f deploy/phase-a/api-cors.override.yml up -d api

# 3. Web'i 3000 portunda build + ayağa kaldır
docker compose -f docker-compose.web.prod.yml build
docker compose -f docker-compose.web.prod.yml up -d

# 4. Kontrol
docker ps
curl -s http://127.0.0.1/health
curl -s http://127.0.0.1/
curl -s -o /dev/null -w "web:%{http_code}\n" http://127.0.0.1:3000/

# 5. Loglar
docker logs uniflow-api-prod --tail 50
docker logs uniflow-web-prod --tail 50
```

## Tarayıcı testi

1. `http://49.13.89.74/health` → Healthy
2. `http://49.13.89.74/` → `{"name":"UniFlow.API",...}` (değişmemeli)
3. `http://49.13.89.74:3000/` → Login ekranı
4. Register / Login → Network tab'da `http://49.13.89.74/api/v1/auth/...` 200
5. Console'da CORS hatası olmamalı

## Geri alma

```bash
docker compose -f docker-compose.web.prod.yml down
# API'yi eski CORS'suz hale getirmek için override olmadan:
docker compose -f docker-compose.prod.yml up -d api
```

## Phase B (sonra)

`:80` root → web, `/api/*` → API. Bkz. `docs/deployment-web.md`.
