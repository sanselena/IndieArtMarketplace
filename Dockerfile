# Build aşaması
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy .csproj and .sln files and restore dependencies
COPY IndieArtMarketplace.csproj ./
COPY IndieArtMarketplace.sln ./
RUN dotnet restore

# Copy the rest of the application files
COPY . .

# Copy wwwroot separately to ensure it's included
COPY wwwroot/ ./wwwroot/

# Publish the application
RUN dotnet publish -c Release -o out

# Runtime aşaması
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# Port ayarla
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Başlat
ENTRYPOINT ["dotnet", "IndieArtMarketplace.dll"]
