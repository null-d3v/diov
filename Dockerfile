FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ./Directory.Build.props ./
COPY ./Directory.Packages.props ./
COPY src/Data/Diov.Data.csproj src/Data/
COPY src/Web/Diov.Web.csproj src/Web/
RUN dotnet restore -r linux-musl-x64 src/Web/Diov.Web.csproj

COPY . .
WORKDIR /src/src/Web
RUN dotnet publish -c Release --no-restore -o /app -r linux-musl-x64 --self-contained false

FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine

ENV \
    DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE=false \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    LC_ALL=en_US.UTF-8 \
    LANG=en_US.UTF-8
RUN \
    apk add --no-cache \
    icu-data-full \
    icu-libs

WORKDIR /app
COPY --from=build /app .

ENTRYPOINT ["./Diov.Web"]