FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["RemotixSignalingServer.csproj", "."]
RUN dotnet restore "RemotixSignalingServer.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "RemotixSignalingServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RemotixSignalingServer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Use the PORT environment variable provided by Heroku
ENV ASPNETCORE_URLS=http://+:$PORT
ENTRYPOINT ["dotnet", "RemotixSignalingServer.dll"]