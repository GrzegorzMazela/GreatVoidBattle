using GreatVoidBattle.Application.Hubs;
using GreatVoidBattle.Application.Factories;
using GreatVoidBattle.Application.Services;
using GreatVoidBattle.Infrastructure;
using GreatVoidBattle.Infrastructure.Repository;
using GreatVoidBattle.Application.Repositories;
using GreatVoidBattle.Api.Middleware;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson;

var builder = WebApplication.CreateBuilder(args);

// Register the GuidSerializer globally
BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

var mongoConn = builder.Configuration.GetValue<string>("Mongo:ConnectionString") ?? "mongodb://localhost:27017";
var mongoDbName = builder.Configuration.GetValue<string>("Mongo:Database") ?? "great-void-battle";
builder.Services.AddSingleton(sp => MongoDbFactory.Create(mongoConn, mongoDbName));
builder.Services.AddScoped<IBattleStateRepository, BattleStateRepository>();
builder.Services.AddScoped<IBattleEventRepository, BattleEventRepository>();

// Discord OAuth
builder.Services.AddHttpClient<IDiscordService, DiscordService>();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
    {
        Title = "Great Void Battle API",
        Version = "v1",
        Description = "API for Great Void Battle game"
    });
});
builder.Services.AddMemoryCache();
builder.Services.AddTransient<BattleManagerFactory>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173",
                "http://localhost:5174",
                "http://localhost:5175",
                "http://localhost:5112",
                "https://localhost:7295",
                "http://109.173.167.125",
                "http://109.173.167.125:32000",
                "http://109.173.167.125:32001",
                "https://api.wielkapustka.pl",
                "https://wielkapustka.pl")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials(); // Wymagane dla SignalR
    });
});

builder.Services.AddControllers();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseHttpsRedirection();
app.UseSession();
app.UseMiddleware<DiscordAuthMiddleware>();
app.UseFractionAuthorization();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

app.MapControllers();
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapHub<BattleHub>("/hubs/battle");
app.Run();