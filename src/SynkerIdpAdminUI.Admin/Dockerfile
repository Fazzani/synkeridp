FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80
HEALTHCHECK --interval=3m --timeout=1m CMD curl -f http://localhost/liveness || exit 1

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY ["src/SynkerIdpAdminUI.Admin/SynkerIdpAdminUI.Admin.csproj", "src/SynkerIdpAdminUI.Admin/"]
RUN dotnet restore "src/SynkerIdpAdminUI.Admin/SynkerIdpAdminUI.Admin.csproj"
COPY . .
WORKDIR "/src/src/SynkerIdpAdminUI.Admin"
RUN dotnet build "SynkerIdpAdminUI.Admin.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "SynkerIdpAdminUI.Admin.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "SynkerIdpAdminUI.Admin.dll"]