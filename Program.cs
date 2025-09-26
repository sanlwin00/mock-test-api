using AspNetCoreRateLimit;
using Carter;
using Hangfire;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MockTestApi.Data;
using MockTestApi.Data.Interfaces;
using MockTestApi.Mapping;
using MockTestApi.Models;
using MockTestApi.Services;
using MockTestApi.Services.Interfaces;
using MockTestApi.Utils;
using MongoDB.Bson;
using MongoDB.Driver;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

ConfigureServices(builder);
ConfigureAuthentication(builder);
ConfigureDatabase(builder);
ConfigureHangfire(builder);
ConfigureCors(builder);
ConfigureLogging(builder);
ConfigureRateLimiting(builder.Services);

var app = builder.Build();

ConfigureMiddleware(app);

var version = builder.Configuration.GetValue<string>("ApiVersion");
app.MapGet("/", () => $"Hello World! v{version}");

SeedData(app);

app.UseIpRateLimiting();

app.Run();

void ConfigureServices(WebApplicationBuilder builder)
{
    builder.Services.AddCarter();
    builder.Services.AddAntiforgery(options =>
    {
        options.HeaderName = "X-XSRF-TOKEN";
        options.Cookie.Name = "XSRF-TOKEN";
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.None;
    });
    
    builder.Services.Configure<MailTemplateSettings>(builder.Configuration.GetSection("MailTemplateSettings"));
    builder.Services.Configure<OpenApiSetting>(builder.Configuration.GetSection("OpenApi"));
    builder.Services.Configure<MyDbSettings>(builder.Configuration.GetSection("MongoDB"));
    builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
    builder.Services.Configure<SmtpSettings>("Smtp1", builder.Configuration.GetSection("SmtpSettings:Smtp1"));
    builder.Services.Configure<SmtpSettings>("Smtp2", builder.Configuration.GetSection("SmtpSettings:Smtp2"));
    builder.Services.Configure<SmtpSettings>("Smtp3", builder.Configuration.GetSection("SmtpSettings:Smtp3"));
    builder.Services.Configure<SendGridApiSettings>(builder.Configuration.GetSection("EmailApi:SendGrid"));
    builder.Services.Configure<BrevoApiSettings>(builder.Configuration.GetSection("EmailApi:Brevo"));
    Stripe.StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

    builder.Services.AddAutoMapper(typeof(MappingProfile));
    builder.Services.AddHttpClient();
    builder.Services.AddHttpContextAccessor();

    RegisterRepositories(builder.Services);
    RegisterServices(builder.Services);
    RegisterNotificationServices(builder.Services);
}

void ConfigureAuthentication(WebApplicationBuilder builder)
{
    var jwtSettings = builder.Configuration.GetSection("Jwt");
    var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);
    var issuer = jwtSettings["Issuer"];
    var audience = jwtSettings["Audience"];

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    });
}

void ConfigureDatabase(WebApplicationBuilder builder)
{
    MongoUtility.RegisterConventions();
    var settings = builder.Services.BuildServiceProvider().GetRequiredService<IOptions<MyDbSettings>>().Value;
    var connectionString = Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING") ?? settings.ConnectionString;

    builder.Services.AddSingleton<IMongoDatabase>(sp =>
    {
        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException("MONGODB_CONNECTION_STRING is not set.");

        try
        {
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            database.RunCommandAsync((Command<BsonDocument>)"{ping:1}").Wait();

            return database;
        }
        catch (Exception ex) when (ex is MongoConnectionException || ex is TimeoutException)
        {
            Console.WriteLine($"Failed to connect to MongoDB {settings.ConnectionString}");
            throw new ApplicationException("Failed to connect to the MongoDB server. Please check your connection settings.", ex);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error while connecting to MongoDB. {ex.Message}");
            throw new ApplicationException("An unexpected error occurred while connecting to the MongoDB server.", ex);
        }
    });
}

void ConfigureHangfire(WebApplicationBuilder builder)
{
    var settings = builder.Services.BuildServiceProvider().GetRequiredService<IOptions<MyDbSettings>>().Value;
    var connectionString = Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING") ?? settings.ConnectionString;

    builder.Services.AddHangfire(x => x.UseMongoStorage($"{connectionString}", new MongoStorageOptions
    {
        MigrationOptions = new MongoMigrationOptions
        {
            MigrationStrategy = new DropMongoMigrationStrategy()
        }
    }));
    builder.Services.AddHangfireServer();
}

void ConfigureCors(WebApplicationBuilder builder)
{
    var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>();

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowSpecificOrigins",
            policy =>
            {
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
    });
}

void ConfigureLogging(WebApplicationBuilder builder)
{
    Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(builder.Configuration)
        .CreateLogger();
    builder.Host.UseSerilog();
}

void RegisterRepositories(IServiceCollection services)
{
    services.AddScoped<IUserRepository, UserRepository>();
    services.AddScoped<IQuestionRepository, QuestionRepository>();
    services.AddScoped<ITestRepository, TestRepository>();
    services.AddScoped<IPaymentRepository, PaymentRepository>();
    services.AddScoped<INotificationRepository, NotificationRepository>();    
    services.AddScoped<IMockTestRepository, MockTestRepository>();
    services.AddScoped<IMockTestHistoryRepository, MockTestHistoryRepository>();
    services.AddScoped<IReferenceMaterialRepository, ReferenceMaterialRepository>();
    services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
    services.AddScoped<IAuditLogRepository, AuditLogRepository>();
}

