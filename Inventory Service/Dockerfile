FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Inventory-Service.csproj", "./"]
RUN dotnet restore "./Inventory-Service.csproj"
COPY . .
RUN dotnet publish "./Inventory-Service.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Inventory-Service.dll"]
