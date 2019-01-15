FROM microsoft/dotnet:2.1-runtime-nanoserver-1809 AS base
WORKDIR /app

FROM microsoft/dotnet:2.1-sdk-nanoserver-1809 AS build
WORKDIR /src
COPY CustomerServiceConsole/CustomerServiceConsole.csproj CustomerServiceConsole/
COPY RabbitClasses/RabbitClasses.csproj RabbitClasses/
RUN dotnet restore CustomerServiceConsole/CustomerServiceConsole.csproj
COPY CustomerServiceConsole/ CustomerServiceConsole/
COPY RabbitClasses/ RabbitClasses/
WORKDIR /src/CustomerServiceConsole
RUN dotnet build CustomerServiceConsole.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish CustomerServiceConsole.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "CustomerServiceConsole.dll"]
