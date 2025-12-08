# Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY ["SchoolManagementSystem.Web/SchoolManagementSystem.Web/SchoolManagementSystem.Web.csproj", "SchoolManagementSystem.Web/SchoolManagementSystem.Web/"]
COPY ["SchoolManagementSystem.Web/SchoolManagementSystem.Web.Client/SchoolManagementSystem.Web.Client.csproj", "SchoolManagementSystem.Web/SchoolManagementSystem.Web.Client/"]
RUN dotnet restore "SchoolManagementSystem.Web/SchoolManagementSystem.Web/SchoolManagementSystem.Web.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/SchoolManagementSystem.Web/SchoolManagementSystem.Web"
RUN dotnet build "SchoolManagementSystem.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SchoolManagementSystem.Web.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Fix for SQLite permissions in Docker (ensure 'app' user can write to the directory)
USER root
RUN chown -R $APP_UID /app
USER $APP_UID

# Explicitly listen on port 8080 (default for .NET 8/9 images, but good for clarity)
ENV ASPNETCORE_HTTP_PORTS=8080

ENTRYPOINT ["dotnet", "SchoolManagementSystem.Web.dll"]
