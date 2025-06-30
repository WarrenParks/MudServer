using System.Text.Json;
using System.Text.Json.Serialization;

using MudServer;
using MudServer.Server.Commands;
using MudServer.Server.Models;
using MudServer.Server.Services;

var builder = WebApplication.CreateBuilder(args);

var jsonSerializerOptions = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true
};
jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<ChatCommand>();
builder.Services.AddTransient<IChatManager, ChatManager>();
builder.Services.AddTransient<MoveCommand>();
builder.Services.AddTransient<INotificationManager, NotificationManager>();
builder.Services.AddTransient<PingCommand>();
builder.Services.AddTransient<RegisterCommand>();
builder.Services.AddTransient<StartGameCommand>();
builder.Services.AddTransient<StopServerCommand>();
builder.Services.AddTransient<WebSocketConnectionHandler>();
builder.Services.AddTransient<IWebSocketMessenger, WebSocketMessenger>();
builder.Services.AddSingleton<GameLoop>();
builder.Services.AddSingleton<IActionManager, ActionManager>();
builder.Services.AddSingleton<IConnectionManager, ConnectionManager>();
builder.Services.AddSingleton<IGameCommandFactory, GameCommandFactory>();
builder.Services.AddSingleton<IGameStateManager, GameStateManager>();
builder.Services.AddSingleton(jsonSerializerOptions);
builder.Services.AddHostedService<GameLoopService>();

var app = builder.Build();

// Configure graceful shutdown
app.Lifetime.ApplicationStopping.Register(() =>
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Application shutdown initiated");
});

app.UseWebSockets();
app.Use(async (context, next) =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var webSocketConnectionHandler = context.RequestServices.GetRequiredService<WebSocketConnectionHandler>();
        await webSocketConnectionHandler.StartAsync(CancellationToken.None);
    }
    else
    {
        // If not a WebSocket request, continue to the next middleware
        await next();
    }
});

app.UseStaticFiles();
app.MapGet("/terminal", async context =>
{
    context.Response.ContentType = "text/html";
    await context.Response.SendFileAsync("wwwroot/index.html");
});

app.MapGet("/", async context =>
{
    context.Response.ContentType = "text/html";
    await context.Response.SendFileAsync("wwwroot/client.html");
});

app.Run();