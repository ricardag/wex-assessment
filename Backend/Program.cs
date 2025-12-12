using System.Globalization;
using System.Text;
using System.Threading.RateLimiting;
using Backend.Application.Configuration;
using Backend.Application.Interfaces;
using Backend.Application.Services;
using Backend.Domain.Interfaces;
using Backend.InfraStructure.Data;
using Backend.Infrastructure.Data.Repositories;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Polly;
using Serilog;

// Get secrets from .env file
// Priority: ENV_FILE_PATH > .env.{environment} > .env
var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..");
var envFilePath = Environment.GetEnvironmentVariable("ENV_FILE_PATH");

if (!string.IsNullOrEmpty(envFilePath) && File.Exists(envFilePath))
    {
    Env.Load(envFilePath);
    }
else
    {
    // Fallback: .env.{environment} or .env
    var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
    var envPath = Path.Combine(basePath, $".env.{environment.ToLower()}");

    if (!File.Exists(envPath))
        {
        envPath = Path.Combine(basePath, ".env");
        }

    if (File.Exists(envPath))
        {
        Env.Load(envPath);
        }
    }

var builder = WebApplication.CreateBuilder(args);

// Sets global culture
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

builder.Services.AddSwaggerGen(c =>
    {
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Wex Assesment API", Version = "v1" });

    var security = new OpenApiSecurityRequirement()
        {
            {
            new OpenApiSecurityScheme
                {
                Reference = new OpenApiReference
                    {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                    },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
                },
            new List<string>()
            }
        };

    c.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
            {
            In = ParameterLocation.Header,
            Description = "Paste 'Bearer ' + token'",
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey
            });

    c.AddSecurityRequirement(security);
    });

// Works behind Proxies
builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    });

// Logging Client IP
builder.Services.AddHttpContextAccessor();

// Adding Rate Limiting
builder.Services.AddRateLimiter(options =>
    {
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: partition => new FixedWindowRateLimiterOptions
                {
                AutoReplenishment = true,
                PermitLimit = 100,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
                }));
    options.RejectionStatusCode = 429; // Too Many Requests
    });

// Adding CORS
builder.Services.AddCors(options =>
    {
    options.AddPolicy(name: "CorsPolicy",
        bld =>
            {
            bld.WithOrigins(
                    "http://localhost:4200" // Angular Site
                    )
                //.SetIsOriginAllowedToAllowWildcardSubdomains()
                .WithMethods("GET", "POST", "PUT", "DELETE")
                .WithHeaders("Content-Type", "Content-Disposition", "Authorization");
            });
    });

// Adding Newtonsoft Json
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
        {
        options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
        options.SerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
        options.SerializerSettings.DateFormatString = "yyyy-MM-dd'T'HH:mm:ss'Z'";
        });

// Configuring JWT Settings
var jwtSettings = new JwtSettings
    {
    Secret = Environment.GetEnvironmentVariable("JWT_SECRET")
             ?? throw new InvalidOperationException("JWT_SECRET environment variable is required"),
    Issuer = builder.Configuration.GetValue<string>("JWT:Issuer")
             ?? throw new InvalidOperationException("JWT:Issuer is required"),
    Audience = builder.Configuration.GetValue<string>("JWT:Audience")
             ?? throw new InvalidOperationException("JWT:Audience is required"),
    ExpirationMinutes = builder.Configuration.GetValue<int?>("JWT:ExpirationMinutes")
             ?? throw new InvalidOperationException("JWT:ExpirationMinutes is required"),
    RefreshTokenExpirationMinutes = builder.Configuration.GetValue<int?>("JWT:RefreshTokenExpirationMinutes")
             ?? throw new InvalidOperationException("JWT:RefreshTokenExpirationMinutes is required")
    };
builder.Services.AddSingleton(jwtSettings);


var treasuryApiSettings = new TreasuryApiSettings
    {
    TreasuryApiBaseUrl = builder.Configuration.GetValue<string>("TreasuryApiBaseUrl")
                         ?? throw new InvalidOperationException("TreasuryApiBaseUrl is required"),
    };
builder.Services.AddSingleton(treasuryApiSettings);

// Adding JwtBearer
var key = Encoding.ASCII.GetBytes(jwtSettings.Secret);
builder.Services.AddAuthentication(x =>
        {
        x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
    .AddJwtBearer(x =>
        {
        x.RequireHttpsMetadata = false;
        x.SaveToken = true;
        x.TokenValidationParameters = new TokenValidationParameters
            {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience
            };
        });

// Using Serilog
builder.Host.UseSerilog((context, services, conf) => conf
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithClientIp()
    );

// Adding EF Core
var mySqlHost = Environment.GetEnvironmentVariable("MARIADB_HOST") ?? "127.0.0.1";
var mySqlPort = Environment.GetEnvironmentVariable("MARIADB_PORT") ?? "3306";
var mySqlDatabase = Environment.GetEnvironmentVariable("MARIADB_DATABASE")
                    ?? throw new InvalidOperationException("MARIADB_DATABASE environment variable is required");
var mySqlUser = Environment.GetEnvironmentVariable("MARIADB_USER")
                ?? throw new InvalidOperationException("MARIADB_USER environment variable is required");
var mySqlPassword = Environment.GetEnvironmentVariable("MARIADB_PASSWORD")
                    ?? throw new InvalidOperationException("MARIADB_PASSWORD environment variable is required");

var mySqlConnStr =
    $"Server={mySqlHost};Port={mySqlPort};Database={mySqlDatabase};User={mySqlUser};Password={mySqlPassword};SslMode=None";

// Registering DbContext
builder.Services.AddDbContext<AppDbContext>(opt =>
    {
    opt.UseMySql(mySqlConnStr, ServerVersion.AutoDetect(mySqlConnStr));
    });

// Registering Repositories
builder.Services.AddScoped<IPurchaseRepository, PurchaseRepository>();
builder.Services.AddScoped<ICountryCurrencyRepository, CountryCurrencyRepository>();

// Registering Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPurchaseService, PurchaseService>();
builder.Services.AddScoped<ICountryCurrencyService, CountryCurrencyService>();
builder.Services.AddScoped<IExchangeRateService, ExchangeRateService>();

// Registering HttpClient with Polly retry policy
//    3 retries with linear backoff (5s, 6s, 7s = 18s total delay)
//    30s timeout max (allows ~12s for actual requests across all retries)
builder.Services.AddHttpClient<IHttpService, HttpService>()
    .AddTransientHttpErrorPolicy(policyBuilder =>
        policyBuilder.WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(4 + retryAttempt),
            onRetry: (outcome, timespan, retryAttempt, _) =>
                {
                Log.Warning("HttpClient retry {RetryAttempt} after {Delay}s due to {Exception}",
                    retryAttempt, timespan.TotalSeconds, outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString());
                }))
    .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(30)));

// Registering Hosted Services for loading country/currencies from the Treasury API
builder.Services.AddHostedService<CurrencySyncHostedService>();

var app = builder.Build();

app.UseSerilogRequestLogging();

app.UseForwardedHeaders();
app.UseStaticFiles();
app.UseRouting();

if (app.Environment.IsDevelopment())
    {
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
    }

app.UseCors("CorsPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.MapControllers();

app.Run();

// Required for WebApplicationFactory in integration tests
public partial class Program { }