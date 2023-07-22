# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app

# Kopiere das Projekt und die Restore-Anweisungen separat,
# damit das Docker-Cache effizient genutzt wird
COPY *.csproj ./
RUN dotnet restore

# Kopiere den Rest des Codes und builden der Anwendung
COPY . ./
RUN dotnet publish -c Release -o out

# Stage 2: Run the application
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./

# Definiere den Befehl, der beim Ausführen des Containers ausgeführt wird
ENTRYPOINT ["dotnet", "ChatWebServer.dll"]
