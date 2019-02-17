FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY ["Matterhook.NET/Matterhook.NET.csproj", "Matterhook.NET/"]
RUN dotnet restore "Matterhook.NET/Matterhook.NET.csproj"
COPY . .
WORKDIR "/src/Matterhook.NET"
RUN dotnet build "Matterhook.NET.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Matterhook.NET.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Matterhook.NET.dll"]