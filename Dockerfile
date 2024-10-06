# Imagen base para ASP.NET Core 8 en ejecución
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Imagen base para SDK de .NET Core 8 (para compilar y restaurar dependencias)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar el archivo de proyecto y restaurar las dependencias
COPY ["BasculaPG/BasculaPG.csproj", "BasculaPG/"]
RUN dotnet restore "BasculaPG/BasculaPG.csproj"

# Copiar todos los archivos de la aplicación y compilarla
COPY . .
WORKDIR "/src/BasculaPG"
RUN dotnet build "BasculaPG.csproj" -c Release -o /app/build

# Publicar la aplicación
FROM build AS publish
RUN dotnet publish "BasculaPG.csproj" -c Release -o /app/publish

# Imagen final para ejecutar la aplicación en un entorno más ligero
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Configuración para escuchar en todos los puertos (0.0.0.0) en el puerto 8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
ENTRYPOINT ["dotnet", "BasculaPG.dll"]


