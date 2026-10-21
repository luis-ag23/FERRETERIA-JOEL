# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar slnx y csproj para aprovechar cache (excluir Pruebas)
COPY ["FERRETERIA  Joel.slnx", "./"]
COPY ["FERRETERIA  Joel/FERRETERIA__Joel.Web.csproj", "FERRETERIA  Joel/"]
COPY ["FERRETERIA__Joel.Dominio/FERRETERIA__Joel.Dominio.csproj", "FERRETERIA__Joel.Dominio/"]
COPY ["FERRETERIA__Joel.Aplicacion/FERRETERIA__Joel.Aplicacion.csproj", "FERRETERIA__Joel.Aplicacion/"]
COPY ["FERRETERIA__Joel.Infraestructura/FERRETERIA__Joel.Infraestructura.csproj", "FERRETERIA__Joel.Infraestructura/"]

RUN dotnet restore "FERRETERIA  Joel/FERRETERIA__Joel.Web.csproj"

# Copiar resto del código (excluir Pruebas via .dockerignore)
COPY . .

# Publicar app web
WORKDIR /src/FERRETERIA  Joel
RUN dotnet publish "FERRETERIA__Joel.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Docker
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FERRETERIA__Joel.Web.dll"]
