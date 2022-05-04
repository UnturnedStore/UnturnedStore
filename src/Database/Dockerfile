FROM bitnami/dotnet-sdk:5 as COMPILE

USER root

WORKDIR /compiling/compile
COPY . .
RUN dotnet build Database.Build/Database.Build.csproj --configuration Release


FROM markhobson/sqlpackage:latest
ARG CONNECTION_STRING
ENV CONNECTION_STRING=$CONNECTION_STRING

USER root

WORKDIR /bruh
COPY --from=COMPILE /compiling/compile/ /bruh/.

ENTRYPOINT sqlpackage /a:Publish /tcs:"$CONNECTION_STRING" /sf:Database.Build/bin/Release/netstandard2.0/Database.Build.dacpac
