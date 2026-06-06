using System.Text;
using System.Text.Json.Serialization;
using Fixnow.Data;
using Fixnow.Middleware;
using Fixnow.Middlewares;
using Fixnow.Repositories;
using Fixnow.Repositories.Interfaces;
using Fixnow.Services;
using Fixnow.Services.Interfaces;
using Fixnow.Services.Providers;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Amazon.S3;
using Amazon.Runtime;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Npgsql;
using Serilog;
using Serilog.Events;
using System.Threading.RateLimiting;

Log.Logger = new LoggerConfiguration()
  .MinimumLevel.Information()
  .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
  .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
  .Enrich.FromLogContext()
  .WriteTo.Console()
    .WriteTo.Conditional(
        evt => Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "Production",
        wt => wt.Seq("http://localhost:5341"))
    .CreateLogger();

try {
  Log.Information("Application is starting up...");
  var builder = WebApplication.CreateBuilder(args);

  // Use Serilog
  builder.Host.UseSerilog();

// ─── Database ─────────────────────────────────────────────────────────────────
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
if (!string.IsNullOrEmpty(databaseUrl))
{
    try {
        if (databaseUrl.StartsWith("postgres://") || databaseUrl.StartsWith("postgresql://")) {
            var uri = new Uri(databaseUrl);
            var userInfo = uri.UserInfo.Split(':');
            // Check if host is internal (like Render's internal DB host)
            bool isInternal = !uri.Host.Contains(".") || uri.Host.EndsWith("-a"); 
            var builderConn = new Npgsql.NpgsqlConnectionStringBuilder {
                Host = uri.Host,
                Port = uri.Port > 0 ? uri.Port : 5432,
                Database = uri.AbsolutePath.TrimStart('/'),
                Username = userInfo[0],
                Password = userInfo.Length > 1 ? userInfo[1] : "",
                SslMode = isInternal ? SslMode.Disable : SslMode.Require,
                Timeout = 15,
                CommandTimeout = 30
            };

            connectionString = builderConn.ToString();
        } else {
            connectionString = databaseUrl; // Assume it's already a valid Npgsql connection string
        }
    } catch (Exception ex) {
        Log.Warning(ex, "Error parsing DATABASE_URL. Falling back to DefaultConnection.");
    }
}

// Ensure connection string is found
if (string.IsNullOrEmpty(connectionString))
{
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
}

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Database connection string is not configured.");
}

// Log connection info (SAFE - excludes password)
try {
    var cb = new Npgsql.NpgsqlConnectionStringBuilder(connectionString);
    Log.Information("Database connection target: Host={Host}, Port={Port}, Database={Database}, SslMode={SslMode}", 
        cb.Host, cb.Port, cb.Database, cb.SslMode);
} catch {
    Log.Warning("Could not parse connection string for diagnostic logging.");
}

var dataSourceBuilder = new Npgsql.NpgsqlDataSourceBuilder(connectionString);
dataSourceBuilder.UseNetTopologySuite();
var dataSource = dataSourceBuilder.Build();

