FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
HEALTHCHECK --interval=3m --timeout=1m CMD curl -f http://localhost/liveness || exit 1

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY ["src/StsServerIdentity/StsServerIdentity.csproj", "src/StsServerIdentity/"]
RUN dotnet restore "src/StsServerIdentity/StsServerIdentity.csproj"
COPY . .
WORKDIR "/src/src/StsServerIdentity"
RUN dotnet build "StsServerIdentity.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "StsServerIdentity.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "StsServerIdentity.dll"]