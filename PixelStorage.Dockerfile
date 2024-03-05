FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

COPY ./PixelStorage/PixelStorage.csproj ./
COPY ./Pixel.Shared/Pixel.Shared.csproj ../Pixel.Shared/
RUN dotnet restore ./PixelStorage.csproj

COPY ./PixelStorage ./
COPY ./Pixel.Shared/ ../Pixel.Shared/
RUN dotnet publish ./PixelStorage.csproj -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "PixelStorage.dll"]