// Test database connection before building the app
try {
    Log.Information("Testing database connection (with 5s timeout)...");
    using var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(5));
    using var connection = await dataSource.OpenConnectionAsync(cts.Token);
    Log.Information("Database connection test successful.");
} catch (OperationCanceledException) {
    Log.Error("Database connection test timed out after 5 seconds.");
} catch (Exception ex) {
    Log.Error(ex, "Database connection test failed. This might cause a hang later.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
  options.UseNpgsql(dataSource, o => {
    o.UseNetTopologySuite();
    o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
  })
);

// ─── JWT Authentication ───────────────────────────────────────────────────────
var jwtSecret = builder.Configuration["Jwt:SecretKey"]
  ?? throw new InvalidOperationException("JWT SecretKey is not configured.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
  .AddJwtBearer(options =>
  {
    options.TokenValidationParameters = new TokenValidationParameters
    {
      ValidateIssuer = true,
      ValidateAudience = true,
      ValidateLifetime = true,
      ValidateIssuerSigningKey = true,
      ValidIssuer = builder.Configuration["Jwt:Issuer"],
      ValidAudience = builder.Configuration["Jwt:Audience"],
      IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
    };
    options.Events = new JwtBearerEvents
    {
      OnMessageReceived = context =>
      {
        var accessToken = context.Request.Query["access_token"];
        var path = context.HttpContext.Request.Path;
        if (!string.IsNullOrEmpty(accessToken) && 
            (path.StartsWithSegments("/chatHub") || path.StartsWithSegments("/hubs/notification")))
        {
          context.Token = accessToken;
        }
        return Task.CompletedTask;
      }
    };
  });

builder.Services.AddAuthorization();

// ─── CORS ─────────────────────────────────────────────────────────────────────
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

// Support comma-separated string from environment variable "Cors__AllowedOrigins"
var rawEnvCors = Environment.GetEnvironmentVariable("Cors__AllowedOrigins");
if (!string.IsNullOrEmpty(rawEnvCors))
{
    allowedOrigins = rawEnvCors.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                .Select(o => o.Trim())
                                .ToArray();
}

builder.Services.AddCors(options =>
{
  options.AddDefaultPolicy(policy =>
  {
    policy.WithOrigins(allowedOrigins)
          .AllowAnyHeader()
          .AllowAnyMethod()
          .AllowCredentials(); 
  });
});

// ─── Repositories ─────────────────────────────────────────────────────────────
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IWorkerLocationRepository, WorkerLocationRepository>();
builder.Services.AddScoped<IBookingMatchingLogRepository, BookingMatchingLogRepository>();
builder.Services.AddScoped<IServiceCategoryRepository, ServiceCategoryRepository>();
builder.Services.AddScoped<IBookingStatusHistoryRepository, BookingStatusHistoryRepository>();

builder.Services.AddScoped<IWorkerProfileRepository, WorkerProfileRepository>();
builder.Services.AddScoped<IWorkerKycRepository, WorkerKycRepository>();
builder.Services.AddScoped<IWorkerServiceRepository, WorkerServiceRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IFileRepository, FileRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();

// ─── Services ─────────────────────────────────────────────────────────────────
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IOtpService, OtpService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IMatchingService, MatchingService>();
builder.Services.AddScoped<IBookingService, BookingService>();

builder.Services.AddScoped<IWorkerProfileService, WorkerProfileService>();
builder.Services.AddScoped<IKycService, KycService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<INotificationInboxService, NotificationInboxService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IQuotationService, QuotationService>();
builder.Services.AddScoped<IPromotionService, PromotionService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

// Register Payment Providers
builder.Services.AddScoped<IPaymentProvider, VNPayProvider>();
builder.Services.AddScoped<IPaymentProvider, MoMoProvider>();
builder.Services.AddScoped<IPaymentProvider, SePayProvider>();

builder.Services.AddScoped<IConversationRepository, ConversationRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IBookingJobService, BookingJobService>();
builder.Services.AddScoped<ISystemJobService, SystemJobService>();

builder.Services.AddScoped<IQuotationRepository, QuotationRepository>();
builder.Services.AddScoped<IOpenJobRepository, OpenJobRepository>();
builder.Services.AddScoped<IOfferRepository, OfferRepository>();

builder.Services.AddScoped<IQuotationService, QuotationService>();
builder.Services.AddScoped<IOpenJobService, OpenJobService>();

builder.Services.AddScoped<IWalletRepository, WalletRepository>();
builder.Services.AddScoped<IWalletTransactionRepository, WalletTransactionRepository>();
builder.Services.AddScoped<IWithdrawalRepository, WithdrawalRepository>();
builder.Services.AddScoped<IPromotionRepository, PromotionRepository>();
builder.Services.AddScoped<IWalletService, WalletService>();

// Register Repositories P4 Dispute
builder.Services.AddScoped<IDisputeRepository, DisputeRepository>();
builder.Services.AddScoped<IRefundRepository, RefundRepository>();
builder.Services.AddScoped<IDisputeService, DisputeService>();

builder.Services.AddHttpClient<IGeminiService, GeminiService>();

// Configure Cloudflare R2 (S3 Compatible)
var r2AccessKey = builder.Configuration["CloudflareR2:AccessKey"];
var r2SecretKey = builder.Configuration["CloudflareR2:SecretKey"];
var r2ServiceUrl = builder.Configuration["CloudflareR2:ServiceURL"];

if (!string.IsNullOrEmpty(r2AccessKey) && !string.IsNullOrEmpty(r2SecretKey) && !string.IsNullOrEmpty(r2ServiceUrl))
{
    var awsCredentials = new BasicAWSCredentials(r2AccessKey, r2SecretKey);
    var s3Config = new AmazonS3Config
    {
        ServiceURL = r2ServiceUrl,
        ForcePathStyle = true // Required for some S3 compatible services like R2 depending on URL structure
    };
    builder.Services.AddSingleton<IAmazonS3>(new AmazonS3Client(awsCredentials, s3Config));
}

builder.Services.AddSingleton<Microsoft.AspNetCore.SignalR.IUserIdProvider, Fixnow.Providers.CustomUserIdProvider>();
builder.Services.AddSignalR();

// ─── Hangfire ─────────────────────────────────────────────────────────────────
Log.Information("Configuring Hangfire Services...");
builder.Services.AddHangfire(config => config
  .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
  .UseSimpleAssemblyNameTypeSerializer()
  .UseRecommendedSerializerSettings()
  .UsePostgreSqlStorage(c => c.UseNpgsqlConnection(connectionString)));

builder.Services.AddHangfireServer();
Log.Information("Hangfire Services configured.");

// ─── Controllers + Swagger ────────────────────────────────────────────────────
builder.Services.AddControllers()
  .AddJsonOptions(options =>
  {
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
  });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
  options.SwaggerDoc("v1", new OpenApiInfo
  {
    Title = "FixNow API",
    Version = "v1",
    Description = "FixNow – On-demand home services platform API"
  });

  // JWT Bearer support in Swagger UI
  options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
  {
    Name = "Authorization",
    Type = SecuritySchemeType.Http,
    Scheme = "Bearer",
    BearerFormat = "JWT",
    In = ParameterLocation.Header,
    Description = "Enter: Bearer {your JWT token}"
  });

  options.AddSecurityRequirement(new OpenApiSecurityRequirement
  {
    {
      new OpenApiSecurityScheme
      {
        Reference = new OpenApiReference
        {
          Type = ReferenceType.SecurityScheme,
          Id = "Bearer"
        }
      },
      Array.Empty<string>()
    }
  });
});

