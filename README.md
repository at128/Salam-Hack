# SalamHack [![CI/CD Pipeline](https://github.com/at128/Salam-Hack/actions/workflows/ci-cd.yml/badge.svg?branch=master)](https://github.com/at128/Salam-Hack/actions/workflows/ci-cd.yml)

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
- Swagger through frontend proxy: `http://localhost:5009/swagger/index.html`
- API Swagger direct: `http://localhost:5010/swagger/index.html`
- Health ready direct: `http://localhost:5010/api/v1/health/ready`
- Health live direct: `http://localhost:5010/api/v1/health/live`
- Seq logs: `http://localhost:5341`
- OpenTelemetry collector metrics: `http://localhost:8889/metrics`
- Prometheus: `http://localhost:9090`
- Grafana: `http://localhost:3000`
- SQL Server: `localhost,14333`

The frontend is served by Nginx in Docker on port `5009` and proxies `/api` requests to the API container. The API direct port `5010` is only for local Swagger/debug access.

## Production

Production is deployed from `master` by GitHub Actions using `docker-compose.prod.yml`.
Keep real server IPs, SSH key paths, filesystem paths, and passwords out of this file. Store those details in the server `.env`, GitHub secrets, or private ops notes.

- Public app: `https://<your-domain>`
- Public API health: `https://<your-domain>/api/v1/health/live`
- Public Swagger: `https://<your-domain>/swagger/index.html`

On the server:

```bash
cd <server-project-dir>
docker compose -f docker-compose.prod.yml ps
docker compose -f docker-compose.prod.yml logs -f api
```

## Observability

The stack includes Seq for structured logs and OpenTelemetry metrics exported through Prometheus into Grafana.

- Seq receives Serilog events from the API and is best for searching request logs, exceptions, correlation IDs, and slow request log entries.
- The OpenTelemetry collector receives OTLP metrics from the API on `4317` and exposes Prometheus-format metrics on `8889`.
- Prometheus scrapes the collector and stores time-series data.
- Grafana is provisioned with a Prometheus datasource and the `SalamHack API Overview` dashboard.

Local credentials are controlled by `.env`:

```powershell
GRAFANA_ADMIN_USER=admin
GRAFANA_ADMIN_PASSWORD=<strong-local-password>
```

Production observability endpoints depend on the server host, ports, and firewall rules. In `docker-compose.prod.yml`, Seq, Prometheus, and Grafana bind to `127.0.0.1` by default so they are reached through SSH tunneling unless you explicitly change the bind address in `.env`.

- Seq: `http://<server-host>:<seq-port>`
- Prometheus: `http://<server-host>:<prometheus-port>`
- Grafana: `http://<server-host>:<grafana-port>`

If those ports are bound to localhost or closed by firewall rules, use an SSH tunnel from your machine:

```powershell
ssh -i "<private-key-path>" -L 3000:127.0.0.1:<grafana-port> -L 9090:127.0.0.1:<prometheus-port> -L 5341:127.0.0.1:<seq-port> <user>@<server-host>
```

Then open:

- Grafana: `http://localhost:3000`
- Prometheus: `http://localhost:9090`
- Seq: `http://localhost:5341`

Grafana is provisioned with the `SalamHack API Overview` dashboard. Prometheus scrapes OpenTelemetry metrics from `otel-collector:8889`, and Grafana reads Prometheus through the built-in provisioned datasource.

For production, set a strong `GRAFANA_ADMIN_PASSWORD` in the server `.env` and avoid exposing Grafana or Prometheus publicly unless access is protected.

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
- Do not commit real server IPs, SSH key paths, passwords, or production filesystem paths.
- Change `JWT_SECRET`, database passwords, and CORS origins before production.
- App-level rate limiting is enabled, but real DDoS protection should be handled before the app with a WAF/CDN/reverse proxy.
- Tests are not included in this repo right now.
