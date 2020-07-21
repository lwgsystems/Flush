FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build-env
WORKDIR /app

# Copy the project and build
COPY . ./
RUN dotnet publish -c Release -o out Flush.sln

# Install a copy of the dotnet cert tool as a global tool
RUN dotnet tool install --add-source ./dotnet-certificate-tool/src/nupkg --tool-path /app/out dotnet-certificate-tool

# Pull asp.net core runtime image
FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /app

# Copy in build output
COPY --from=build-env /app/out .

# Copy in certificates for use by flush/cert-tool
COPY *.pfx ./
COPY docker-entrypoint.sh .

# Start the bootstrapper on run
ENTRYPOINT ["./docker-entrypoint.sh"]
