using ClaimIq.Api.Services;
using Amazon.Lambda.AspNetCoreServer;
using ClaimIq.Api.Configuration;
using ClaimIq.Api.Services.FeatureFlags;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddHealthChecks();

// JWT STUFF!!!
var jwtKey = builder.Configuration["JWT_SECRET"] ?? "your-super-secret-key-for-demo-purposes-only-change-in-production";
var key = Encoding.ASCII.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // For local development
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,  // For demo
        ValidateAudience = false, // For demo
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Swagger up
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 🔥 REDIS FOR FEATURE FLAGS ONLY
var redisEndpoint = builder.Configuration["REDIS_ENDPOINT"];
var redisPort = builder.Configuration["REDIS_PORT"] ?? "6379";

if (!string.IsNullOrEmpty(redisEndpoint))
{
    // 🔥 PRODUCTION: USE REDIS FOR FEATURE FLAGS
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = $"{redisEndpoint}:{redisPort}";
        options.InstanceName = "ClaimIq_FeatureFlags_";

        options.ConfigurationOptions = new StackExchange.Redis.ConfigurationOptions
        {
            EndPoints = { $"{redisEndpoint}:{redisPort}" },
            ConnectTimeout = 5000,                              // time to connect
            SyncTimeout = 10000,                                 // time for sync operations
            AsyncTimeout = 10000,                                // time for async operations
            ConnectRetry = 3,                                   // retry on connect
            AbortOnConnectFail = false,                         // don't abort, try to reconnect
            KeepAlive = 180,                                   // send a message every 3 minutes
            DefaultDatabase = 0,                                 // use default DB
            AllowAdmin = false,                                 // don't allow admin commands

            // lambda specific
            ReconnectRetryPolicy = new StackExchange.Redis.ExponentialRetry(1000), // retry with exponential backoff
            SocketManager = null
        };

    });
    builder.Services.AddSingleton<IFeatureFlagRepository, RedisFeatureFlagRepository>();
}
else
{
    // 🔥 LOCAL: USE IN-MEMORY FOR FEATURE FLAGS
    builder.Services.AddDistributedMemoryCache();
    builder.Services.AddSingleton<IFeatureFlagRepository, InMemoryFeatureFlagRepository>();
}

// 🔥 SIMPLE SERVICES - NO CACHING FOR CLAIMS
builder.Services.AddSingleton<IClaimsDataService, ClaimsDataService>();  // Simple, fast, no cache needed
builder.Services.AddSingleton<IFeatureFlagService, CachedFeatureFlagService>();  // This one uses Redis
builder.Services.AddSingleton<IJwtService, JwtService>();

///////////////////
//// CORS HELL ////
///////////////////
var corsSettings = builder.Configuration.GetSection("CorsSettings").Get<CorsSettings>() ?? new CorsSettings();

builder.Services.AddCors(options =>
{
    options.AddPolicy("BlazorPolicy", policy =>
    {
        policy.WithOrigins(corsSettings.ResolvedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();

        if (corsSettings.AllowCredentials)
        {
            policy.AllowCredentials();
        }
    });
});

// AWS Stuff
builder.Services.AddAWSLambdaHosting(LambdaEventSource.RestApi);

/////////////////////////
//// BUILD THE APP ! ////
/////////////////////////
var app = builder.Build();

app.MapGet("/", () => new { message = "ClaimIQ API is alive!", timestamp = DateTime.UtcNow });
app.MapGet("/api", () => new { message = "API root", endpoints = new[] { "/api/claims", "/api/auth/login", "/api/FeatureFlag" } });  // 🔥 ADD ALL ENDPOINTS

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHealthChecks("/health");

app.UseCors("BlazorPolicy");

// 🔥 JWT MIDDLEWARE
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();