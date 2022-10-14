FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

ENV ConnectionStrings:Snap_DB="server=172.17.0.1;port=3306;user=xxxx;password=xxx; database=xxxxx;"

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
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