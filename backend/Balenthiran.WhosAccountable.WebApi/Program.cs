using System.Text.Json.Serialization;
using Balenthiran.WhosAccountable.WebApi;
using Balenthiran.WhosAccountable.WebApi.Routes;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddBackendServices(builder.Configuration);
// Enums travel as names ("WaterUtilities", "Emissions"), so the client reads them without a number table.
builder.Services.ConfigureHttpJsonOptions(o => o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var app = builder.Build();

// Fail at startup, not on the first request, if the dataset cannot be loaded and scored.
_ = app.Services.GetRequiredService<ScoreboardStore>();

var pathBase = builder.Configuration["PathBase"];
if (!string.IsNullOrWhiteSpace(pathBase))
    app.UsePathBase(pathBase);

app.MapGroup("/api")
    .MapStatusRoutes()
    .MapLeaderboardRoutes()
    .MapCompanyRoutes()
    .MapMethodologyRoutes();

app.Run();

// Lets the test project host the app in-process (WebApplicationFactory<Program>).
public partial class Program;
