FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CopticChanter.Blazor.App/CopticChanter.Blazor.App.csproj", "CopticChanter.Blazor.App/"]
RUN dotnet restore "CopticChanter.Blazor.App/CopticChanter.Blazor.App.csproj"
COPY . .
WORKDIR "/src/CopticChanter.Blazor.App"
RUN dotnet build "CopticChanter.Blazor.App.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "CopticChanter.Blazor.App.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CopticChanter.Blazor.App.dll"]
