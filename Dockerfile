# Imagen base para ASP.NET Core 8 en ejecución
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Imagen base para SDK de .NET Core 8 (para compilar y restaurar dependencias)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar el archivo de proyecto al contenedor
COPY *.csproj ./

# Restaurar dependencias
RUN dotnet restore

# Copiar el resto de los archivos del proyecto al contenedor
COPY . .

# Compilar la aplicación sin usar carpetas adicionales
RUN dotnet build -c Release

# Publicar la aplicación directamente en la carpeta de trabajo actual (/src)
RUN dotnet publish -c Release -o /src/publish

# Imagen final para ejecutar la aplicación
FROM base AS final
WORKDIR /app

# Copiar todo el contenido publicado desde la etapa de construcción a la carpeta actual
COPY --from=build /src/publish .

# Configurar la aplicación para escuchar en 0.0.0.0 por el puerto 8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080

# Establecer el archivo de inicio
ENTRYPOINT ["dotnet", "/app/publish/BasculaPG.dll"]
