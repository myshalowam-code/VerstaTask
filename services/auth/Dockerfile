FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY services/auth/src/Versta.Auth.Api/Versta.Auth.Api.csproj services/auth/src/Versta.Auth.Api/
RUN dotnet restore services/auth/src/Versta.Auth.Api/Versta.Auth.Api.csproj
COPY services/auth/src services/auth/src
RUN dotnet publish services/auth/src/Versta.Auth.Api/Versta.Auth.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Versta.Auth.Api.dll"]
