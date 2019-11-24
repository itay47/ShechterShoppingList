FROM mcr.microsoft.com/dotnet/core/aspnet:2.2 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build
WORKDIR /src
COPY ["ShechterShoppingList.csproj", "ShechterShoppingList/"]
RUN dotnet restore "ShechterShoppingList/ShechterShoppingList.csproj"
#WORKDIR "/src/ShechterShoppingList/"
COPY . /src/ShechterShoppingList/.
WORKDIR /src/ShechterShoppingList/
RUN dotnet build "ShechterShoppingList.csproj" -c Debug -o /app/build

FROM build AS publish
RUN dotnet publish "ShechterShoppingList.csproj" -c Debug -o /app/publish
WORKDIR /app/publish/
RUN dotnet dev-certs https --clean --verbose
RUN dotnet dev-certs https --verbose

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ShechterShoppingList.dll"]