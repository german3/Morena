# Use the official .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["Morena.csproj", "./"]
RUN dotnet restore "Morena.csproj"

# Copy everything else and build the app
COPY . .
RUN dotnet build "Morena.csproj" -c Release -o /app/build

# Publish the app
FROM build AS publish
RUN dotnet publish "Morena.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Generate final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Expose port (ASP.NET Core 8.0+ binds to port 8080 by default)
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Morena.dll"]
