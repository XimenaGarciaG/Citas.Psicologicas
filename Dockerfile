# ── Build stage ────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Restore con caché separada
COPY ["Citas.Psicologicas/Citas.Psicologicas.csproj", "Citas.Psicologicas/"]
RUN dotnet restore "Citas.Psicologicas/Citas.Psicologicas.csproj"

# Copiar el resto y publicar
COPY . .
WORKDIR "/src/Citas.Psicologicas"
RUN dotnet publish "Citas.Psicologicas.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ── Runtime stage ──────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Render enruta HTTP por el puerto 8080; mantener appdata escribible.
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Citas.Psicologicas.dll"]
