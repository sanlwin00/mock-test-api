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

builder.Services.AddCarter();

// Add anti-forgery services
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-XSRF-TOKEN";
    options.Cookie.Name = "XSRF-TOKEN";
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.None;
});

// Bind settings from configuration
builder.Services.Configure<MailTemplateSettings>(builder.Configuration.GetSection("MailTemplateSettings"));
builder.Services.Configure<OpenApiSetting>(builder.Configuration.GetSection("OpenApi"));
builder.Services.Configure<MyDbSettings>(builder.Configuration.GetSection("MongoDB"));
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
// Add Smtp settings
builder.Services.Configure<SmtpSettings>("Smtp1", builder.Configuration.GetSection("SmtpSettings:Smtp1"));
builder.Services.Configure<SmtpSettings>("Smtp2", builder.Configuration.GetSection("SmtpSettings:Smtp2"));
builder.Services.Configure<SmtpSettings>("Smtp3", builder.Configuration.GetSection("SmtpSettings:Smtp3"));
builder.Services.Configure<SendGridApiSettings>(builder.Configuration.GetSection("EmailApi:SendGrid"));
builder.Services.Configure<BrevoApiSettings>(builder.Configuration.GetSection("EmailApi:Brevo"));


// Configure JWT 
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);
var issuer = jwtSettings["Issuer"];
var audience = jwtSettings["Audience"];

// Configure Stripe
Stripe.StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];


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

// Configure MongoDB
MongoUtility.RegisterConventions();
var settings = builder.Services.BuildServiceProvider().GetRequiredService<IOptions<MyDbSettings>>().Value;
var connectionString = Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING") ?? settings.ConnectionString;
builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    if (string.IsNullOrEmpty(connectionString))
        throw new InvalidOperationException("MONGODB_CONNECTION_STRING environment variable is not set.");
    try
    {
        var client = new MongoClient(connectionString);
        var database = client.GetDatabase(settings.DatabaseName);
        
        // perform a ping test to ensure the connection is successful
        database.RunCommandAsync((Command<BsonDocument>)"{ping:1}").Wait();

        return database;
    }
    catch (MongoConnectionException ex)
    {
        // Log the exception and provide a meaningful error message
        Console.WriteLine($"Error connecting to MongoDB {settings.ConnectionString}: {ex.Message}");
        throw new ApplicationException("Failed to connect to the MongoDB server. Please check your connection settings.", ex);
    }
    catch (TimeoutException ex)
    {
        Console.WriteLine($"Connection to MongoDB {settings.ConnectionString} timed out: {ex.Message}");
        throw new ApplicationException("MongoDB connection timed out. Please check if the server is running and accessible.", ex);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An unexpected error occurred while connecting to MongoDB: {ex.Message}");
        throw new ApplicationException("An unexpected error occurred while connecting to the MongoDB server.", ex);
    }
});

// Add Hangfire services
//builder.Services.AddHangfire(x => x.UseMongoStorage($"{connectionString}/{settings.DatabaseName}"));
builder.Services.AddHangfire(x =>
    x.UseMongoStorage($"{connectionString}", new MongoStorageOptions
    {
        MigrationOptions = new MongoMigrationOptions
        {
            MigrationStrategy = new DropMongoMigrationStrategy()
        }
    }));
builder.Services.AddHangfireServer();

RegisterRepositories(builder.Services);
//-------

builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddHttpClient();
builder.Services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserStore, MongoUserStore>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IPasswordResetService, PasswordResetService>();
builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
builder.Services.AddScoped<IQuestionService, QuestionService>();
builder.Services.AddScoped<ITestRepository, TestRepository>();
builder.Services.AddScoped<ITestService, TestService>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<IMockTestRepository, MockTestRepository>();
builder.Services.AddScoped<IMockTestService, MockTestService>();
builder.Services.AddScoped<IMockTestHistoryRepository, MockTestHistoryRepository>();
builder.Services.AddScoped<IMockTestHistoryService, MockTestHistoryService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IReferenceMaterialRepository, ReferenceMaterialRepository>();    
builder.Services.AddScoped<IReferenceMaterialService, ReferenceMaterialService>();

// Register concrete implementations of EmailServiceHandler
builder.Services.AddTransient<SmtpEmailServiceHandler>(sp =>
{
    var smtp1 = sp.GetRequiredService<IOptionsSnapshot<SmtpSettings>>().Get("Smtp1");
    return new SmtpEmailServiceHandler(Options.Create(smtp1));
});

builder.Services.AddTransient<SmtpEmailServiceHandler>(sp =>
{
    var smtp2 = sp.GetRequiredService<IOptionsSnapshot<SmtpSettings>>().Get("Smtp2");
    return new SmtpEmailServiceHandler(Options.Create(smtp2));
});

builder.Services.AddTransient<SmtpEmailServiceHandler>(sp =>
{
    var smtp3 = sp.GetRequiredService<IOptionsSnapshot<SmtpSettings>>().Get("Smtp3");
    return new SmtpEmailServiceHandler(Options.Create(smtp3));
});

builder.Services.AddTransient<SendGridEmailServiceHandler>();

