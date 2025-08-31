FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# install wget to support health checks
RUN apt-get update && \
    apt-get install -y wget && \
    rm -rf /var/lib/apt/lists/* && \
    apt-get clean

# 🔥 ACCEPT BUILD ARGS
ARG ENVIRONMENT=Development
ARG API_BASE_URL=http://localhost:5234

# Copy project files
COPY ["ClaimIq.Web/ClaimIq.Web.csproj", "ClaimIq.Web/"]
COPY ["ClaimIq.Domain/ClaimIq.Domain.csproj", "ClaimIq.Domain/"]

RUN dotnet restore "ClaimIq.Web/ClaimIq.Web.csproj"

# Copy source code
COPY ClaimIq.Web/ ClaimIq.Web/
COPY ClaimIq.Domain/ ClaimIq.Domain/

WORKDIR "/src/ClaimIq.Web"

# 🔥 INJECT CONFIG AT BUILD TIME!
RUN sed -i "s|\"ApiBaseUrl\":.*|\"ApiBaseUrl\": \"${API_BASE_URL}\"|g" wwwroot/appsettings.json

# Publish Blazor WASM
RUN dotnet publish "ClaimIq.Web.csproj" -c Release -o /app/publish

FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html

# Copy nginx config
COPY ClaimIq.Web/nginx.conf /etc/nginx/nginx.conf

# Copy the Blazor WASM files
COPY --from=build /app/publish/wwwroot/ .

EXPOSE 8080
CMD ["nginx", "-g", "daemon off;"]