# UniFlow Architecture

High-level view of the UniFlow / AppPreneur stack: .NET MAUI mobile client, ASP.NET Core API, layered backend, and pluggable AI/OCR providers.

## System diagram

```mermaid
flowchart TB
    subgraph Client["Mobile client"]
        MAUI["UniFlow.Mobile<br/>.NET MAUI Android"]
    end

    subgraph API["UniFlow.API"]
        Auth["Auth / JWT"]
        CRUD["Courses & Tasks"]
        Syl["Syllabus scan / confirm"]
        Dash["Dashboard"]
        AI["AI & Chat controllers"]
    end

    subgraph Business["UniFlow.Business"]
        Svc["Domain services"]
        Val["FluentValidation"]
        Router["AiProviderRouter"]
    end

    subgraph Data["UniFlow.DataAccess"]
        EF["EF Core 8"]
        Q["Query layer"]
    end

    subgraph Store["Database"]
        PG[("PostgreSQL")]
        SS[("SQL Server LocalDB")]
    end

    subgraph External["External providers"]
        Gemini["Gemini API"]
        OAI["OpenAI-compatible"]
        Fake["Fake provider"]
        OCR["OCR: Stub / Gemini / Azure / Tesseract"]
    end

    MAUI -->|"HTTPS + JWT"| API
    API --> Business
    Business --> Data
    EF --> PG
    EF --> SS
    Syl --> OCR
    AI --> Router
    Router --> Gemini
    Router --> OAI
    Router --> Fake
```

## Layer responsibilities

| Layer | Project | Responsibility |
| --- | --- | --- |
| API | `UniFlow.API` | HTTP endpoints, JWT auth, rate limiting, health checks, Swagger |
| Business | `UniFlow.Business` | Domain logic, AI orchestration, OCR, validation, prompts |
| Data | `UniFlow.DataAccess` | EF Core DbContext, repositories, queries, migrations |
| Entity | `UniFlow.Entity` | Persistence models, enums, `Result<T>` types, read models |
| Mobile | `UniFlow.Mobile` | MAUI UI, MVVM, `ApiClient`, secure token storage |

## Key flows

### Syllabus ingestion

1. Mobile uploads multipart form → `POST /api/v1/syllabus/scan`
2. File validation → OCR → AI/heuristic parse → preview session (user-bound)
3. User edits selection → `POST /api/v1/syllabus/confirm`
4. Transaction creates or reuses course, syllabus, and task items

### AI products

1. Controller resolves `userId` from JWT
2. Business service loads user-scoped data (courses, tasks, profile)
3. `AiProviderRouter` selects Gemini, OpenAI-compatible, or Fake provider
4. Response parsed with JSON extractors and fallback builders where applicable

## Related docs

- [Root README](../README.md)
- [Backend README](../src/backend/README.md)
