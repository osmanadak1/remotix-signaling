using RemotixSignalingServer;

var builder = WebApplication.CreateBuilder(args);

// Configure port for Heroku
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
builder.WebHost.UseUrls($"http://*:{port}");

// Add SignalR services with increased message size for screen data
builder.Services.AddSignalR(options =>
{
    options.MaximumReceiveMessageSize = 10 * 1024 * 1024; // 10MB for screen data
});

// Add CORS to allow connections from any origin
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// Use CORS
app.UseCors();

// Map the SignalR hub
app.MapHub<ConnectionHub>("/connectionhub");

// Health check endpoint
app.MapGet("/", () => "Remotix SignalR Server is running!");

// Status endpoint
app.MapGet("/status", () => new { Status = "OK", Service = "Remotix SignalR Server" });

app.Run();