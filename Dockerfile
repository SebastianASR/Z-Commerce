FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["EcommerceApp/EcommerceApp.csproj", "EcommerceApp/"]
RUN dotnet restore "EcommerceApp/EcommerceApp.csproj"

COPY . .
WORKDIR "/src/EcommerceApp"

RUN dotnet publish "EcommerceApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["sh", "-c", "dotnet EcommerceApp.dll --urls http://0.0.0.0:${PORT:-8080}"]