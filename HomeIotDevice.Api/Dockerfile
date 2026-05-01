FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["HomeIotDevice.Domain/HomeIotDevice.Domain.csproj", "HomeIotDevice.Domain/"]
COPY ["HomeIotDevice.Application/HomeIotDevice.Application.csproj", "HomeIotDevice.Application/"]
COPY ["HomeIotDevice.Infrastructure/HomeIotDevice.Infrastructure.csproj", "HomeIotDevice.Infrastructure/"]
COPY ["HomeIotDevice.Api/HomeIotDevice.Api.csproj", "HomeIotDevice.Api/"]
RUN dotnet restore "HomeIotDevice.Api/HomeIotDevice.Api.csproj"
COPY . .
WORKDIR "/src/HomeIotDevice.Api"
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:10000
ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ["dotnet", "HomeIotDevice.Api.dll"]
