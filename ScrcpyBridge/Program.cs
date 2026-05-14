using AdvancedSharpAdbClient;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSignalR();

// Register ADB Client services
builder.Services.AddSingleton<IAdbClient>(new AdbClient());

var app = builder.Build();

app.MapHub<BridgeHub>("/bridgeHub");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapPost("/adb/connect", (string address, IAdbClient adbClient) =>
{
    try
    {
        // Simple IP:Port parsing
        string host = address;
        int port = 5555;
        if (address.Contains(":"))
        {
            var parts = address.Split(':');
            host = parts[0];
            port = int.Parse(parts[1]);
        }

        DnsEndPoint endPoint = new DnsEndPoint(host, port);
        adbClient.Connect(endPoint);
        return Results.Ok(new { Message = $"Connected to {address}" });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { Error = ex.Message });
    }
})
.WithName("AdbConnect");

app.MapGet("/adb/devices", (IAdbClient adbClient) =>
{
    try
    {
        var devices = adbClient.GetDevices();
        return Results.Ok(devices.Select(d => new { d.Serial, d.State, d.Model, d.Name }));
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { Error = ex.Message });
    }
})
.WithName("GetAdbDevices");

app.MapGet("/bridge/status", () =>
{
    return Results.Ok(new
    {
        Status = "Online",
        Message = "Scrcpy Sidecar Bridge is running",
        TailscaleIP = Environment.GetEnvironmentVariable("TAILSCALE_IP") ?? "Unknown",
        Timestamp = DateTime.UtcNow
    });
})
.WithName("GetBridgeStatus");

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

public class BridgeHub : Microsoft.AspNetCore.SignalR.Hub
{
    public async Task SendFrame(byte[] frame)
    {
        await Clients.All.SendAsync("ReceiveFrame", frame);
    }
}
