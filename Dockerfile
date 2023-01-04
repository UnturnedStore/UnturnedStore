FROM bitnami/dotnet-sdk:latest as build

WORKDIR /app
COPY . .

RUN dotnet publish src/Website/Server/Website.Server.csproj -o /app/compiled/unturnedstore/


FROM mcr.microsoft.com/dotnet/sdk:6.0

EXPOSE 5000/tcp
WORKDIR /var/www/unturnedstore/app
COPY --from=build /app/compiled/unturnedstore/. /var/www/unturnedstore/app/.
USER root

RUN dotnet dev-certs https

CMD ["dotnet", "Website.Server.dll", "--urls=https://0.0.0.0:5000", "--environment=Production"]