// ─── Rate Limiting ────────────────────────────────────────────────────────────
builder.Services.AddRateLimiter(options =>
{
  options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
  
  options.OnRejected = async (context, token) =>
  {
    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
    logger.LogWarning("RATE_LIMIT_EXCEEDED: IP {Ip} exceeded rate limit on path {Path}",
      context.HttpContext.Connection.RemoteIpAddress, context.HttpContext.Request.Path);

    await context.HttpContext.Response.WriteAsync("Too many requests. Please try again later.", cancellationToken: token);
  };

  // Global Limit: 100 req/min per IP
  options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    RateLimitPartition.GetFixedWindowLimiter(
      partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? httpContext.Request.Headers.Host.ToString(),
      factory: partition => new FixedWindowRateLimiterOptions
      {
        AutoReplenishment = true,
        PermitLimit = 100,
        QueueLimit = 0,
        Window = TimeSpan.FromMinutes(1)
      }));

  // Login Limit: 5 req/min per IP
  options.AddPolicy("login-policy", httpContext =>
    RateLimitPartition.GetFixedWindowLimiter(
      partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? httpContext.Request.Headers.Host.ToString(),
      factory: partition => new FixedWindowRateLimiterOptions
      {
        AutoReplenishment = true,
        PermitLimit = 5,
        QueueLimit = 0,
        Window = TimeSpan.FromMinutes(1)
      }));

  // OTP Limit: 3 req/5min per IP
  options.AddPolicy("otp-policy", httpContext =>
    RateLimitPartition.GetFixedWindowLimiter(
      partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? httpContext.Request.Headers.Host.ToString(),
      factory: partition => new FixedWindowRateLimiterOptions
      {
        AutoReplenishment = true,
        PermitLimit = 3,
        QueueLimit = 0,
        Window = TimeSpan.FromMinutes(5)
      }));
});

// ─── Build ────────────────────────────────────────────────────────────────────
Log.Information("Building the WebApplication...");
var app = builder.Build();
Log.Information("WebApplication built successfully.");

// ─── Middleware Pipeline ──────────────────────────────────────────────────────
app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();

app.UseCors();

app.UseRateLimiter();

if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

// Serve static files from wwwroot (for KYC image uploads)
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<Fixnow.Hubs.ChatHub>("/chatHub");
app.MapHub<Fixnow.Hubs.NotificationHub>("/hubs/notification");

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
  Authorization = new[] { new Fixnow.Filters.HangfireAuthorizationFilter() }
});

// Setup Recurring Jobs in Background
app.Lifetime.ApplicationStarted.Register(() =>
{
    try {
        using (var scope = app.Services.CreateScope())
        {
            Log.Information("Registering Recurring Jobs in background...");
            var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
            recurringJobManager.AddOrUpdate<ISystemJobService>(
                "system-cleanup-job",
                service => service.CleanupExpiredDataAsync(),
                Cron.Hourly);
            Log.Information("Recurring Jobs registered successfully.");
        }
    } catch (Exception ex) {
        Log.Error(ex, "Failed to register recurring jobs.");
    }
});

if (args.Length > 0 && args[0] == "seed")
{
    // await SeedData.Initialize(app.Services);
}

// Auto apply migrations on startup (for Cloud Providers like Render, Neon)
using (var scope = app.Services.CreateScope())
{
    try
    {
        Log.Information("Applying database migrations...");
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
        Log.Information("Database migrations applied successfully.");

        // Auto-create Admin user if not exists
        var adminExists = db.Users.Any(u => u.Role == Fixnow.Enums.UserRole.ADMIN);
        if (!adminExists)
        {
            var adminUser = new Fixnow.Entities.User
            {
                Email = "admin@fixnow.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123456"),
                FullName = "System Admin",
                Role = Fixnow.Enums.UserRole.ADMIN,
                Status = "ACTIVE",
                EmailVerified = true,
                PhoneNumber = "0123456789"
            };
            db.Users.Add(adminUser);
            db.SaveChanges();
            Log.Information("Default Admin account created successfully.");
        }
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Failed to apply database migrations or seed admin.");
    }
}

Log.Information("Calling app.Run()...");
if (Environment.GetEnvironmentVariable("ASPNETCORE_URLS") != null || Environment.GetEnvironmentVariable("PORT") != null)
{
    var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
    app.Run($"http://0.0.0.0:{port}");
}
else
{
    app.Run();
}
}
catch (Exception ex)
{
  Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
  Log.CloseAndFlush();
}
