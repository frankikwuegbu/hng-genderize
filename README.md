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
- `GET /api/profiles/search?q=young males from nigeria`
- `DELETE /api/profiles/{id}`

## Natural language parsing

The `GET /api/profiles/search` endpoint uses a rule-based parser only. It does not call any AI model or LLM. The implementation normalizes the query to lowercase, removes punctuation, collapses repeated whitespace, and then applies a small set of keyword and regex rules to convert plain English into the same filters used by `GET /api/profiles`.

### Supported keywords and mappings

- Gender:
  `male`, `males` -> `gender=male`
  `female`, `females` -> `gender=female`
- Age groups:
  `child`, `children` -> `age_group=child`
  `teen`, `teens`, `teenager`, `teenagers` -> `age_group=teenager`
  `adult`, `adults` -> `age_group=adult`
  `senior`, `seniors` -> `age_group=senior`
- Young:
  `young` -> `min_age=16` and `max_age=24`
  This is a parsing-only shortcut and is not a stored age group in the database.
- Minimum age phrases:
  `above 30`, `over 30`, `older than 30`, `older 30` -> `min_age=30`
- Maximum age phrases:
  `under 20`, `below 20`, `younger than 20`, `younger 20` -> `max_age=20`
- Country phrases:
  `from nigeria` -> `country_id=NG`
  `from angola` -> `country_id=AO`
  `from kenya` -> `country_id=KE`
  Country names are matched against .NET `RegionInfo` data and converted to ISO two-letter country codes.

### How the parser works

1. The query is normalized to lowercase plain text.
2. Gender keywords are checked first.
3. Age-group keywords are checked next.
4. The special `young` keyword is mapped to the age range `16-24`.
5. Regex-based age phrases are used to detect lower and upper age bounds.
6. A trailing `from <country>` phrase is resolved into a country code.
7. The resulting filters are forwarded into the normal `GET /api/profiles` filtering, sorting, and pagination pipeline.

### Example interpretations

- `young males` -> `gender=male`, `min_age=16`, `max_age=24`
- `females above 30` -> `gender=female`, `min_age=30`
- `people from angola` -> `country_id=AO`
- `adult males from kenya` -> `gender=male`, `age_group=adult`, `country_id=KE`
- `male and female teenagers above 17` -> `age_group=teenager`, `min_age=17`

## Parser limitations

The parser is intentionally narrow and rule-based, so it only handles patterns that were explicitly coded.

- It does not understand free-form semantics beyond the supported keywords and regex rules.
- It does not support sorting instructions inside the natural language query itself. Sorting must still be passed as explicit query parameters if needed.
- It does not support probability phrases such as `high confidence males` or `country probability above 0.8`.
- It does not support compound country phrasing beyond the implemented `from <country>` pattern.
- It does not support relative age phrases such as `mid twenties`, `early thirties`, `old`, or `very young`.
- It does not support negation such as `not male`, `not from nigeria`, or `non-adults`.
- It does not support OR-style filter expansion. If both male and female appear together, the parser intentionally does not apply a gender filter.
- It does not validate logical conflicts such as `young seniors`; it simply combines any recognized rules.
- It relies on `RegionInfo` country names, so uncommon aliases, abbreviations, slang, or misspellings may not resolve.
- Queries with no recognized rule return:

```json
{
  "status": "error",
  "message": "Unable to interpret query"
}
```

## Notes

- Duplicate names are handled idempotently and return the existing stored profile.
- All timestamps are stored in UTC.
- Profile IDs use UUID v7.
- CORS is configured with `Access-Control-Allow-Origin: *`.
