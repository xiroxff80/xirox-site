# ---------- Build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["XIROX/XIROX.csproj", "XIROX/"]
RUN dotnet restore "XIROX/XIROX.csproj"
COPY . .
RUN dotnet publish "XIROX/XIROX.csproj" -c Release -o /app/out

# ---------- Run stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/out .
EXPOSE 8080
# نکته: اینجا bash متغیر PORT را جایگزین می‌کند (اگر نبود، 8080 برای لوکال)
CMD ["bash","-lc","dotnet XIROX.dll --urls http://0.0.0.0:${PORT:-8080}"]