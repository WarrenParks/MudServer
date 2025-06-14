using System.Text.Json;
using System.Text.Json.Serialization;
using MudServer;
using MudServer.Models;
using MudServer.Server.Services;
using MudServer.Services;

var builder = WebApplication.CreateBuilder(args);

var jsonSerializerOptions = new JsonSerializerOptions
{
  PropertyNameCaseInsensitive = true
};
jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<Client>();
builder.Services.AddSingleton<GameLoop>();
builder.Services.AddSingleton<IConnectionManager, ConnectionManager>();
builder.Services.AddSingleton<IActionManager, ActionManager>();
builder.Services.AddSingleton(jsonSerializerOptions);
builder.Services.AddHostedService<GameLoopService>();

var app = builder.Build();

app.UseWebSockets();
app.Use(async (context, next) =>
{
  if (context.WebSockets.IsWebSocketRequest)
  {
    var client = context.RequestServices.GetRequiredService<Client>();
    await client.StartAsync(CancellationToken.None);
  }
  else
  {
    // If not a WebSocket request, continue to the next middleware
    await next();
  }
});


app.MapGet("/", async context =>
{
  context.Response.ContentType = "text/html";
  await context.Response.SendFileAsync("wwwroot/index.html");
});

app.Run();
