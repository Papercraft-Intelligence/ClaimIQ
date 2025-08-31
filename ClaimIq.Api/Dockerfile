FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# install wget to support health checks
RUN apt-get update && \
    apt-get install -y wget && \
    rm -rf /var/lib/apt/lists/* && \
    apt-get clean

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files (from root context)
COPY ["ClaimIq.Api/ClaimIq.Api.csproj", "ClaimIq.Api/"]
COPY ["ClaimIq.Domain/ClaimIq.Domain.csproj", "ClaimIq.Domain/"]

# Restore dependencies
RUN dotnet restore "ClaimIq.Api/ClaimIq.Api.csproj"

# Copy source code (from root context)
COPY ClaimIq.Api/ ClaimIq.Api/
COPY ClaimIq.Domain/ ClaimIq.Domain/

WORKDIR "/src/ClaimIq.Api"
RUN dotnet build "ClaimIq.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ClaimIq.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ClaimIq.Api.dll"]