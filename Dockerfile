# BUILD
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ./AuthApi ./AuthApi
WORKDIR /src/AuthApi

RUN dotnet restore
RUN dotnet publish -c Release -o /app/out

# RUNTIME
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "AuthApi.dll"]