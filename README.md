# UniFlow (AppPreneur)

AI-assisted academic planning for university students: syllabus ingestion, daily focus (Big 3), courses, tasks, and personality-tuned dashboard messages.

## Stack

| Layer | Technology |
| ----- | ---------- |
| Mobile | .NET MAUI (`src/frontend/UniFlow.Mobile`) |
| Backend | ASP.NET Core 8 (`src/backend/UniFlow.API`) |
| ORM | Entity Framework Core 8 |
| Database (target) | **PostgreSQL** via Docker Compose |
| Database (dev fallback) | SQL Server LocalDB |
| AI | Google Gemini (optional in Development; heuristic fallback) |
| OCR | Stub / Azure Document Intelligence / Tesseract |

## Main flows

1. **Register / login** — JWT auth
2. **Onboarding / profile** — display name, major, goals, personality vibe
3. **Syllabus scan → preview → confirm** — creates course, syllabus, and tasks
4. **Dashboard / today** — Big 3, stats, daily message
5. **Course & task CRUD** — manual management alongside AI-generated tasks

## Quick start (Docker — recommended)

Requires [Docker Desktop](https://www.docker.com/products/docker-desktop/) and .NET 8 SDK.

```bash
# 1. Environment
cp .env.example .env
# Edit .env — JWT_KEY must be at least 32 characters

# 2. Start PostgreSQL + API
docker compose config    # validate
docker compose up --build

# 3. Health check
curl http://localhost:5000/health
```

API: `http://localhost:5000` · Swagger (Development): `http://localhost:5000/swagger`

Migrations run automatically on API startup in **Development** (including the Docker API container).

### Migration note (PostgreSQL)

Existing EF migrations were **scaffolded for SQL Server**. They are applied to PostgreSQL via the same migration history for local Docker dev. If `database update` fails with provider-specific SQL, use SQL Server LocalDB fallback (see [Backend README](src/backend/README.md)) or report the issue — do not delete migration history.

Manual migration from host (PostgreSQL running in Docker):

```bash
set Database__Provider=PostgreSql
set ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=uniflow;Username=uniflow;Password=uniflow_dev_password
dotnet ef database update --project src/backend/UniFlow.DataAccess --startup-project src/backend/UniFlow.API
```

(PowerShell: `$env:Database__Provider="PostgreSql"` etc.)

## Quick start (local — SQL Server LocalDB)

Windows with LocalDB:

```bash
cd src/backend/UniFlow.API
dotnet user-secrets set "Jwt:Key" "your-secret-at-least-32-characters-long"
dotnet run
```

Swagger: `http://localhost:5087/swagger`

Default `appsettings.Development.json` uses `Database:Provider = SqlServer` and LocalDB.

## Mobile API base URL

Configured in `src/frontend/UniFlow.Mobile/ApiConstants.cs` via environment variable **`UNIFLOW_API_BASE_URL`** (trailing slash optional).

| Scenario | Base URL |
| -------- | -------- |
| Android emulator + **Docker API** | `http://10.0.2.2:5000/` |
| Android emulator + **dotnet run** | `http://10.0.2.2:5087/` |
| iOS simulator / Windows desktop | `http://127.0.0.1:5000/` or `5087/` |
| Physical device | `http://<LAN-IP>:5000/` — run API on all interfaces (`http-all-interfaces` launch profile) and allow firewall |

Set before launching the app:

```bash
# Windows PowerShell
$env:UNIFLOW_API_BASE_URL="http://10.0.2.2:5000/"
dotnet build src/frontend/UniFlow.Mobile/UniFlow.Mobile.csproj -f net8.0-android
```

## Environment variables

| Variable | Purpose |
| -------- | ------- |
| `ConnectionStrings__DefaultConnection` | Database connection string |
| `Database__Provider` | `SqlServer` or `PostgreSql` |
| `JWT_KEY` | JWT signing key (min 32 chars) — overrides `Jwt:Key` |
| `GEMINI_API_KEY` | Gemini API key — overrides `UniFlow:Gemini:ApiKey` |
| `AZURE_DOCUMENT_INTELLIGENCE_KEY` | Azure OCR key |
| `UNIFLOW_API_BASE_URL` | Mobile API base URL |
| `POSTGRES_*` | Docker Compose PostgreSQL (see `.env.example`) |

Copy `src/backend/UniFlow.API/appsettings.example.json` as a reference; never commit real secrets.

## Tests

```bash
dotnet build src/backend/UniFlow.sln
dotnet test src/backend/UniFlow.sln
dotnet build src/frontend/UniFlow.Mobile/UniFlow.Mobile.csproj -f net8.0-android
```

Integration tests use SQLite in-memory (`Testing` environment) — no PostgreSQL required for CI.

## Security

- Do **not** commit `.env`, user-secrets, or `appsettings.Local.json`
- `appsettings.json` ships with empty secrets; set keys via user-secrets or env vars
- Gemini key is optional in Development (heuristic syllabus parsing)
- Production requires JWT key, connection string, and Gemini (see backend README)

## Documentation

- [Backend README](src/backend/README.md) — secrets, migrations, providers
- [Technical PRD](UniFlow%20Teknik%20PRD%20v1.0.md)

## Project layout

```
src/backend/          .NET 8 API, Business, DataAccess, Entity
src/frontend/         .NET MAUI mobile app
docker-compose.yml    Local PostgreSQL + API
.env.example          Docker / env template (copy to .env)
```
