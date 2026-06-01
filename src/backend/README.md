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
| Gemini API key (legacy) | `UniFlow:Gemini:ApiKey` | `GEMINI_API_KEY` |
| AI provider | `Ai:Provider` | `Ai__Provider` |
| AI API key | `Ai:ApiKey` | `Ai__ApiKey` or `AI_API_KEY` |
| OpenAI-compatible base URL | `Ai:BaseUrl` | `Ai__BaseUrl` |
| AI model | `Ai:Model` | `Ai__Model` |
| AI timeout / retry | `Ai:TimeoutSeconds`, `Ai:RetryCount` | `Ai__TimeoutSeconds`, `Ai__RetryCount` |
| Prompt version | `Ai:PromptVersion` | `Ai__PromptVersion` |
| Azure Document Intelligence | `UniFlow:Ocr:Azure:ApiKey` | `AZURE_DOCUMENT_INTELLIGENCE_KEY` |
| DB provider | `Database:Provider` | `Database__Provider` |
| SQL/Postgres connection | `ConnectionStrings:DefaultConnection` | `ConnectionStrings__DefaultConnection` |

```bash
dotnet user-secrets set "Jwt:Key" "your-secret-at-least-32-characters-long"
dotnet user-secrets list
```

## Validation behavior

- **Development:** `Jwt:Key` required (user-secrets or `JWT_KEY`). AI optional — use `Ai:Provider = Fake` or leave `Ai:ApiKey` empty for heuristic syllabus parsing.
- **Production:** JWT key, connection string required; `Ai:ApiKey` required when `Ai:Provider` is not `Fake`.
- **Testing:** Integration tests use SQLite in-memory and `Ai:Provider = Fake` — no external AI calls.

## AI provider configuration

Central section in `appsettings.json` / user-secrets / environment variables:

```json
{
  "Ai": {
    "Provider": "Gemini",
    "ApiKey": "",
    "BaseUrl": "https://api.openai.com/v1",
    "Model": "gemini-2.0-flash",
    "TimeoutSeconds": 30,
    "RetryCount": 2,
    "PromptVersion": "v1",
    "EnableFallback": true,
    "LogMetadataOnly": true
  }
}
```

| `Ai:Provider` | Use case |
| ------------- | -------- |
| `Gemini` | Google Gemini REST API (default for existing setups) |
| `OpenAiCompatible` | OpenAI, OpenRouter, Groq, or any OpenAI-style `/chat/completions` gateway |
| `Fake` | Local/tests — deterministic responses; syllabus scan uses heuristic parser |

Legacy `UniFlow:Gemini` settings still bind; `GEMINI_API_KEY` maps to `Ai:ApiKey` when `Ai:ApiKey` is empty.

### Gemini

```bash
dotnet user-secrets set "Ai:Provider" "Gemini"
dotnet user-secrets set "Ai:Model" "gemini-2.0-flash"
dotnet user-secrets set "Ai:ApiKey" "YOUR_GEMINI_KEY"
# or: set GEMINI_API_KEY=...
```

### OpenAI-compatible (OpenRouter example)

Pick a current model from the [OpenRouter model list](https://openrouter.ai/models) — do not rely on hardcoded names.

```bash
dotnet user-secrets set "Ai:Provider" "OpenAiCompatible"
dotnet user-secrets set "Ai:BaseUrl" "https://openrouter.ai/api/v1"
dotnet user-secrets set "Ai:Model" "meta-llama/llama-3.2-3b-instruct:free"
dotnet user-secrets set "Ai:ApiKey" "YOUR_OPENROUTER_KEY"
```

### Groq (OpenAI-compatible)

```bash
dotnet user-secrets set "Ai:Provider" "OpenAiCompatible"
dotnet user-secrets set "Ai:BaseUrl" "https://api.groq.com/openai/v1"
dotnet user-secrets set "Ai:Model" "openai/gpt-oss-20b"
dotnet user-secrets set "Ai:ApiKey" "YOUR_GROQ_KEY"
```

### Fake provider (local / CI)

```json
"Ai": { "Provider": "Fake", "Model": "fake-model" }
```

Chat returns a deterministic stub message. Syllabus scan/confirm uses the heuristic parser (no real API call).

### AI product endpoints

All require JWT auth and are rate-limited (`RateLimitPolicies.Ai`).

| Endpoint | Description |
| -------- | ----------- |
| `POST /api/v1/ai/study-plan` | Generate N-day study plan from pending/upcoming tasks |
| `POST /api/v1/ai/task-feedback` | Status-change feedback (Done/Missed/Pending) |
| `GET /api/v1/ai/weekly-summary` | Last 7 days summary with counts |

Request example (study plan):

```json
{ "courseId": 1, "days": 7, "focus": "exam preparation" }
```

- `days`: 1–14
- `courseId`: optional; must belong to the authenticated user
- Invalid AI JSON → deterministic fallback plan (no raw response stored)

Daily dashboard message uses `PersonalizedDailyMessageService`:
- `Ai:Provider = Fake` or missing API key → template fallback
- Active provider → short AI message; failure → template fallback

### Logging & storage policy

- Only metadata is logged: provider, model, prompt version, input/output lengths, fallback flag.
- Prompts, raw OCR text, and raw AI HTTP bodies are **not** logged.
- `SyllabusTextStorage:StoreAiRawResponse` remains `false` — parsed preview JSON only when enabled.

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
