FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Robot.ED.FacebookConnector.Service.API/Robot.ED.FacebookConnector.Service.API.csproj", "Robot.ED.FacebookConnector.Service.API/"]
COPY ["Robot.ED.FacebookConnector.Common/Robot.ED.FacebookConnector.Common.csproj", "Robot.ED.FacebookConnector.Common/"]
RUN dotnet restore "Robot.ED.FacebookConnector.Service.API/Robot.ED.FacebookConnector.Service.API.csproj"
COPY . .
WORKDIR "/src/Robot.ED.FacebookConnector.Service.API"
RUN dotnet build "Robot.ED.FacebookConnector.Service.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Robot.ED.FacebookConnector.Service.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Robot.ED.FacebookConnector.Service.API.dll"]
