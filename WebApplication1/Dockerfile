FROM mcr.microsoft.com/dotnet/aspnet:8.0 As base
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
FROM mcr.microsoft.com/dotnet/sdk:8.0 As build
WORKDIR /src
COPY ["WebApplication1.csproj","./"]
RUN dotnet restore
COPY . . 
RUN dotnet publish -c Release -o /app/publish
FROM base As final 
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet" ,"WebApplication1.dll"]