FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY services/orders/src/Versta.Orders.Domain/Versta.Orders.Domain.csproj services/orders/src/Versta.Orders.Domain/
COPY services/orders/src/Versta.Orders.Application/Versta.Orders.Application.csproj services/orders/src/Versta.Orders.Application/
COPY services/orders/src/Versta.Orders.Infrastructure/Versta.Orders.Infrastructure.csproj services/orders/src/Versta.Orders.Infrastructure/
COPY services/orders/src/Versta.Orders.Api/Versta.Orders.Api.csproj services/orders/src/Versta.Orders.Api/
RUN dotnet restore services/orders/src/Versta.Orders.Api/Versta.Orders.Api.csproj
COPY services/orders/src services/orders/src
RUN dotnet publish services/orders/src/Versta.Orders.Api/Versta.Orders.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Versta.Orders.Api.dll"]
