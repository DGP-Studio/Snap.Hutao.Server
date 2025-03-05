FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

ENV TZ="Asia/Shanghai"

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Snap.Hutao.Server/Snap.Hutao.Server.csproj", "Snap.Hutao.Server/"]
RUN dotnet restore "Snap.Hutao.Server/Snap.Hutao.Server.csproj"
COPY . .
WORKDIR "/src/Snap.Hutao.Server"
RUN dotnet build "Snap.Hutao.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Snap.Hutao.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Snap.Hutao.Server.dll"]