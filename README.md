# hng-genderizeApp

ASP.NET Core Web API that creates and manages demographic profiles for a name by calling:

- `https://api.genderize.io`
- `https://api.agify.io`
- `https://api.nationalize.io`

The API stores the merged result in SQL Server via Entity Framework Core and exposes profile management endpoints.

## Requirements

- .NET SDK `9.0.306`
- SQL Server or LocalDB

## Configuration

Update the connection string in `hng-genderizeApp/appsettings.json` if needed:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=hng-genderize;Trusted_Connection=true;TrustServerCertificate=true;"
}
```

## Database setup

Apply the existing migration from the repository root:

```powershell
dotnet ef database update --project Infrastructure --startup-project hng-genderizeApp --context ApplicationDbContext
```

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
