#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["ScrumPokerClub.App/ScrumPokerClub.App.csproj", "ScrumPokerClub.App/"]
RUN dotnet restore "ScrumPokerClub.App/ScrumPokerClub.App.csproj"
COPY . .
WORKDIR "/src/ScrumPokerClub.App"
RUN dotnet build "ScrumPokerClub.App.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ScrumPokerClub.App.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ScrumPokerClub.App.dll"]