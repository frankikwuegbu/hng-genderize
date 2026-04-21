FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY global.json ./
COPY hng-genderizeApp.sln ./
COPY Application/Application.csproj Application/
COPY Domain/Domain.csproj Domain/
COPY Infrastructure/Infrastructure.csproj Infrastructure/
COPY hng-genderizeApp/hng-genderizeApp.csproj hng-genderizeApp/

RUN dotnet restore hng-genderizeApp.sln

COPY Application/ Application/
COPY Domain/ Domain/
COPY Infrastructure/ Infrastructure/
COPY hng-genderizeApp/ hng-genderizeApp/

RUN dotnet publish hng-genderizeApp/hng-genderizeApp.csproj --no-restore -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

COPY --from=build /app/out ./

EXPOSE 8080

CMD ["sh", "-c", "ASPNETCORE_URLS=${ASPNETCORE_URLS:-http://0.0.0.0:${PORT:-8080}} dotnet hng-genderizeApp.dll"]
