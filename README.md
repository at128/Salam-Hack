# SalamHack

Backend API for the SalamHack project.

Built with ASP.NET Core, Clean Architecture, CQRS, EF Core, SQL Server, Identity, JWT authentication, refresh tokens, Docker, Serilog, and OpenTelemetry.

## Run Locally

```powershell
copy .env.example .env
docker compose up -d --build --remove-orphans
```

For normal day-to-day startup after images have already been built:

```powershell
docker compose up -d --remove-orphans
```

## Local URLs

- Frontend / reverse proxy entry: `http://localhost:5009`
- API through frontend proxy: `http://localhost:5009/api/v1/health/ready`
- API Swagger direct: `http://localhost:5010/swagger/index.html`
- Health ready direct: `http://localhost:5010/api/v1/health/ready`
- Health live direct: `http://localhost:5010/api/v1/health/live`
- Seq logs: `http://localhost:5341`
- Prometheus metrics: `http://localhost:8889/metrics`
- SQL Server: `localhost,14333`

The frontend is served by Nginx in Docker on port `5009` and proxies `/api` requests to the API container. The API direct port `5010` is only for local Swagger/debug access.

## Project Structure

```text
src/
  SalamHack.Api/              Controllers, middleware, Swagger, rate limits
  SalamHack.Application/      CQRS handlers, validators, interfaces, behaviors
  SalamHack.Contracts/        Request and response DTOs
  SalamHack.Domain/           Entities, domain events, shared domain rules
  SalamHack.Infrastructure/   EF Core, Identity, JWT, persistence, caching
```

## Authentication

Auth is already wired and tested.

- `POST /api/v1/auth/register`
- `POST /api/v1/auth/login`
- `POST /api/v1/auth/refresh`
- `POST /api/v1/auth/logout`
- `POST /api/v1/auth/logout-all`
- `GET /api/v1/auth/profile`
- `PUT /api/v1/auth/profile`
- `POST /api/v1/auth/change-password`

Access tokens are returned in the response body. Refresh tokens are stored in an HttpOnly cookie and rotated on refresh.

## Database

Migrations live in:

```text
src/SalamHack.Infrastructure/Migrations
```

Add a migration:

```powershell
$env:MSBuildEnableWorkloadResolver='false'
dotnet build SalamHack.sln --no-restore -m:1
dotnet ef migrations add MigrationName --project src\SalamHack.Infrastructure\SalamHack.Infrastructure.csproj --startup-project src\SalamHack.Api\SalamHack.Api.csproj --output-dir Migrations --no-build
```

Apply migrations manually:

```powershell
dotnet ef database update --project src\SalamHack.Infrastructure\SalamHack.Infrastructure.csproj --startup-project src\SalamHack.Api\SalamHack.Api.csproj
```

In Docker development, migrations are applied automatically when the API starts.

## Development Flow

For a new feature:

1. Add domain entities or rules in `SalamHack.Domain`.
2. Add request/response DTOs in `SalamHack.Contracts`.
3. Add commands, queries, handlers, and validators in `SalamHack.Application`.
4. Add persistence configuration in `SalamHack.Infrastructure`.
5. Add API endpoints in `SalamHack.Api`.
6. Add an EF migration if the database model changed.

## Useful Commands

```powershell
dotnet restore SalamHack.sln
$env:MSBuildEnableWorkloadResolver='false'; dotnet build SalamHack.sln --no-restore -m:1
docker compose up -d --build --remove-orphans
docker compose up -d --remove-orphans
docker compose ps
docker compose logs -f api
docker compose down
```

## Notes

- `.env` is for local development secrets.
- Change `JWT_SECRET`, database passwords, and CORS origins before production.
- App-level rate limiting is enabled, but real DDoS protection should be handled before the app with a WAF/CDN/reverse proxy.
- Tests are not included in this repo right now.
