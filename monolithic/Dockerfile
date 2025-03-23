FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

COPY . ./
RUN dotnet restore "SocialNetworkApi/SocialNetworkApi.csproj"

RUN dotnet publish "SocialNetworkApi/SocialNetworkApi.csproj" -c Release -o /out --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Ensure "Public" folder exists inside the container
RUN mkdir -p /app/Public

COPY --from=build /out .

EXPOSE 5046
ENV ASPNETCORE_URLS=http://+:5046

ENTRYPOINT ["dotnet", "SocialNetworkApi.dll"]
