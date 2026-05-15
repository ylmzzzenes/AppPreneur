# UniFlow Backend

.NET 8 API (`UniFlow.API`) with Business, DataAccess, and Entity layers.

## Local development

```bash
cd src/backend
dotnet run --project UniFlow.API
```

Swagger: `http://localhost:5087/swagger` (Development only).

`appsettings.Development.json` includes only the LocalDB connection string. **Jwt:Key must be set via user-secrets** before the app will start.

## Secrets (never commit)

| Setting | User-secrets key | Environment variable |
|--------|------------------|----------------------|
| JWT signing key | `Jwt:Key` | `JWT_KEY` |
| Gemini API key | `UniFlow:Gemini:ApiKey` | `GEMINI_API_KEY` |
| Azure Document Intelligence | `UniFlow:Ocr:Azure:ApiKey` | `AZURE_DOCUMENT_INTELLIGENCE_KEY` |
| SQL connection | `ConnectionStrings:DefaultConnection` | `ConnectionStrings__DefaultConnection` |

### User secrets

From `src/backend/UniFlow.API`:

```bash
dotnet user-secrets set "Jwt:Key" "your-secret-at-least-32-characters-long"
dotnet user-secrets set "UniFlow:Gemini:ApiKey" "your-gemini-api-key"
```

List configured keys:

```bash
dotnet user-secrets list
```

### Environment variables (Production / CI)

```bash
export JWT_KEY="your-secret-at-least-32-characters-long"
export GEMINI_API_KEY="your-gemini-api-key"
export ConnectionStrings__DefaultConnection="Server=...;Database=UniFlowDb;..."
```

## Validation behavior

- **Development:** `Jwt:Key` is required (user-secrets or `JWT_KEY`). Gemini API key is optional (heuristic syllabus parsing is used when absent).
- **Production:** App fails at startup if `Jwt:Key`, `ConnectionStrings:DefaultConnection`, or (when using Azure OCR) OCR credentials are missing. Gemini API key is required outside Development.

Gemini HTTP calls send the API key in the `x-goog-api-key` header (not the query string).

## Database migrations

```bash
dotnet ef database update --project UniFlow.DataAccess --startup-project UniFlow.API
```
