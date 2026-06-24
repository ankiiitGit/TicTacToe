using System.Text.Json.Serialization;
using TicTacToe.Api.Storage;

var builder = WebApplication.CreateBuilder(args);

// CORS policy name (referenced again below when enabling the middleware).
const string FrontendCors = "frontend";

// --- Register services into the dependency-injection container -----------------

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        // Serialize enums as their string names ("X", "InProgress", "TwoPlayer")
        // so the JSON matches the Angular string-literal types. (camelCase for
        // property names is already the default for Web APIs.)
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// The in-memory store is a SINGLETON: one shared instance holds all games and the
// scoreboard for the whole app lifetime. (Scoped/Transient would lose state.)
builder.Services.AddSingleton<IGameStore, InMemoryGameStore>();

// Allow the Angular dev server (http://localhost:4200) to call this API.
// Browsers block cross-origin calls unless the server opts in via CORS headers.
builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCors, policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.AddOpenApi(); // exposes /openapi/v1.json in Development

var app = builder.Build();

// --- Configure the HTTP request pipeline (order matters) ----------------------

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Enable the CORS policy. Must come before MapControllers so the headers are added.
app.UseCors(FrontendCors);

app.MapControllers();

app.Run();

// Exposed so the integration/unit test project can reference the entry-point
// assembly via WebApplicationFactory<Program> if needed.
public partial class Program { }
