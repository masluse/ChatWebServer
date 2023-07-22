# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app

# Kopiere den Projektordner und führe einen Restore durch, um die Abhängigkeiten zu installieren
COPY ./*.csproj ./
RUN dotnet restore

# Kopiere den Rest des Codes und führe den Build durch
COPY . ./
RUN dotnet publish -c Release -o out

# Stage 2: Erstelle das Produktions-Image
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./

# Starte die Webanwendung beim Start des Containers
ENTRYPOINT ["dotnet", "ChatWebServer.dll"]
