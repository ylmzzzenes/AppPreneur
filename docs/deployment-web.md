# UniFlow Web — Production Deployment (Hetzner)

Bu rehber, mevcut mobil API'yi **bozmadan** web arayüzünü canlıya almak içindir.

## Mevcut mimari

| Bileşen | Container | Açıklama |
|---------|-----------|----------|
| PostgreSQL | `uniflow-postgres-prod` | Veri katmanı |
| API | `uniflow-api-prod` | ASP.NET Core 8, **sadece internal** `:8080` |
| Web gateway | `uniflow-web-prod` | Nginx: SPA + `/api/*` proxy |

### URL yönlendirme (IP, domain yok)

| URL | Hedef |
|-----|-------|
| `http://49.13.89.74/` | React SPA (web) |
| `http://49.13.89.74/api/v1/...` | API (değişmedi) |
| `http://49.13.89.74/health` | API health |

**Mobil uyumluluk:** MAUI uygulaması `http://49.13.89.74/` base URL + `api/v1/...` yollarını kullanır. API yolları aynı kaldığı için mobil güncelleme gerekmez.

> **Uyarı:** API'yi `/backend/` gibi yeni bir prefix altına taşırsanız mobil `ApiConstants.cs` güncellenmelidir.

## Geçiş planı (en güvenli)

### Aşama 1 — Hazırlık (şimdi)

1. Sunucuda repo güncelle (`git pull`).
2. `.env.production` dosyasını koruyun (şifreler, JWT, AI anahtarları).
3. `docker compose -f docker-compose.prod.yml build`.
4. API container artık host'a port açmaz; sadece `web` container `:80` dinler.

### Aşama 2 — Host nginx geçişi

Sunucuda **host nginx** doğrudan API container'a proxy yapıyorsa:

**Seçenek A (önerilen):** Host nginx'i `web:80`'e yönlendir

```nginx
# /etc/nginx/sites-available/uniflow (örnek)
server {
    listen 80;
    server_name 49.13.89.74;

    client_max_body_size 25m;

    location / {
        proxy_pass http://127.0.0.1:80;  # uniflow-web-prod (docker publish)
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
    }
}
```

Eğer host nginx zaten `:80`'i kullanıyorsa, docker `WEB_PORT=8080` ile çalıştırıp host nginx'i `127.0.0.1:8080`'e proxy edin.

**Seçenek B (geçici, sıfır risk):** Web'i ayrı portta yayınla

```bash
WEB_PORT=3000 docker compose -f docker-compose.prod.yml up -d web
```

- Web: `http://49.13.89.74:3000/`
- API (mevcut): `http://49.13.89.74/` (host nginx → api) — mobil etkilenmez

### Aşama 3 — Domain geldiğinde

| Host | Servis |
|------|--------|
| `api.domain.com` | API (veya `domain.com/api`) |
| `app.domain.com` veya `domain.com` | Web SPA |

`Cors__AllowedOrigins__0=https://app.domain.com` ortam değişkeni ile CORS ekleyin.

## Sunucuda deployment komutları

```bash
cd /path/to/AppPreneur

# 1. Kodu çek
git pull

# 2. Ortam dosyası (ilk kurulumda)
cp .env.production.example .env.production
# .env.production içinde POSTGRES_PASSWORD, JWT_KEY, AI anahtarlarını doldurun

# 3. Build
docker compose -f docker-compose.prod.yml build

# 4. Ayağa kaldır
docker compose -f docker-compose.prod.yml up -d

# 5. Durum kontrol
docker ps

# 6. Health testleri
curl -s http://127.0.0.1/health
curl -s -o /dev/null -w "%{http_code}" http://127.0.0.1/
curl -s -o /dev/null -w "%{http_code}" http://127.0.0.1/login

# 7. API (mobil yolu)
curl -s -X POST http://127.0.0.1/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"wrong"}'

# 8. Loglar
docker logs uniflow-web-prod --tail 100
docker logs uniflow-api-prod --tail 100
docker logs uniflow-postgres-prod --tail 50

# 9. Restart sonrası
docker compose -f docker-compose.prod.yml restart
docker ps
curl -s http://127.0.0.1/health
```

## Lokal geliştirme

### Backend

```powershell
cd src/backend/UniFlow.API
dotnet run
# veya: docker compose up -d api postgres
```

### Web

```powershell
cd src/frontend/UniFlow.Web
npm install
npm run dev
# http://localhost:5173 — VITE_API_BASE_URL=http://localhost:5000
```

CORS: `appsettings.Development.json` → `localhost:5173` izinli.

## Test kriterleri

- [ ] `GET /health` → `Healthy`
- [ ] `/` → React login sayfası (HTML)
- [ ] Register → PostgreSQL'de kullanıcı oluşur
- [ ] Login → JWT `localStorage`'a yazılır
- [ ] Dashboard veri çeker
- [ ] Görev CRUD çalışır
- [ ] Müfredat tarama hata verirse UI spinner'da takılmaz
- [ ] Browser console'da CORS hatası yok (prod same-origin)
- [ ] `docker compose restart` sonrası sistem ayağa kalkar
- [ ] Mobil uygulama `http://49.13.89.74/api/v1/...` ile çalışmaya devam eder

## API endpoint özeti

| Alan | Route |
|------|-------|
| Auth | `POST /api/v1/auth/register`, `POST /api/v1/auth/login` |
| Users | `GET /api/v1/users/me`, `PATCH /api/v1/users/me/onboarding` |
| Dashboard | `GET /api/v1/dashboard/today` |
| Courses | `/api/v1/courses` |
| Tasks | `/api/v1/tasks` (alias: `/api/v1/Task`) |
| Syllabus | `POST /api/v1/syllabus/scan`, `confirm` |
| AI | `/api/v1/ai/study-plan`, `task-feedback`, `weekly-summary` |
| Chat | `POST /api/v1/Chat` |
| Health | `GET /health` |

## Riskler

1. **Port 80 çakışması:** Host nginx + docker web aynı portu kullanırsa biri kapatılmalı.
2. **Migration:** Web için yeni migration gerekmez.
3. **API root `/`:** Nginx artık `/`'i SPA'ya verir; mobil `/` kullanmıyor, sorun yok.
4. **HTTPS:** Domain gelince Let's Encrypt + `proxy_set_header X-Forwarded-Proto https`.

## Önerilen commit mesajları

```
feat(web): add React Vite frontend with auth, dashboard, tasks, syllabus
feat(api): add configurable CORS for web dev origins
feat(docker): add production compose with nginx web gateway
docs: add Hetzner web deployment guide
chore: update env examples for web and production
```
