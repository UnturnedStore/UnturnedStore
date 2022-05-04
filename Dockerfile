FROM bitnami/dotnet-sdk:latest
WORKDIR /app
COPY . .

RUN dotnet publish src/Website/Server/Website.Server.csproj -o /var/www/unturnedstore/app

#FROM bitnami/dotnet-sdk:latest
EXPOSE 5000/tcp
WORKDIR /var/www/unturnedstore/app

#COPY --from=publish /app/unturnedstore/app/. .
#USER root

CMD ["dotnet", "Website.Server.dll", "--urls=https://0.0.0.0:5000", "--environment=Production"]
