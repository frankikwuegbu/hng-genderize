# hng-genderizeApp

ASP.NET Core Web API that creates and manages demographic profiles for a name by calling:

- `https://api.genderize.io`
- `https://api.agify.io`
- `https://api.nationalize.io`

The API stores the merged result in PostgreSQL via Entity Framework Core and exposes profile management endpoints.

## Requirements

- .NET SDK `9.0.306`
- PostgreSQL

## Configuration

For local development, update the connection string in `hng-genderizeApp/appsettings.json` if needed:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=hng-genderize;Username=postgres;Password=postgrespassword"
}
```

For Railway, the app can build its PostgreSQL connection automatically from these service variables:

- `PGHOST`
- `PGPORT`
- `PGDATABASE`
- `PGUSER`
- `PGPASSWORD`

You can also set `ConnectionStrings__DefaultConnection` explicitly if you prefer.

## Database setup

Generate a fresh PostgreSQL migration from the repository root:

```powershell
dotnet ef migrations add InitialPostgres --project Infrastructure --startup-project hng-genderizeApp --context ApplicationDbContext --output-dir Migrations
```

Apply that migration to the configured PostgreSQL database:

```powershell
dotnet ef database update --project Infrastructure --startup-project hng-genderizeApp --context ApplicationDbContext
```

The previous SQL Server migration files were removed because they were provider-specific and would conflict with the PostgreSQL provider.

## Run locally

```powershell
dotnet run --project hng-genderizeApp
```

## Endpoints

- `POST /api/profiles`
- `GET /api/profiles/{id}`
- `GET /api/profiles?gender=male&country_id=NG&age_group=adult`
- `DELETE /api/profiles/{id}`

## Notes

- Duplicate names are handled idempotently and return the existing stored profile.
- All timestamps are stored in UTC.
- Profile IDs use UUID v7.
- CORS is configured with `Access-Control-Allow-Origin: *`.
