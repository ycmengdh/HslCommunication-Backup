# MgTx.Service Dockerfile - 猛攻通讯
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/MgTx.Core/MgTx.Core.csproj", "src/MgTx.Core/"]
COPY ["src/MgTx.Plc/MgTx.Plc.csproj", "src/MgTx.Plc/"]
COPY ["src/MgTx.Network/MgTx.Network.csproj", "src/MgTx.Network/"]
COPY ["src/MgTx.Service/MgTx.Service.csproj", "src/MgTx.Service/"]

# Restore dependencies
RUN dotnet restore "src/MgTx.Service/MgTx.Service.csproj"

# Copy everything
COPY . .

# Build and publish
WORKDIR "/src/src/MgTx.Service"
RUN dotnet build "MgTx.Service.csproj" -c Release -o /app/build
RUN dotnet publish "MgTx.Service.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

# Create logs directory
RUN mkdir -p /app/logs

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "MgTx.Service.dll"]
