FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

WORKDIR /app
COPY SanDoku.* ./
COPY SanDoku SanDoku
RUN dotnet tool restore
RUN dotnet publish -c Release -r linux-x64 SanDoku.sln

FROM mcr.microsoft.com/dotnet/runtime:6.0
WORKDIR /app
COPY --from=build /app/SanDoku/bin/Release/net6.0/linux-x64/publish .
ENTRYPOINT ["dotnet", "SanDoku.dll"]
EXPOSE 80