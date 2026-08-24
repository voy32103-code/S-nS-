FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish backend/SanSo.Migrator.V8/SanSo.Migrator.V8.csproj -c Release -o /out --no-self-contained

FROM mcr.microsoft.com/dotnet/runtime:9.0 AS runtime
WORKDIR /app
COPY --from=build /out .
USER app
ENTRYPOINT ["dotnet","SanSo.Migrator.V8.dll"]