void RegisterServices(IServiceCollection services)
{
    services.AddScoped<IUserService, UserService>();
    services.AddScoped<IAuthenticationService, AuthenticationService>();
    services.AddScoped<IPasswordResetService, PasswordResetService>();
    services.AddScoped<IQuestionService, QuestionService>();
    services.AddScoped<ITestService, TestService>();
    services.AddScoped<IPaymentService, PaymentService>();
    services.AddScoped<IAuditLogService, AuditLogService>();
    services.AddScoped<IMockTestService, MockTestService>();
    services.AddScoped<IMockTestHistoryService, MockTestHistoryService>();
    services.AddScoped<IChatService, ChatService>();
    services.AddScoped<IReferenceMaterialService, ReferenceMaterialService>();
}

void RegisterNotificationServices(IServiceCollection services)
{
    var serviceProvider = builder.Services.BuildServiceProvider();
    var smtpHandlers = new List<SmtpEmailServiceHandler>();

    // Check and register SMTP handlers only if they are active
    var smtp1Settings = builder.Configuration.GetSection("SmtpSettings:Smtp1").Get<SmtpSettings>();
    if (smtp1Settings?.IsActive == true)
    {
     builder.Services.AddTransient<SmtpEmailServiceHandler>(sp =>
   {
     var smtp1 = sp.GetRequiredService<IOptionsSnapshot<SmtpSettings>>().Get("Smtp1");
            return new SmtpEmailServiceHandler(Options.Create(smtp1));
      });
    }

    var smtp2Settings = builder.Configuration.GetSection("SmtpSettings:Smtp2").Get<SmtpSettings>();
    if (smtp2Settings?.IsActive == true)
    {
   builder.Services.AddTransient<SmtpEmailServiceHandler>(sp =>
        {
      var smtp2 = sp.GetRequiredService<IOptionsSnapshot<SmtpSettings>>().Get("Smtp2");
  return new SmtpEmailServiceHandler(Options.Create(smtp2));
        });
    }

 var smtp3Settings = builder.Configuration.GetSection("SmtpSettings:Smtp3").Get<SmtpSettings>();
    if (smtp3Settings?.IsActive == true)
    {
        builder.Services.AddTransient<SmtpEmailServiceHandler>(sp =>
        {
    var smtp3 = sp.GetRequiredService<IOptionsSnapshot<SmtpSettings>>().Get("Smtp3");
  return new SmtpEmailServiceHandler(Options.Create(smtp3));
        });
 }

    builder.Services.AddTransient<SendGridEmailServiceHandler>();

    builder.Services.AddHttpClient<BrevoEmailServiceHandler>((sp, client) =>
    {
        var apiSettings = sp.GetRequiredService<IOptionsMonitor<BrevoApiSettings>>().CurrentValue;

        client.BaseAddress = new Uri(apiSettings.BaseUrl);
        client.Timeout = TimeSpan.FromSeconds(30);
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        client.DefaultRequestHeaders.Add("api-key", apiSettings.ApiKey);
    });

    builder.Services.AddScoped<IEmailServiceHandler>(sp =>
    {
        // Retrieve active SMTP handlers for general notifications
        var smtpHandlers = sp.GetServices<SmtpEmailServiceHandler>().ToList();

        if (smtpHandlers.Count == 0)
        {
            // If no SMTP handlers are active, return null or a default handler
            return null;
        }

        // Set up the SMTP chain for failover
        for (int i = 0; i < smtpHandlers.Count - 1; i++)
        {
            smtpHandlers[i].SetNext(smtpHandlers[i + 1]);
        }

        return smtpHandlers[0];
    });
    
    builder.Services.AddTransient<INotificationService>(sp =>
    {
        return new NotificationService(
            templateSettings: sp.GetRequiredService<IOptions<MailTemplateSettings>>(),
     generalNotificationService: sp.GetRequiredService<BrevoEmailServiceHandler>(),
            transactionalNotificationService: sp.GetRequiredService<SendGridEmailServiceHandler>(),
     notificationRepository: sp.GetRequiredService<INotificationRepository>(),
            backgroundJobClient: sp.GetRequiredService<IBackgroundJobClient>()
    );
    });
}

void ConfigureMiddleware(WebApplication app)
{
    app.UseSerilogRequestLogging();
    app.MapCarter();
    app.UseHangfireDashboard();
    app.UseCors("AllowSpecificOrigins");
    app.UseAuthentication();
    app.UseAntiforgery();
    app.Use(async (context, next) =>
    {
        // Check if the request is for an API endpoint that requires anti-forgery
        if (context.Request.Path.StartsWithSegments("/emails/send-contact-form") &&
            context.Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase))
        {
            var antiforgery = context.RequestServices.GetRequiredService<IAntiforgery>();
            try
            {
                await antiforgery.ValidateRequestAsync(context);
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync($"Antiforgery validation failed: {ex.Message}");
                return;
            }
        }

        await next();
    });
    app.UseAuthorization();
}

void ConfigureRateLimiting(IServiceCollection services)
{
    services.AddMemoryCache();
    services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
    services.AddInMemoryRateLimiting();
    services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
    services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
    services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
}

void SeedData(WebApplication app)
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var database = services.GetRequiredService<IMongoDatabase>();

        MongoUtility.SeedCollection(database, "users", DummyData.GetUsers());
        MongoUtility.SeedCollection(database, "questions", DummyData.GetQuestions());
        MongoUtility.SeedCollection(database, "tests", DummyData.GetTests());
        MongoUtility.SeedCollection(database, "reference_materials", DummyData.GetReferenceMaterials());
        MongoUtility.SeedCollection(database, "notifications", DummyData.GetNotifications());
    }
}
