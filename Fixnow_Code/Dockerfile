# Sử dụng image .NET 8 SDK để build app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy file sln và csproj vào trước để chạy restore (tối ưu cache layer của Docker)
COPY ["Fixnow.sln", "./"]
COPY ["Fixnow/Fixnow.csproj", "Fixnow/"]
RUN dotnet restore "Fixnow.sln"

# Copy toàn bộ mã nguồn còn lại
COPY . .

# Build dự án
WORKDIR "/src/Fixnow"
RUN dotnet publish "Fixnow.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Chuyển sang image Runtime nhẹ hơn để chạy ứng dụng (tiết kiệm RAM trên máy chủ)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Expose port (Render web services thường listen ở cổng này nếu chạy Docker)
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080

ENTRYPOINT ["dotnet", "Fixnow.dll"]