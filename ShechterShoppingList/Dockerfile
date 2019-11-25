FROM mcr.microsoft.com/dotnet/core/runtime-deps:2.2-alpine AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-alpine AS build
WORKDIR /src
COPY ["ShechterShoppingList/*.csproj", "ShechterShoppingList/"]
RUN dotnet restore "ShechterShoppingList/ShechterShoppingList.csproj"
WORKDIR "/src/ShechterShoppingList/"
COPY . .
#WORKDIR "/src/ShechterShoppingList/"
RUN dotnet build "ShechterShoppingList.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ShechterShoppingList.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
WORKDIR /app/publish
ENTRYPOINT ["dotnet", "ShechterShoppingList.dll"]