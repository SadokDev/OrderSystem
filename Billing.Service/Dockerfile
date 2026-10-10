FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Billing.Service/Billing.Service.csproj", "Billing.Service/"]
COPY ["OrderSystem.Contracts/OrderSystem.Contracts.csproj", "OrderSystem.Contracts/"]

RUN dotnet restore "Billing.Service/Billing.Service.csproj"

COPY . .

RUN dotnet publish "Billing.Service/Billing.Service.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Billing.Service.dll"]