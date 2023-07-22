FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

WORKDIR /app

COPY ChatWebServer.csproj .

RUN dotnet restore

COPY . .

RUN dotnet build -c Release --no-restore

RUN dotnet publish -c Release -o out --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime

WORKDIR /app

COPY --from=build /app/out .

EXPOSE 80

ENV ASPNETCORE_URLS=http://+/

ENTRYPOINT ["dotnet", "ChatWebServer.dll"]