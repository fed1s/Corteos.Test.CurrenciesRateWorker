FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copy everything else and build
COPY . ./
RUN --mount=type=cache,target=/root/.nuget/packages dotnet publish -c Release -o .build
RUN rm /app/.build/*.pdb
RUN rm -rf /app/.build/runtimes/win
RUN rm -rf /app/.build/runtimes/osx

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/.build .
ENTRYPOINT ["/app/Corteos.Test.CurrenciesRateWorker"]
