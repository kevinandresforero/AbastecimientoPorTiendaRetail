# Imagen de la API (multi-stage). Contexto de build: raiz del repositorio (necesita Directory.Build.props).
# Uso: docker compose up --build   (ver docker-compose.yml)

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 1) Restaurar paquetes copiando solo los archivos de proyecto (aprovecha la cache de capas)
COPY Directory.Build.props ./
COPY src/Abastecimiento.Dominio/Abastecimiento.Dominio.csproj src/Abastecimiento.Dominio/
COPY src/Abastecimiento.Aplicacion/Abastecimiento.Aplicacion.csproj src/Abastecimiento.Aplicacion/
COPY src/Abastecimiento.Infraestructura/Abastecimiento.Infraestructura.csproj src/Abastecimiento.Infraestructura/
COPY src/Abastecimiento.Api/Abastecimiento.Api.csproj src/Abastecimiento.Api/
RUN dotnet restore src/Abastecimiento.Api/Abastecimiento.Api.csproj

# 2) Compilar y publicar
COPY src/ src/
RUN dotnet publish src/Abastecimiento.Api/Abastecimiento.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
USER app
ENTRYPOINT ["dotnet", "Abastecimiento.Api.dll"]