builder.Services.AddHttpClient<BrevoEmailServiceHandler>((sp, client) =>
{
    var apiSettings = sp.GetRequiredService<IOptionsMonitor<BrevoApiSettings>>().CurrentValue;

    client.BaseAddress = new Uri(apiSettings.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30); 
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.DefaultRequestHeaders.Add("api-key", apiSettings.ApiKey); 
});

// Register NotificationService
builder.Services.AddTransient<INotificationService>(sp =>
{
    // Retrieve and chain SMTP handlers for general notifications
    var smtpHandlers = sp.GetServices<SmtpEmailServiceHandler>().ToList();
    var smtp1Handler = smtpHandlers[0];
    var smtp2Handler = smtpHandlers[1];
    var smtp3Handler = smtpHandlers[2];

    // Set up the SMTP chain for failover
    smtp1Handler.SetNext(smtp2Handler)
                .SetNext(smtp3Handler);
    
    var sendGridHandler = sp.GetRequiredService<SendGridEmailServiceHandler>();
    var brevoHandler = sp.GetRequiredService<BrevoEmailServiceHandler>();
    var templateSettings = sp.GetRequiredService<IOptions<MailTemplateSettings>>();

    return new NotificationService(
        templateSettings,
        generalNotificationService: sp.GetRequiredService<BrevoEmailServiceHandler>(),
        transactionalNotificationService: sp.GetRequiredService<SendGridEmailServiceHandler>(),
        notificationRepository: sp.GetRequiredService<INotificationRepository>(),
        backgroundJobClient: sp.GetRequiredService<IBackgroundJobClient>()
    );
});

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

//* Configure SeriLog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration).CreateLogger();
builder.Host.UseSerilog();

var app = builder.Build();

app.UseSerilogRequestLogging(); //* log every request

app.MapCarter();

app.UseHangfireDashboard();

app.UseCors("AllowSpecificOrigins");

//** Moved up to solve 'Anti-Forgery Token was meant for a different claims-based user' issue
app.UseAuthentication();

// Use anti-forgery middleware
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
var version = builder.Configuration.GetValue<string>("ApiVersion");
app.MapGet("/", () => $"Hello World! v{version}");

// Seed data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var database = services.GetRequiredService<IMongoDatabase>();

    MongoUtility.SeedCollection(database, "users", DummyData.GetUsers());
    MongoUtility.SeedCollection(database, "questions", DummyData.GetQuestions());
    MongoUtility.SeedCollection(database, "tests", DummyData.GetTests());
    MongoUtility.SeedCollection(database, "reference_materials", DummyData.GetReferenceMaterials());
    MongoUtility.SeedCollection(database, "notifications", DummyData.GetNotifications());
    //MongoUtility.SeedCollection(database, "payments", DummyData.GetPayments());
    //MongoUtility.SeedCollection(database, "audit_logs", DummyData.GetAuditLogs());
    //MongoUtility.SeedCollection(database, "mock_tests", DummyData.GetMockTests());
    //MongoUtility.SeedCollection(database, "mock_test_histories", DummyData.GetMockTestHistories());
}

app.Run();


void RegisterRepositories(IServiceCollection services)
{
    services.AddScoped<IRepository<User>>(sp =>
    {
        var database = sp.GetRequiredService<IMongoDatabase>();
        return new MongoRepository<User>(database, "users");
    });
    services.AddScoped<IRepository<Question>>(sp =>
    {
        var database = sp.GetRequiredService<IMongoDatabase>();
        return new MongoRepository<Question>(database, "questions");
    });
    services.AddScoped<IRepository<Test>>(sp =>
    {
        var database = sp.GetRequiredService<IMongoDatabase>();
        return new MongoRepository<Test>(database, "tests");
    });
    services.AddScoped<IRepository<Payment>>(sp =>
    {
        var database = sp.GetRequiredService<IMongoDatabase>();
        return new MongoRepository<Payment>(database, "payments");
    });
    services.AddScoped<IRepository<Notification>>(sp =>
    {
        var database = sp.GetRequiredService<IMongoDatabase>();
        return new MongoRepository<Notification>(database, "notifications");
    });
    services.AddScoped<IRepository<AuditLog>>(sp =>
    {
        var database = sp.GetRequiredService<IMongoDatabase>();
        return new MongoRepository<AuditLog>(database, "audit_logs");
    });
    services.AddScoped<IRepository<MockTest>>(sp =>
    {
        var database = sp.GetRequiredService<IMongoDatabase>();
        return new MongoRepository<MockTest>(database, "mock_tests");
    });
    services.AddScoped<IRepository<MockTestHistory>>(sp =>
    {
        var database = sp.GetRequiredService<IMongoDatabase>();
        return new MongoRepository<MockTestHistory>(database, "mock_test_histories");
    });
    services.AddScoped<IRepository<PasswordResetToken>>(sp =>
    {
        var database = sp.GetRequiredService<IMongoDatabase>();
        return new MongoRepository<PasswordResetToken>(database, "password_reset_token");
    });
    services.AddScoped<IRepository<ReferenceMaterial>>(sp =>
    {
        var database = sp.GetRequiredService<IMongoDatabase>();
        return new MongoRepository<ReferenceMaterial>(database, "reference_materials");
    });
}




