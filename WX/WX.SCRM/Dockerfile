FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app

EXPOSE 80
EXPOSE 443

RUN apt-get update
RUN apt-get install libgdiplus --assume-yes

COPY . .
ENTRYPOINT ["dotnet", "WX.SCRM.dll"]