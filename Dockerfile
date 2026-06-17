FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia csproj separadamente para aproveitar cache de restore
COPY src/Borgonha.Domain/Borgonha.Domain.csproj src/Borgonha.Domain/
COPY src/Borgonha.Infrastructure/Borgonha.Infrastructure.csproj src/Borgonha.Infrastructure/
COPY src/Borgonha.Service/Borgonha.Service.csproj src/Borgonha.Service/
COPY src/Borgonha.Api/Borgonha.Api.csproj src/Borgonha.Api/
RUN dotnet restore src/Borgonha.Api/Borgonha.Api.csproj

COPY src/ src/
RUN dotnet publish src/Borgonha.Api/Borgonha.Api.csproj \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Borgonha.Api.dll"]
