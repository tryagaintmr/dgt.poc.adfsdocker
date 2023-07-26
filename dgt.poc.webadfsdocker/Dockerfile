FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Install dependencies required to work with certificates
RUN apt-get update && apt-get install -y ca-certificates

# Add certificate to the image
COPY dc.dev.local.crt /usr/local/share/ca-certificates/dc.dev.local.crt
RUN update-ca-certificates

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["dgt.poc.webadfsdocker.csproj", "./"]
RUN dotnet restore "dgt.poc.webadfsdocker.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "dgt.poc.webadfsdocker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "dgt.poc.webadfsdocker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
# COPY --from=build /etc/hosts /etc/hosts
# Import root CA certificate
COPY ["dc.dev.local.crt", "/usr/local/share/ca-certificates/"]
RUN chmod 644 /usr/local/share/ca-certificates/dc.dev.local.crt && update-ca-certificates

ENTRYPOINT ["dotnet", "dgt.poc.webadfsdocker.dll"]
