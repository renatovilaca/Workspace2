FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 7000
EXPOSE 7001

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Robot.ED.FacebookConnector.Dashboard/Robot.ED.FacebookConnector.Dashboard.csproj", "Robot.ED.FacebookConnector.Dashboard/"]
COPY ["Robot.ED.FacebookConnector.Common/Robot.ED.FacebookConnector.Common.csproj", "Robot.ED.FacebookConnector.Common/"]
RUN dotnet restore "Robot.ED.FacebookConnector.Dashboard/Robot.ED.FacebookConnector.Dashboard.csproj"
COPY . .
WORKDIR "/src/Robot.ED.FacebookConnector.Dashboard"
RUN dotnet build "Robot.ED.FacebookConnector.Dashboard.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Robot.ED.FacebookConnector.Dashboard.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Robot.ED.FacebookConnector.Dashboard.dll"]
