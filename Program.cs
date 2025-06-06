using MudServer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<Client>();

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
