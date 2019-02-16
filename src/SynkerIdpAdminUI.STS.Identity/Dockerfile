FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
HEALTHCHECK --interval=3m --timeout=1m CMD curl -f http://localhost/liveness || exit 1

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY ["src/SynkerIdpAdminUI.STS.Identity/SynkerIdpAdminUI.STS.Identity.csproj", "src/SynkerIdpAdminUI.STS.Identity/"]
RUN dotnet restore "src/SynkerIdpAdminUI.STS.Identity/SynkerIdpAdminUI.STS.Identity.csproj"
COPY . .
WORKDIR "/src/src/SynkerIdpAdminUI.STS.Identity"
RUN dotnet build "SynkerIdpAdminUI.STS.Identity.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "SynkerIdpAdminUI.STS.Identity.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "SynkerIdpAdminUI.STS.Identity.dll"]