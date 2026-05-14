#!/bin/sh

# Start tailscaled in the background
# We use --tun=userspace-networking if we don't have /dev/net/tun, 
# but usually sidecars use the host's TUN device via --device /dev/net/tun
tailscaled --state=/var/lib/tailscale/tailscaled.state --socket=/run/tailscale/tailscaled.sock &

# Wait for tailscaled to start
echo "Waiting for tailscaled to start..."
sleep 5

# Authenticate and bring up Tailscale
if [ -n "$TAILSCALE_AUTHKEY" ]; then
    echo "Authenticating with Tailscale..."
    tailscale up --authkey="$TAILSCALE_AUTHKEY" --hostname=scrcpy-bridge --accept-routes
else
    echo "Warning: TAILSCALE_AUTHKEY not set. Tailscale will not be initialized automatically."
fi

# Start ADB server listening on all interfaces
echo "Starting ADB server..."
adb -a nodaemon server start &
sleep 2

echo "Starting Scrcpy Bridge .NET application..."
# Run the .NET application
exec dotnet ScrcpyBridge.dll
