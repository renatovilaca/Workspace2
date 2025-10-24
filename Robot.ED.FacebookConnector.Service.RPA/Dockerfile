FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Install Chrome and ChromeDriver for Selenium
RUN apt-get update && apt-get install -y \
    wget \
    gnupg \
    && wget -q -O - https://dl-ssl.google.com/linux/linux_signing_key.pub | apt-key add - \
    && echo "deb [arch=amd64] http://dl.google.com/linux/chrome/deb/ stable main" >> /etc/apt/sources.list.d/google.list \
    && apt-get update \
    && apt-get install -y google-chrome-stable \
    && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Robot.ED.FacebookConnector.Service.RPA/Robot.ED.FacebookConnector.Service.RPA.csproj", "Robot.ED.FacebookConnector.Service.RPA/"]
COPY ["Robot.ED.FacebookConnector.Common/Robot.ED.FacebookConnector.Common.csproj", "Robot.ED.FacebookConnector.Common/"]
RUN dotnet restore "Robot.ED.FacebookConnector.Service.RPA/Robot.ED.FacebookConnector.Service.RPA.csproj"
COPY . .
WORKDIR "/src/Robot.ED.FacebookConnector.Service.RPA"
RUN dotnet build "Robot.ED.FacebookConnector.Service.RPA.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Robot.ED.FacebookConnector.Service.RPA.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN mkdir -p screenshots
ENTRYPOINT ["dotnet", "Robot.ED.FacebookConnector.Service.RPA.dll"]
