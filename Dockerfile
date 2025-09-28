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
# Render مقدار PORT را می‌فرستد؛ اپ باید روی همان گوش کند:
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}
# برای اجرای لوکال اگر PORT نبود، 8080
ENV PORT=8080
EXPOSE 8080
ENTRYPOINT ["dotnet","XIROX.dll"]