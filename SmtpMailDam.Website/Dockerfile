#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["SmtpMailtrap.Website/SmtpMailtrap.Website.csproj", "SmtpMailtrap.Website/"]
RUN dotnet restore "SmtpMailtrap.Website/SmtpMailtrap.Website.csproj"
COPY . .
WORKDIR "/src/SmtpMailtrap.Website"
RUN dotnet build "SmtpMailtrap.Website.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SmtpMailtrap.Website.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SmtpMailtrap.Website.dll"]