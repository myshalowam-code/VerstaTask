FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY gateway/Versta.Gateway/Versta.Gateway.csproj gateway/Versta.Gateway/
RUN dotnet restore gateway/Versta.Gateway/Versta.Gateway.csproj
COPY gateway/Versta.Gateway gateway/Versta.Gateway
RUN dotnet publish gateway/Versta.Gateway/Versta.Gateway.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Versta.Gateway.dll"]
