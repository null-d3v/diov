FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

COPY src/Data/Diov.Data.csproj src/Data/
COPY src/Web/Diov.Web.csproj src/Web/
RUN dotnet restore -r linux-musl-x64 src/Web/Diov.Web.csproj

COPY . .
WORKDIR /src/src/Web
RUN dotnet publish -c Release --no-restore -o /app -r linux-musl-x64 --self-contained false

FROM mcr.microsoft.com/dotnet/aspnet:7.0-alpine

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT false
RUN apk add --no-cache icu-libs
ENV LC_ALL en_US.UTF-8
ENV LANG en_US.UTF-8

WORKDIR /app
COPY --from=build /app .

ENTRYPOINT ["./Diov.Web"]