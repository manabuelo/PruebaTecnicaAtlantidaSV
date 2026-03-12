FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

COPY ["MundialCorporativo.Api/MundialCorporativo.Api.csproj", "MundialCorporativo.Api/"]
COPY ["MundialCorporativo.Application/MundialCorporativo.Application.csproj", "MundialCorporativo.Application/"]
COPY ["MundialCorporativo.Domain/MundialCorporativo.Domain.csproj", "MundialCorporativo.Domain/"]
COPY ["MundialCorporativo.Infrastructure/MundialCorporativo.Infrastructure.csproj", "MundialCorporativo.Infrastructure/"]
COPY ["NuGet.config", "./"]
RUN dotnet restore "MundialCorporativo.Api/MundialCorporativo.Api.csproj"

COPY . .
WORKDIR "/src/MundialCorporativo.Api"
RUN dotnet publish "MundialCorporativo.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "MundialCorporativo.Api.dll"]
