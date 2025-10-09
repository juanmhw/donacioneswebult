# syntax=docker/dockerfile:1

#########################
# Etapa de build
#########################
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiamos los csproj primero para aprovechar caché
COPY webDonaciones.sln .
COPY donacionesWeb/donacionesWeb.csproj donacionesWeb/
# (opcional: si existe el proyecto de tests, lo copiamos solo para el restore de la solución)
# COPY donacionesWeb.Tests/donacionesWeb.Tests.csproj donacionesWeb.Tests/

# Restaurar dependencias (puedes restaurar solo el proyecto web para evitar tests)
RUN dotnet restore donacionesWeb/donacionesWeb.csproj

# Copiar el resto del código
COPY . .

# Publicar SOLO el proyecto web (no la solución)
RUN dotnet publish donacionesWeb/donacionesWeb.csproj -c Release -o /app/publish /p:UseAppHost=false

#########################
# Etapa de runtime
#########################
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .

# Ejecuta el .dll del proyecto web
ENTRYPOINT ["dotnet","donacionesWeb.dll"]
