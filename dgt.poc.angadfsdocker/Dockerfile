#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.
FROM node:14 as angular-build
WORKDIR /app
COPY /ClientApp/package.json /ClientApp/package-lock.json ./
RUN npm install
COPY /ClientApp/ .
RUN npm run build -- --prod

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
COPY ["dgt.poc.angadfsdocker.csproj", "./"]
RUN dotnet restore "dgt.poc.angadfsdocker.csproj"
COPY . .
RUN dotnet build "dgt.poc.angadfsdocker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "dgt.poc.angadfsdocker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=angular-build /app/dist /app/wwwroot
ENTRYPOINT ["dotnet", "dgt.poc.angadfsdocker.dll"]
