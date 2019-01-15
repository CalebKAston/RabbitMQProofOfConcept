FROM microsoft/dotnet:2.1-runtime-nanoserver-1809 AS base
WORKDIR /app

FROM microsoft/dotnet:2.1-sdk-nanoserver-1809 AS build
WORKDIR /src
COPY ContentServiceConsole/ContentServiceConsole.csproj ContentServiceConsole/
COPY RabbitClasses/RabbitClasses.csproj RabbitClasses/
RUN dotnet restore ContentServiceConsole/ContentServiceConsole.csproj
COPY ContentServiceConsole/ ContentServiceConsole/
COPY RabbitClasses/ RabbitClasses/
WORKDIR /src/ContentServiceConsole
RUN dotnet build ContentServiceConsole.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish ContentServiceConsole.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "ContentServiceConsole.dll"]
