#### BUILD STAGE ####
FROM mcr.microsoft.com/dotnet/sdk:9.0 as build
WORKDIR /src

# Copy project file and restore dependencies
COPY ["ClaimIq.Web/ClaimIq.Web.csproj", "ClaimIq.Web/"]
RUN dotnet restore "ClaimIq.Web/ClaimIq.Web.csproj"

# Copy source code and build
COPY . .
WORKDIR "/src/ClaimIq.Web"
RUN dotnet publish "ClaimIq.Web.csproj" -c Release -o /app/publish

#### FINAL STAGE ####
FROM nginx:alpine as final

# Copy the published wwwroot to nginx
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html

# Copy nginx configuration
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80