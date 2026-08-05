# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# Builds the React SPA. Its output is copied into the API's wwwroot in the final stage so
# both are served from one origin — which is what lets the SameSite=Strict refresh cookie
# work without a CORS allowlist.
FROM node:20-alpine AS client
WORKDIR /client
# Copy the lockfile first so this layer is only rebuilt when dependencies actually change,
# not on every source edit.
COPY ["src/Web.Client/package.json", "src/Web.Client/package-lock.json", "./"]
# `npm ci` over `npm install`: installs exactly the locked versions and fails if the
# lockfile is out of sync, so a container build can never silently resolve a new version.
RUN npm ci
COPY ["src/Web.Client/", "./"]
RUN npm run build


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Packages.props", "."]
COPY ["Directory.Build.props", "."]
COPY ["src/Web.Api/Web.Api.csproj", "src/Web.Api/"]
COPY ["src/Infrastructure/Infrastructure.csproj", "src/Infrastructure/"]
COPY ["src/Application/Application.csproj", "src/Application/"]
COPY ["src/Domain/Domain.csproj", "src/Domain/"]
COPY ["src/SharedKernel/SharedKernel.csproj", "src/SharedKernel/"]
RUN dotnet restore "./src/Web.Api/Web.Api.csproj"
COPY . .
WORKDIR "/src/src/Web.Api"
RUN dotnet build "./Web.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Web.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
# The SPA lands in wwwroot, where UseStaticFiles and MapFallbackToFile pick it up.
COPY --from=client /client/dist ./wwwroot
ENTRYPOINT ["dotnet", "Web.Api.dll"]
