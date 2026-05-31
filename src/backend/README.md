# UniFlow Backend

.NET 8 API (`UniFlow.API`) with Business, DataAccess, and Entity layers.

## Local development

### Option A — Docker (PostgreSQL, recommended)

From repository root:

```bash
cp .env.example .env
# Set JWT_KEY (min 32 characters)
docker compose up --build
```

- API: `http://localhost:5000`
- Health: `http://localhost:5000/health`
- Swagger: `http://localhost:5000/swagger`
- PostgreSQL: `localhost:5432` (user/db/password from `.env`)

Migrations apply automatically on startup in Development.

### Option B — dotnet run (SQL Server LocalDB fallback)

```bash
cd src/backend/UniFlow.API
dotnet user-secrets set "Jwt:Key" "your-secret-at-least-32-characters-long"
dotnet run
```

Swagger: `http://localhost:5087/swagger`

Uses `Database:Provider = SqlServer` and LocalDB from `appsettings.Development.json`.

### Option C — dotnet run + local PostgreSQL

Start Postgres (Docker or native), then:

```bash
cd src/backend/UniFlow.API
dotnet user-secrets set "Jwt:Key" "your-secret-at-least-32-characters-long"
dotnet user-secrets set "Database:Provider" "PostgreSql"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=uniflow;Username=uniflow;Password=uniflow_dev_password"
dotnet run
```

## Database providers

Configure in `appsettings.json`, user-secrets, or environment variables:

```json
{
  "Database": {
    "Provider": "PostgreSql"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=uniflow;Username=uniflow;Password=..."
  }
}
```

| `Database:Provider` | EF Core |
| ------------------- | ------- |
| `SqlServer` | `UseSqlServer` (default) |
| `PostgreSql` | `UseNpgsql` |

Unknown provider → startup error.

## Secrets (never commit)

See `appsettings.example.json` for the full template.

| Setting | User-secrets key | Environment variable |
|--------|------------------|----------------------|
| JWT signing key | `Jwt:Key` | `JWT_KEY` |
| Gemini API key | `UniFlow:Gemini:ApiKey` | `GEMINI_API_KEY` |
| Azure Document Intelligence | `UniFlow:Ocr:Azure:ApiKey` | `AZURE_DOCUMENT_INTELLIGENCE_KEY` |
| DB provider | `Database:Provider` | `Database__Provider` |
| SQL/Postgres connection | `ConnectionStrings:DefaultConnection` | `ConnectionStrings__DefaultConnection` |

```bash
dotnet user-secrets set "Jwt:Key" "your-secret-at-least-32-characters-long"
dotnet user-secrets list
```

## Validation behavior

- **Development:** `Jwt:Key` required (user-secrets or `JWT_KEY`). Gemini optional (heuristic syllabus parsing when absent).
- **Production:** JWT key, connection string required; Gemini required outside Development.
- **Testing:** Integration tests use SQLite in-memory; no external DB.

## Database migrations

Migrations live in `UniFlow.DataAccess/Migrations/` and were **historically scaffolded against SQL Server**. The migration history is shared; both providers use the same migration set for local dev.

Apply manually:

```bash
dotnet ef database update --project UniFlow.DataAccess --startup-project UniFlow.API
```

For EF CLI, set provider + connection via environment variables (see `UniFlowDbContextFactory`).

**Development auto-migrate:** On `dotnet run` or Docker API startup, pending migrations are applied automatically when `ASPNETCORE_ENVIRONMENT=Development`.

Add a new migration (typically against SQL Server or your active provider):

```bash
dotnet ef migrations add YourMigrationName --project UniFlow.DataAccess --startup-project UniFlow.API
```

## Health check

`GET /health` — includes EF Core database connectivity check.

## Docker

- Dockerfile: `src/backend/UniFlow.API/Dockerfile`
- Compose: repository root `docker-compose.yml`

Build API image only:

```bash
docker build -f src/backend/UniFlow.API/Dockerfile -t uniflow-api .
```

## Tests

```bash
dotnet test src/backend/UniFlow.sln
```

## Mobile integration

Mobile reads `UNIFLOW_API_BASE_URL` (see root README). Default without env var:

- Android emulator → `http://10.0.2.2:5087/` (dotnet run)
- Other → `http://127.0.0.1:5087/`

For Docker API use port **5000** and set `UNIFLOW_API_BASE_URL` accordingly.
