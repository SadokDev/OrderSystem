FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Orders.Api/Orders.Api.csproj", "Orders.Api/"]
COPY ["OrderSystem.Contracts/OrderSystem.Contracts.csproj", "OrderSystem.Contracts/"]

RUN dotnet restore "Orders.Api/Orders.Api.csproj"

COPY . .

RUN dotnet publish "Orders.Api/Orders.Api.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "Orders.Api.dll"]

