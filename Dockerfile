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
ENTRYPOINT ["dotnet", "SchoolManagementSystem.Web.dll"]
