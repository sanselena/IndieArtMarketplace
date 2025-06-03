# Build aşaması
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Proje dosyalarını kopyala ve publish et
COPY . . 
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
