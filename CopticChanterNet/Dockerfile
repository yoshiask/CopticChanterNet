#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["CopticChanterNet/CopticChanterNet.csproj", "CopticChanterNet/"]
RUN dotnet restore "CopticChanterNet/CopticChanterNet.csproj"
COPY . .
WORKDIR "/src/CopticChanterNet"
RUN dotnet build "CopticChanterNet.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CopticChanterNet.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CopticChanterNet.dll"]