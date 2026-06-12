# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/backend/UniFlow.sln ./
COPY src/backend/UniFlow.API/UniFlow.API.csproj UniFlow.API/
COPY src/backend/UniFlow.Business/UniFlow.Business.csproj UniFlow.Business/
COPY src/backend/UniFlow.DataAccess/UniFlow.DataAccess.csproj UniFlow.DataAccess/
COPY src/backend/UniFlow.Entity/UniFlow.Entity.csproj UniFlow.Entity/

RUN dotnet restore UniFlow.sln

COPY src/backend/ ./
WORKDIR /src/UniFlow.API
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .

HEALTHCHECK --interval=30s --timeout=5s --start-period=40s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "UniFlow.API.dll"]
