FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN rm -f global.json && dotnet restore RxFlow.Api/RxFlow.Api.csproj --configfile NuGet.Config -p:NuGetAudit=false
RUN dotnet publish RxFlow.Api/RxFlow.Api.csproj -c Release -o /app/out --no-restore -p:NuGetAudit=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "RxFlow.Api.dll"]