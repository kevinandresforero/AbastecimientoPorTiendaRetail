# Imagen del frontend Blazor (multi-stage). Contexto de build: raiz del repositorio (necesita Directory.Build.props).
# Uso: docker compose up -d --build   (ver docker-compose.yml; el front queda en http://localhost:3000)

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 1) Restaurar paquetes copiando solo el archivo de proyecto (aprovecha la cache de capas)
COPY Directory.Build.props ./
COPY front/Abastecimiento.Front/Abastecimiento.Front.csproj front/Abastecimiento.Front/
RUN dotnet restore front/Abastecimiento.Front/Abastecimiento.Front.csproj

# 2) Compilar y publicar (solo front/; las pruebas quedan fuera por .dockerignore)
COPY front/ front/
RUN dotnet publish front/Abastecimiento.Front/Abastecimiento.Front.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
USER app
ENTRYPOINT ["dotnet", "Abastecimiento.Front.dll"]
