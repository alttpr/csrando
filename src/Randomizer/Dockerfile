FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY ./Randomizer.csproj ./
RUN dotnet restore ./Randomizer.csproj

# Copy everything else and publish
COPY . .
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# ASP.NET will listen on 8080 inside the container
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .

EXPOSE 8080

# Run the console app in API mode
ENTRYPOINT ["dotnet", "Randomizer.dll", "api"]

