# build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore FiitFlow.Server/FiitFlow.Server.csproj -p:ExcludeClientBuild=true
RUN dotnet publish FiitFlow.Server/FiitFlow.Server.csproj -c Release -o /app/publish /p:UseAppHost=false -p:ExcludeClientBuild=true

# run
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS run
WORKDIR /app
RUN mkdir -p /data /app/cfg
ENV ASPNETCORE_URLS=http://0.0.0.0:8080 \
    FIITFLOW_ROOT_PATH=/app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet","FiitFlow.Server.dll"]
