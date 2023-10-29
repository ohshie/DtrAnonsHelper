FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["DtrAnonsHelper/DtrAnonsHelper.csproj", "DtrAnonsHelper/"]
RUN dotnet restore "DtrAnonsHelper/DtrAnonsHelper.csproj"
COPY . .
WORKDIR "/src/DtrAnonsHelper"
RUN dotnet build "DtrAnonsHelper.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DtrAnonsHelper.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DtrAnonsHelper.dll"]
