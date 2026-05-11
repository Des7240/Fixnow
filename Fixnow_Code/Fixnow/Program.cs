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
  var builder = WebApplication.CreateBuilder(args);

  // Use Serilog
  builder.Host.UseSerilog();

// ─── Database ─────────────────────────────────────────────────────────────────
var dataSourceBuilder = new Npgsql.NpgsqlDataSourceBuilder(builder.Configuration.GetConnectionString("DefaultConnection"));
dataSourceBuilder.UseNetTopologySuite();
var dataSource = dataSourceBuilder.Build();

builder.Services.AddDbContext<AppDbContext>(options =>
  options.UseNpgsql(dataSource, o => o.UseNetTopologySuite())
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
var allowedOrigins = builder.Configuration
  .GetSection("Cors:AllowedOrigins")
  .Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
  options.AddDefaultPolicy(policy =>
  {
    policy.WithOrigins(allowedOrigins)
          .AllowAnyHeader()
          .AllowAnyMethod()
          .AllowCredentials(); // Required for cookies
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
builder.Services.AddScoped<IPaymentService, PaymentService>();

// Register Payment Providers
builder.Services.AddScoped<IPaymentProvider, VNPayProvider>();
builder.Services.AddScoped<IPaymentProvider, MoMoProvider>();

builder.Services.AddScoped<IConversationRepository, ConversationRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IBookingJobService, BookingJobService>();
builder.Services.AddScoped<ISystemJobService, SystemJobService>();

builder.Services.AddScoped<IQuotationRepository, QuotationRepository>();
builder.Services.AddScoped<IQuotationService, QuotationService>();

builder.Services.AddScoped<IWalletRepository, WalletRepository>();
builder.Services.AddScoped<IWalletTransactionRepository, WalletTransactionRepository>();
builder.Services.AddScoped<IWithdrawalRepository, WithdrawalRepository>();
builder.Services.AddScoped<IWalletService, WalletService>();

builder.Services.AddScoped<IDisputeRepository, DisputeRepository>();
builder.Services.AddScoped<IRefundRepository, RefundRepository>();
builder.Services.AddScoped<IDisputeService, DisputeService>();

builder.Services.AddSignalR();

// ─── Hangfire ─────────────────────────────────────────────────────────────────
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddHangfire(config => config
  .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
  .UseSimpleAssemblyNameTypeSerializer()
  .UseRecommendedSerializerSettings()
  .UsePostgreSqlStorage(c => c.UseNpgsqlConnection(connectionString)));

builder.Services.AddHangfireServer();

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
var app = builder.Build();

// ─── Middleware Pipeline ──────────────────────────────────────────────────────
app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();

app.UseRateLimiter();

// Request logging via Serilog
app.UseSerilogRequestLogging(options =>
{
  // Attach correlation ID to the request log
  options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
  {
    if (httpContext.Request.Headers.TryGetValue("X-Correlation-ID", out var correlationId))
    {
      diagnosticContext.Set("CorrelationId", correlationId);
    }
  };
});

if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI(options =>
  {
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "FixNow API v1");
    options.RoutePrefix = "swagger";
  });
}

app.UseCors();
app.UseHttpsRedirection();

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

// Setup Recurring Jobs
RecurringJob.AddOrUpdate<ISystemJobService>(
  "system-cleanup-job",
  service => service.CleanupExpiredDataAsync(),
  Cron.Daily);

app.Run();
}
catch (Exception ex)
{
  Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
  Log.CloseAndFlush();
}
