# Imagen base para ASP.NET Core 8 en ejecución
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

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
RUN dotnet publish


# Configurar la aplicación para escuchar en 0.0.0.0 por el puerto 80
ENV ASPNETCORE_URLS=http://0.0.0.0:80

# Establecer el archivo de inicio
ENTRYPOINT ["dotnet", "/src/bin/Release/net8.0/BasculasPG.dll"]
