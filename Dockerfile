FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build

WORKDIR /app
COPY SanDoku.* ./
COPY SanDoku SanDoku
RUN dotnet tool restore
RUN dotnet publish -c Release -r linux-x64 -p:PublishReadyToRun=true SanDoku.sln

FROM mcr.microsoft.com/dotnet/runtime:5.0
WORKDIR /app
COPY --from=build /app/SanDoku/bin/Release/net5.0/linux-x64/publish .
ENTRYPOINT ["dotnet", "SanDoku.dll"]
EXPOSE 80