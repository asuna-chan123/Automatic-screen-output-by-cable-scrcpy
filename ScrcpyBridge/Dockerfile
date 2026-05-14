# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["ScrcpyBridge.csproj", "./"]
RUN dotnet restore
COPY . .
RUN dotnet publish "ScrcpyBridge.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Install Tailscale and ADB dependencies
RUN apt-get update && apt-get install -y \
    curl \
    gnupg \
    iptables \
    ca-certificates \
    adb \
    && rm -rf /var/lib/apt/lists/*

# Install Tailscale
RUN curl -fsSL https://tailscale.com/install.sh | sh

# Copy the published .NET application
COPY --from=build /app/publish .

# Copy and setup entrypoint script
COPY entrypoint.sh .
RUN chmod +x entrypoint.sh

# State directory for Tailscale
RUN mkdir -p /var/lib/tailscale /run/tailscale

# Environment variables
# Users should override TAILSCALE_AUTHKEY at runtime
ENV TAILSCALE_AUTHKEY=""
ENV ASPNETCORE_URLS=http://+:5000

# Metadata
LABEL maintainer="Antigravity"
LABEL description="Scrcpy Bridge with Tailscale Sidecar"

# The app listens on localhost/5000, Tailscale will route traffic to it
EXPOSE 5000

ENTRYPOINT ["./entrypoint.sh"]
