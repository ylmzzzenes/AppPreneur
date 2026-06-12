# UniFlow (AppPreneur)

**AI-powered academic planning for university students** — scan syllabi into tasks, prioritize your day with Big 3, manage courses, and get personalized AI coaching from a .NET MAUI app backed by ASP.NET Core 8.

UniFlow helps students turn messy syllabi into actionable tasks, stay focused on what matters today, and interact with an AI assistant that knows their courses and deadlines.

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-8-512BD4?logo=dotnet&logoColor=white)
![.NET MAUI](https://img.shields.io/badge/.NET%20MAUI-Android-512BD4?logo=dotnet&logoColor=white)
![EF Core](https://img.shields.io/badge/EF%20Core-8-512BD4?logo=dotnet&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169E1?logo=postgresql&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?logo=docker&logoColor=white)
![JWT](https://img.shields.io/badge/JWT-Auth-000000?logo=jsonwebtokens&logoColor=white)
![Gemini](https://img.shields.io/badge/Gemini-API-4285F4?logo=google&logoColor=white)
![Tests](https://img.shields.io/badge/tests-126%20passing-success)

| Area | Stack |
| --- | --- |
| Backend | ASP.NET Core 8, EF Core 8, FluentValidation, JWT |
| Mobile | .NET MAUI (`net8.0-android`), MVVM, SecureStorage |
| Database | PostgreSQL (Docker) · SQL Server LocalDB (dev fallback) |
| AI | Gemini · OpenAI-compatible (OpenRouter, Groq) · Fake (dev/test) |
| OCR | Stub · Gemini multimodal · Azure · Tesseract |
| Tests | **126** automated tests (82 Business + 44 API) |

---

## Table of contents

- [Features](#features)
- [Screenshots](#screenshots)
- [Architecture](#architecture)
- [AI features](#ai-features)
- [Requirements](#requirements)
- [Setup](#setup)
  - [Docker (recommended)](#setup--docker-recommended)
  - [LocalDB / dotnet run](#setup--localdb--dotnet-run)
  - [Mobile app](#setup--mobile-app)
- [Demo flow](#demo-flow)
- [Configuration](#configuration)
- [AI providers](#ai-providers)
- [API reference](#api-reference)
- [Tests](#tests)
- [Security](#security)
- [PostgreSQL migrations](#postgresql-migrations)
- [Project structure](#project-structure)
- [Documentation](#documentation)
- [GitHub About](#github-about)

---

## Features

### Identity and profile

- JWT register / login
- Onboarding: display name, major, goals, personality vibe
- Profile read and update

### Academic content

- **Syllabus:** scan PDF/image → preview → confirm → course + tasks
- **Courses:** full CRUD with per-user isolation
- **Tasks:** full CRUD, today / upcoming / all lists, status (Pending · Done · Missed)

### Dashboard and AI

- **Today:** Big 3 priorities, stats, personalized daily message
- **Weekly summary:** auto-load on dashboard + manual refresh
- **Study plan:** AI-generated multi-day plan
- **Task feedback:** AI dialog after status change
- **Chat:** personality-aware assistant with course/task context

### Mobile tabs

| Tab | Screen | Description |
| --- | --- | --- |
| Bugün | Dashboard | Big 3, weekly summary, study plan entry |
| Görevler | Tasks | Filtered task list and status actions |
| Dersler | Courses | Course CRUD |
| Sohbet | Chat | AI chat assistant |
| Müfredat | Syllabus | Scan → preview → confirm flow |

---

## Screenshots

> Place PNG files under [`docs/screenshots/`](docs/screenshots/) (see [capture guide](docs/screenshots/README.md)). Until images are added, GitHub shows broken image links — capture from Android emulator after a demo run.

| | |
| --- | --- |
| **Login** | **Onboarding** |
| ![Login](docs/screenshots/01-login.png) | ![Onboarding](docs/screenshots/03-onboarding.png) |
| **Dashboard** | **Tasks** |
| ![Dashboard](docs/screenshots/04-dashboard.png) | ![Tasks](docs/screenshots/05-tasks.png) |
| **Courses** | **Chat** |
| ![Courses](docs/screenshots/06-courses.png) | ![Chat](docs/screenshots/07-chat.png) |
| **Syllabus scan** | **Study plan** |
| ![Syllabus scan](docs/screenshots/08-syllabus-scan.png) | ![Study plan](docs/screenshots/10-study-plan.png) |
| **Weekly summary** | |
| ![Weekly summary](docs/screenshots/11-weekly-summary.png) | |

---

## Architecture

Layered backend with a thin API, business services, EF Core data access, and a MAUI client that talks to the API over JWT-authenticated HTTP.

**Full diagram:** [docs/architecture.md](docs/architecture.md)

| Layer | Project | Responsibility |
| --- | --- | --- |
| API | `UniFlow.API` | HTTP, auth, rate limits, health, Swagger |
| Business | `UniFlow.Business` | Domain logic, AI, OCR, validation |
| Data | `UniFlow.DataAccess` | EF Core, queries, migrations |
| Entity | `UniFlow.Entity` | Models, enums, result types |
| Mobile | `UniFlow.Mobile` | MAUI UI, ApiClient, MVVM |

---

## AI features

| Feature | Endpoint | Backend service | Provider | Mobile screen |
| --- | --- | --- | --- | --- |
| Daily message | `GET /api/v1/dashboard/today` | `PersonalizedDailyMessageService` | Fake / template fallback; Gemini when configured | Dashboard (Bugün) |
| Weekly summary | `GET /api/v1/ai/weekly-summary` | `WeeklySummaryService` | `AiProviderRouter` → Gemini / OpenAI-compatible / Fake | Dashboard (card + Yenile) |
| Study plan | `POST /api/v1/ai/study-plan` | `StudyPlanService` | `AiProviderRouter` | StudyPlanPage |
| Task feedback | `POST /api/v1/ai/task-feedback` | `TaskFeedbackService` | `AiProviderRouter` | Dashboard / Tasks (dialog) |
| Chat | `POST /api/v1/Chat` | `ChatService` + `ChatUserContextBuilder` | `AiProviderRouter` | ChatPage |
| Syllabus parse | `POST /api/v1/syllabus/scan` | `SyllabusParsingService` (+ heuristic fallback) | `AiProviderRouter` | SyllabusPage → Preview |
| Syllabus OCR | `POST /api/v1/syllabus/scan` | `GeminiOcrService` / Stub / Azure / Tesseract | Gemini (dev default) or configured OCR | SyllabusPage |

Production blocks the Fake AI provider at startup. OCR provider is configured separately under `UniFlow:Ocr`.

---

## Requirements

| Tool | Version | Notes |
| --- | --- | --- |
| [.NET SDK](https://dotnet.microsoft.com/download) | 8.x | Backend + mobile build |
| [Docker Desktop](https://www.docker.com/products/docker-desktop/) | Current | PostgreSQL + API (recommended) |
| Visual Studio 2022 | 17.8+ | MAUI + Android emulator |
| SQL Server LocalDB | — | Windows `dotnet run` fallback |

---

## Setup

### Setup — Docker (recommended)

PostgreSQL and API in one command. [Docker Desktop](https://www.docker.com/products/docker-desktop/) must be running.

```powershell
# From repository root
Copy-Item .env.example .env
# Set JWT_KEY to at least 32 characters in .env

docker compose config
docker compose up --build

# Health check
curl http://localhost:5000/health
```

| Service | URL |
| --- | --- |
| API | http://localhost:5000 |
| Swagger | http://localhost:5000/swagger |
| Health | http://localhost:5000/health |
| PostgreSQL | localhost:5432 |

Migrations apply automatically on API startup in Development.

---

### Setup — LocalDB / dotnet run

Fastest local demo without Docker. Use **Fake** AI provider — no API key required.

```powershell
cd src\backend\UniFlow.API
dotnet user-secrets set "Jwt:Key" "your-secret-at-least-32-characters-long"
dotnet user-secrets set "Ai:Provider" "Fake"
dotnet user-secrets set "Ai:Model" "fake-model"
dotnet run --launch-profile http
```

| Service | URL |
| --- | --- |
| Swagger | http://localhost:5087/swagger |
| Health | http://localhost:5087/health |

Default: `Database:Provider = SqlServer` with LocalDB (`appsettings.Development.json`).

**Optional — local PostgreSQL instead of LocalDB:**

```powershell
dotnet user-secrets set "Database:Provider" "PostgreSql"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=uniflow;Username=uniflow;Password=uniflow_dev_password"
dotnet run --launch-profile http
```

---

### Setup — Mobile app

The mobile client reads the API base URL from `UNIFLOW_API_BASE_URL` (`ApiConstants.cs`). Trailing slash is optional.

**1. Choose the URL for your backend**

| Scenario | Base URL |
| --- | --- |
| Android emulator + Docker API | `http://10.0.2.2:5000/` |
| Android emulator + `dotnet run` | `http://10.0.2.2:5087/` |
| iOS simulator / Windows MAUI | `http://127.0.0.1:5000/` or `5087/` |
| Physical device | `http://<LAN-IP>:5000/` (API must listen on all interfaces) |

**2. Build**

```powershell
# Docker API
$env:UNIFLOW_API_BASE_URL = "http://10.0.2.2:5000/"

# Local dotnet run
# $env:UNIFLOW_API_BASE_URL = "http://10.0.2.2:5087/"

dotnet build src/frontend/UniFlow.Mobile/UniFlow.Mobile.csproj -f net8.0-android
```

Run from Visual Studio with an Android emulator (F5). Set the environment variable before build/run.

> The mobile app uses **live API data only** — no dummy data in ViewModels.

---

## Demo flow

Use this sequence for onboarding demos or portfolio walkthroughs.

### Phase 1 — Environment

1. Start API (Docker **or** LocalDB + Fake AI).
2. Set `UNIFLOW_API_BASE_URL` for the emulator.
3. Build and launch the MAUI app.

### Phase 2 — Account

4. **Register** on the login screen.
5. Complete **onboarding** (major, goals, personality).

### Phase 3 — Core data

6. Create a **course** (Dersler → +).
7. Create a **task** manually (Görevler → +).
8. **Scan syllabus** → preview → confirm (Müfredat).

### Phase 4 — AI experience

9. Open **Dashboard** — see Big 3 and daily message.
10. Change a task **status** — trigger AI feedback dialog.
11. Generate a **study plan** (Dashboard → Çalışma planı).
12. Refresh **weekly summary** on Dashboard.

**Minimum demo (no API keys):** LocalDB setup + `Ai:Provider=Fake` + OCR `Stub` (text-only syllabus) or `Gemini` OCR if you have a Gemini key.

---

## Configuration

### Environment variables

| Variable | Purpose |
| --- | --- |
| `JWT_KEY` | JWT signing key (min 32 chars) → `Jwt:Key` |
| `ConnectionStrings__DefaultConnection` | Database connection string |
| `Database__Provider` | `SqlServer` or `PostgreSql` |
| `AI_API_KEY` / `GEMINI_API_KEY` | Fallback for `Ai:ApiKey` |
| `Ai__Provider` | `Gemini` · `OpenAiCompatible` · `Fake` |
| `Ai__ApiKey` | AI provider API key |
| `Ai__BaseUrl` | OpenAI-compatible base URL |
| `Ai__Model` | Model name (e.g. `gemini-2.5-flash`) |
| `Ai__TimeoutSeconds` | HTTP timeout |
| `Ai__RetryCount` | Retry count |
| `Ai__EnableFallback` | Local fallback when key missing (dev) |
| `AZURE_DOCUMENT_INTELLIGENCE_KEY` | Azure OCR |
| `UNIFLOW_API_BASE_URL` | Mobile API base URL |
| `POSTGRES_*` | Docker Compose PostgreSQL (`.env.example`) |

Full template: [`src/backend/UniFlow.API/appsettings.example.json`](src/backend/UniFlow.API/appsettings.example.json)

### Secrets (local development)

```powershell
cd src\backend\UniFlow.API
dotnet user-secrets set "Jwt:Key" "your-secret-at-least-32-characters-long"
dotnet user-secrets list
```

- Never commit `.env` (listed in `.gitignore`).
- `appsettings.json` ships with empty secrets — use user-secrets or environment variables.

---

## AI providers

Primary configuration is the **`Ai`** section. Legacy `UniFlow:Gemini` and env vars `GEMINI_API_KEY` / `AI_API_KEY` still bind to `Ai:ApiKey`.

On startup, **`UniFlow.Ai.Configuration`** logs a safe summary (provider, model, key configured — never the raw key).

| Provider | Use case | Production |
| --- | --- | --- |
| `Fake` | Dev / CI — deterministic responses, heuristic syllabus parse | **Blocked** (fail-fast) |
| `Gemini` | Google Gemini REST API | ApiKey required |
| `OpenAiCompatible` | OpenRouter, Groq, OpenAI | ApiKey + BaseUrl required |

<details>
<summary><strong>Fake (local demo)</strong></summary>

```powershell
dotnet user-secrets set "Ai:Provider" "Fake"
dotnet user-secrets set "Ai:Model" "fake-model"
```

Docker `.env`:

```env
Ai__Provider=Fake
Ai__Model=fake-model
```

</details>

<details>
<summary><strong>Gemini</strong></summary>

```powershell
dotnet user-secrets set "Ai:Provider" "Gemini"
dotnet user-secrets set "Ai:Model" "gemini-2.5-flash"
dotnet user-secrets set "Ai:ApiKey" "YOUR_AI_STUDIO_KEY"
```

Docker `.env`:

```env
Ai__Provider=Gemini
Ai__Model=gemini-2.5-flash
Ai__ApiKey=YOUR_AI_STUDIO_KEY
```

</details>

<details>
<summary><strong>OpenRouter / Groq</strong></summary>

OpenRouter:

```powershell
dotnet user-secrets set "Ai:Provider" "OpenAiCompatible"
dotnet user-secrets set "Ai:BaseUrl" "https://openrouter.ai/api/v1"
dotnet user-secrets set "Ai:Model" "meta-llama/llama-3.2-3b-instruct:free"
dotnet user-secrets set "Ai:ApiKey" "YOUR_OPENROUTER_KEY"
```

Groq:

```powershell
dotnet user-secrets set "Ai:BaseUrl" "https://api.groq.com/openai/v1"
dotnet user-secrets set "Ai:Model" "llama-3.1-8b-instant"
```

</details>

More detail: [Backend README](src/backend/README.md)

---

## API reference

Protected endpoints require `Authorization: Bearer <token>`.

### Auth

| Method | Route | Description |
| --- | --- | --- |
| POST | `/api/v1/auth/register` | Register |
| POST | `/api/v1/auth/login` | Login |

### Users

| Method | Route | Description |
| --- | --- | --- |
| GET | `/api/v1/users/me` | Profile |
| PATCH | `/api/v1/users/me/onboarding` | Update onboarding |

### Dashboard

| Method | Route | Description |
| --- | --- | --- |
| GET | `/api/v1/dashboard/today` | Big 3, stats, daily message |

### Courses

| Method | Route | Description |
| --- | --- | --- |
| GET / POST | `/api/v1/courses` | List / create |
| GET / PUT / DELETE | `/api/v1/courses/{id}` | Detail / update / delete |

### Tasks

| Method | Route | Description |
| --- | --- | --- |
| GET | `/api/v1/tasks` | All tasks |
| GET | `/api/v1/tasks/today` | Today's tasks |
| GET | `/api/v1/tasks/upcoming` | Upcoming tasks |
| POST / PUT / DELETE | `/api/v1/tasks` … | CRUD |
| PATCH | `/api/v1/tasks/{id}/status` | Update status |

### Syllabus

| Method | Route | Description |
| --- | --- | --- |
| POST | `/api/v1/syllabus/scan` | Scan file → preview session |
| POST | `/api/v1/syllabus/confirm` | Confirm → course + tasks |
| POST | `/api/v1/syllabus/ingest` | One-step ingest (legacy) |

### AI and chat

| Method | Route | Description |
| --- | --- | --- |
| POST | `/api/v1/ai/study-plan` | Study plan |
| POST | `/api/v1/ai/task-feedback` | Task feedback |
| GET | `/api/v1/ai/weekly-summary` | Weekly summary |
| POST | `/api/v1/Chat` | Chat |

### System

| Method | Route | Description |
| --- | --- | --- |
| GET | `/health` | Database connectivity check |

---

## Tests

```powershell
# Backend — build and test (126 tests)
dotnet build src/backend/UniFlow.sln
dotnet test src/backend/UniFlow.sln

# Run a single project
dotnet test src/backend/tests/UniFlow.Business.Tests
dotnet test src/backend/tests/UniFlow.API.Tests

# Mobile — build only (no automated UI tests yet)
dotnet build src/frontend/UniFlow.Mobile/UniFlow.Mobile.csproj -f net8.0-android
```

| Suite | Tests | Type |
| --- | --- | --- |
| `UniFlow.Business.Tests` | 82 | Unit (AI, syllabus, services, validation) |
| `UniFlow.API.Tests` | 44 | Integration (SQLite in-memory, `Testing` env) |
| **Total** | **126** | Fake AI provider in test environment |

Integration tests do **not** require PostgreSQL or live AI API keys.

---

## Security

### Secrets and configuration

- Repository ships without production secrets; `appsettings.json` uses empty placeholders.
- `.env` and user-secrets are excluded from version control.
- JWT signing key must be at least 32 characters; validated at startup.

### Authentication and authorization

- JWT bearer auth on all protected endpoints.
- User identity from `NameIdentifier` / `sub` claim; services enforce ownership via `GetOwnedAsync` queries.
- Cross-user access returns **404** (tasks, courses) or **403** (syllabus scan sessions) to reduce enumeration.

### AI and data handling

- API keys are never written to logs — only metadata (provider, model, payload lengths).
- Raw prompts, OCR text, and AI HTTP bodies are not persisted by default (`StoreRawSourceText`, `StoreAiRawResponse` off).
- Fake AI provider is rejected in Production at startup validation.

### Upload and abuse controls

- Syllabus uploads: size limit (10 MB), extension and MIME whitelist.
- Rate limiting on AI endpoints and file upload routes (per-user partition when authenticated).

### Production checklist

| Item | Required |
| --- | --- |
| Strong `JWT_KEY` / `Jwt:Key` | Yes |
| Database connection string | Yes |
| `Ai:ApiKey` (non-Fake provider) | Yes |
| `Ai:Provider` ≠ `Fake` | Yes |
| HTTPS termination | Recommended |
| OCR provider keys (if not using Stub/Gemini dev defaults) | As needed |

---

## PostgreSQL migrations

EF Core migrations were historically scaffolded against **SQL Server**. The same migration history is applied when `Database:Provider=PostgreSql` (Docker dev).

| Situation | Action |
| --- | --- |
| Migrations succeed | `docker compose up` + `/health` is enough |
| Provider-specific SQL error | Use LocalDB fallback or report the issue |
| Reset migration history | **Do not** — causes data loss and team drift |

Manual update from host (Postgres in Docker):

```powershell
$env:Database__Provider = "PostgreSql"
$env:ConnectionStrings__DefaultConnection = "Host=localhost;Port=5432;Database=uniflow;Username=uniflow;Password=uniflow_dev_password"
dotnet ef database update --project src/backend/UniFlow.DataAccess --startup-project src/backend/UniFlow.API
```

See [Backend README](src/backend/README.md) for provider switching details.

---

## Project structure

```
AppPreneur/
├── docs/
│   ├── architecture.md          # System diagram and layer notes
│   └── screenshots/             # Portfolio screenshots (see README inside)
├── src/
│   ├── backend/
│   │   ├── UniFlow.API/         # HTTP API, controllers, Dockerfile
│   │   ├── UniFlow.Business/    # Domain, AI, OCR, validation
│   │   ├── UniFlow.DataAccess/  # EF Core, migrations, queries
│   │   ├── UniFlow.Entity/      # Entities, enums, results
│   │   └── tests/               # Business + API test projects
│   └── frontend/
│       └── UniFlow.Mobile/      # .NET MAUI mobile app
├── docker-compose.yml
├── .env.example
└── README.md
```

---

## Documentation

| Resource | Content |
| --- | --- |
| [Architecture](docs/architecture.md) | Mermaid diagram, layers, key flows |
| [Screenshots guide](docs/screenshots/README.md) | Capture and naming conventions |
| [Backend README](src/backend/README.md) | Secrets, migrations, AI/OCR, health |
| [Technical PRD](UniFlow%20Teknik%20PRD%20v1.0.md) | Product and technical requirements |
| `appsettings.example.json` | Full configuration template |

---

## GitHub About

**Suggested repository description:**

> AI-powered academic planning app for university students built with ASP.NET Core 8, .NET MAUI, EF Core, PostgreSQL, JWT and Gemini/OpenAI-compatible providers.

**Suggested topics:**

`dotnet` · `aspnet-core` · `dotnet-maui` · `ef-core` · `postgresql` · `jwt-authentication` · `ai` · `gemini-api` · `openai-compatible` · `mobile-app` · `docker` · `clean-architecture` · `academic-planner`

---

<p align="center">
  <sub>UniFlow · AppPreneur · .NET 8 · Demo-ready full stack</sub>
</p>
