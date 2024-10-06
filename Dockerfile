# Imagen base para ASP.NET Core 8 en ejecución
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Imagen base para SDK de .NET Core 8 (para compilar y restaurar dependencias)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar el archivo de proyecto
COPY *.csproj ./ 

# Restaurar dependencias
RUN dotnet restore

# Copiar el resto de los archivos del proyecto
COPY . .

# Compilar la aplicación
RUN dotnet build -c Release -o /app/build

# Publicar la aplicación
RUN dotnet publish -c Release -o /app/publish

# Imagen final para ejecutar la aplicación
FROM base AS final
WORKDIR /app

# Copiar los archivos publicados a la carpeta raíz del contenedor
COPY --from=build /app/publish .

# Configurar la aplicación para escuchar en 0.0.0.0 por el puerto 8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080

# Establecer el archivo de inicio
ENTRYPOINT ["dotnet", "BasculaPG.dll"]
