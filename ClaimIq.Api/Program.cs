using ClaimIq.Api.Services;
using Amazon.Lambda.AspNetCoreServer;
using ClaimIq.Api.Configuration;


//////////////////////////////
//// SET UP THE BUILDER ! ////
//////////////////////////////

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddSingleton<IClaimsDataService, ClaimsDataService>();
builder.Services.AddHealthChecks();

// Swagger up
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
app.MapGet("/api", () => new { message = "API root", endpoints = new[] { "/api/claims" } });

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Use old school Swagger for .NET 8
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHealthChecks("/health");


// app.UseHttpsRedirection(); // Not needed because Lambda
app.UseCors("BlazorPolicy");
app.MapControllers();

app.Run();