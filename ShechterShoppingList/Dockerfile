FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS base
RUN apt-get -y update \
    && apt-get -y install python-pip \
    && pip --version \ 
    && pip install awscli \
    && apt-get clean \
    && aws --version \
    && which aws \
    && which dotnet
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY ["ShechterShoppingList.csproj", "ShechterShoppingList/"]
RUN dotnet restore "ShechterShoppingList/ShechterShoppingList.csproj"
WORKDIR "/src/ShechterShoppingList/"
COPY . .
#WORKDIR "/src/ShechterShoppingList/"
RUN dotnet build "ShechterShoppingList.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ShechterShoppingList.csproj" -c Release -o /app/publish

FROM base AS final
COPY itay.sh /app/itay.sh
RUN ["chmod", "+x", "/app/itay.sh"]
WORKDIR /app
COPY --from=publish /app/publish .
#ENTRYPOINT ["dotnet", "ShechterShoppingList.dll"]
CMD ["/bin/bash", "-c", "/app/itay.sh"]
