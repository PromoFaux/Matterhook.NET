# Stage 1
FROM microsoft/aspnetcore-build:1.0-2.0 AS builder
WORKDIR /source

COPY . .
RUN dotnet restore Matterhook.NET.sln
RUN dotnet publish Matterhook.NET.sln -c Release -o /publish

# Stage 2
FROM microsoft/aspnetcore:2.0
WORKDIR /app
EXPOSE 80
COPY --from=builder /publish .
ENTRYPOINT ["dotnet", "Matterhook.NET.dll"]
