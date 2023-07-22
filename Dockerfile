FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 7144

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["ChatWebServer/ChatWebServer.csproj", "."]
RUN dotnet restore "./ChatWebServer/ChatWebServer.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "ChatWebServer/ChatWebServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ChatWebServer/ChatWebServer.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ChatWebServer.dll"]