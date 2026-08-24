FROM mcr.microsoft.com/dotnet/sdk:9.0
WORKDIR /src

COPY tests/Versta.IntegrationTests/Versta.IntegrationTests.csproj tests/Versta.IntegrationTests/
RUN dotnet restore tests/Versta.IntegrationTests/Versta.IntegrationTests.csproj

COPY tests/Versta.IntegrationTests tests/Versta.IntegrationTests
RUN mkdir -p /test-results

ENTRYPOINT ["dotnet", "test", "tests/Versta.IntegrationTests/Versta.IntegrationTests.csproj", "-c", "Release", "--no-restore", "--logger", "console;verbosity=normal", "--logger", "junit;LogFilePath=/test-results/integration-tests.xml;MethodFormat=Class;FailureBodyFormat=Verbose"]
