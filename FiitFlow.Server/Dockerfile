# build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY . .
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .

RUN dotnet restore FiitFlow.Server/FiitFlow.Server.csproj -p:ExcludeClientBuild=true
RUN dotnet publish FiitFlow.Server/FiitFlow.Server.csproj -c Release -o /app/publish /p:UseAppHost=false -p:ExcludeClientBuild=true

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS run
WORKDIR /app
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet","FiitFlow.Server.dll"]

RUN dotnet publish FiitFlow.Server/FiitFlow.Server.csproj -c Release -o /app/publish /p:UseAppHost=false

# run
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS run
WORKDIR /app
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FiitFlow.Server.dll"]

