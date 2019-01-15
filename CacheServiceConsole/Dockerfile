FROM microsoft/dotnet:2.1-runtime-nanoserver-1809 AS base
WORKDIR /app

FROM microsoft/dotnet:2.1-sdk-nanoserver-1809 AS build
WORKDIR /src
COPY CacheServiceConsole/CacheServiceConsole.csproj CacheServiceConsole/
COPY RabbitClasses/RabbitClasses.csproj RabbitClasses/
RUN dotnet restore CacheServiceConsole/CacheServiceConsole.csproj
COPY CacheServiceConsole/ CacheServiceConsole/
COPY RabbitClasses/ RabbitClasses/
WORKDIR /src/CacheServiceConsole
RUN dotnet build CacheServiceConsole.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish CacheServiceConsole.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "CacheServiceConsole.dll"]
