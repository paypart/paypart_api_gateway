FROM microsoft/dotnet:2.0.0-sdk
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY *.csproj .
RUN dotnet restore

# Copy everything else and build
COPY . .
RUN dotnet publish -c Release -o out

EXPOSE 9000/tcp

ENV ASPNETCORE_URLS http://0.0.0.0:9000

ENTRYPOINT ["dotnet", "out/paypart_api_gateway.dll"]