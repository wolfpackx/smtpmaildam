#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:8.0-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0-buster AS build
WORKDIR /src
COPY ["SmtpMailtrap.Worker/SmtpMailtrap.Worker.csproj", "SmtpMailtrap.Worker/"]
RUN dotnet restore "SmtpMailtrap.Worker/SmtpMailtrap.Worker.csproj"
COPY . .
WORKDIR "/src/SmtpMailtrap.Worker"
RUN dotnet build "SmtpMailtrap.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SmtpMailtrap.Worker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 587
ENTRYPOINT ["dotnet", "SmtpMailtrap.Worker.dll"]