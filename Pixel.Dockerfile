FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

COPY ./Pixel/Pixel.csproj ./
COPY ./Pixel.Shared/Pixel.Shared.csproj ../Pixel.Shared/
RUN dotnet restore ./Pixel.csproj

COPY ./Pixel ./
COPY ./Pixel.Shared/ ../Pixel.Shared/
RUN dotnet publish ./Pixel.csproj -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "Pixel.dll"]
