# Sistema de Gestion - Mundial de Futbol Corporativo (.NET 6)

Implementacion con:

- Clean Architecture (Domain, Application, Infrastructure, Api)
- CQRS (Commands con EF Core + UnitOfWork, Queries con Dapper)
- Result Pattern (Result, Result<T>)
- REST completo e idempotencia en POST con header `Idempotency-Key`
- CorrelationId/TraceId por request y logging estructurado con Serilog
- Seed automatico al iniciar si la DB esta vacia
- Docker Compose (API + PostgreSQL)

## Ejecutar con Docker

```bash
docker compose up --build
```

API: http://localhost:8080
Swagger: http://localhost:8080/swagger

## Ejecutar local

1. Levantar PostgreSQL local con credenciales de `appsettings.Development.json`.
2. Ejecutar:

```bash
dotnet run --project MundialCorporativo.Api
```

## Endpoints principales

- `GET /api/teams`
- `POST /api/teams` (requiere `Idempotency-Key`)
- `PUT /api/teams/{id}`
- `PATCH /api/teams/{id}`
- `DELETE /api/teams/{id}`
- `GET /api/players`
- `POST /api/players/teams/{teamId}` (requiere `Idempotency-Key`)
- `GET /api/matches`
- `POST /api/matches` (requiere `Idempotency-Key`)
- `POST /api/matches/{id}/result` (requiere `Idempotency-Key`)
- `GET /api/standings`
- `GET /api/standings/top-scorers`

## Paginacion y filtros

Todos los GET de listas admiten:

- `pageNumber`
- `pageSize`
- `sortBy`
- `sortDirection`
- filtros por entidad (`name`, `teamId`, `status`, `dateFromUtc`, `dateToUtc`)

Respuesta paginada:

```json
{
  "data": [],
  "pageNumber": 1,
  "pageSize": 10,
  "totalRecords": 50,
  "totalPages": 5
}
```
