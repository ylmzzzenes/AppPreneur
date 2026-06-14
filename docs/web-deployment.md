# UniFlow Web — Deployment & Development

## Local development

```powershell
cd src/frontend/UniFlow.Web
npm install
cp .env.web.example .env.development.local
# VITE_API_BASE_URL=http://localhost:5000
npm run dev
```

Open http://localhost:5173 — backend must allow CORS for `localhost:5173` (configured in `appsettings.Development.json`).

## Production API base URL

| Phase | Web URL | `VITE_API_BASE_URL` |
|-------|---------|------------------------|
| A (current) | http://49.13.89.74:3000/ | `http://49.13.89.74` |
| B (domain) | https://app.domain.com | empty (same-origin) |

## Build

```powershell
cd src/frontend/UniFlow.Web
npm run build
```

Output: `dist/`

## Docker (Phase A — port 3000)

```bash
cd /opt/apppreneur
docker compose -f docker-compose.web.prod.yml build
docker compose -f docker-compose.web.prod.yml up -d
```

## Full server deploy (API + web)

```bash
cd /opt/apppreneur
git pull origin main
bash deploy/phase-a/deploy.sh
```

## Tests

```bash
curl -s http://49.13.89.74/health
curl -s -o /dev/null -w "%{http_code}" http://49.13.89.74:3000/
docker logs uniflow-web-prod --tail 50
docker logs uniflow-api-prod --tail 50
```

Browser checklist: login, register, onboarding, dashboard, tasks CRUD, courses, chat, study plan, syllabus scan→preview→confirm, profile, logout.

## PostgreSQL identity fix (one-time)

If register returns 500 (`Users.Id` null), run:

```bash
docker exec -i uniflow-postgres-prod psql -U uniflow_prod -d uniflow < deploy/phase-a/fix-postgres-identity.sql
```

## Risks

- Do not move API from `http://49.13.89.74/` root without updating mobile `ApiConstants.cs`.
- OCR Stub on production only supports text files; PDF/images need Gemini OCR.
- JWT in localStorage — no refresh token.

## Phase B (domain)

- `api.domain.com` → backend
- `app.domain.com` → web gateway (`docker-compose.prod.yml` + `nginx.conf`)
- Update `Cors__AllowedOrigins` when needed.
