using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MockTestApi.Data;
using MongoDB.Driver;
using MockTestApi.Models;
using MockTestApi.Services;
using MockTestApi.Data.Interfaces;
using MockTestApi.Services.Interfaces;
using MockTestApi.Utils;
using MockTestApi.Mapping;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);

// Configure JWT 
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

// Configure MongoDB
MongoConfig.RegisterConventions();
builder.Services.Configure<MyDbSettings>(builder.Configuration.GetSection("MongoDB"));
builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MyDbSettings>>().Value;
    var client = new MongoClient(settings.ConnectionString);
    return client.GetDatabase(settings.DatabaseName);
});

RegisterRepositories(builder.Services);
//-------

builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserStore, MongoUserStore>();
builder.Services.AddScoped<IUserService, UserService>();
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

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});


var app = builder.Build();

app.UseCors("AllowSpecificOrigins");
app.UseAuthentication();
app.UseAuthorization();

// Seed data
/*
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    SeedData(services);
}
*/

// User endpoints
app.MapGet("/users", async (IUserService userService) => await userService.GetAllUsersAsync()).RequireAuthorization("AdminOnly");
app.MapGet("/users/{id}", async (IUserService userService, string id) => await userService.GetUserByIdAsync(id)).RequireAuthorization();
app.MapPost("/users", async (IUserService userService, User user) => await userService.CreateUserAsync(user));
app.MapPut("/users/{id}", async (IUserService userService, User user) => await userService.UpdateUserAsync(user)).RequireAuthorization();
app.MapDelete("/users/{id}", async (IUserService userService, string id) => await userService.DeleteUserAsync(id)).RequireAuthorization("AdminOnly");

// Auth endpoints
app.MapPost("/auth/register", async (IUserService userService, RegisterDto registerDto) =>
{    
    var loginResponse = await userService.RegisterUserAsync(registerDto);

    if (loginResponse == null)
        return Results.Unauthorized();
    else
    {
        return Results.Ok(new { loginResponse.Token, loginResponse.User });
    }
});
app.MapPost("/auth/login", async (IUserService userService, IMapper mapper, LoginRequest loginRequest) =>
{
    var loginResponse = await userService.AuthenticateAsync(loginRequest);

    if (loginResponse == null)
        return Results.Unauthorized();
    else
    {
        return Results.Ok(new { loginResponse.Token, loginResponse.User });
    }
});
app.MapGet("/auth/profile", async (HttpContext httpContext, IUserService userService) => {
    var userId = httpContext.User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
    if (userId == null)
    {
        return Results.Unauthorized();
    }

    var user = await userService.GetUserByIdAsync(userId);
    if (user == null)
    {
        return Results.NotFound();
    }
    
    return Results.Ok(user);
}).RequireAuthorization();


// Question endpoints
app.MapGet("/questions", async (IQuestionService questionService) => await questionService.GetAllQuestionsAsync());
app.MapGet("/questions/{id}", async (IQuestionService questionService, string id) => await questionService.GetQuestionByIdAsync(id));
app.MapPost("/questions", async (IQuestionService questionService, Question question) => await questionService.CreateQuestionAsync(question));
app.MapPut("/questions/{id}", async (IQuestionService questionService, Question question) => await questionService.UpdateQuestionAsync(question));
app.MapDelete("/questions/{id}", async (IQuestionService questionService, string id) => await questionService.DeleteQuestionAsync(id));

// Test endpoints
app.MapGet("/tests", async (ITestService testService) => await testService.GetAllTestsAsync());
app.MapGet("/tests/{id}", async (ITestService testService, string id) => await testService.GetTestByIdAsync(id));
app.MapPost("/tests", async (ITestService testService, Test test) => await testService.CreateTestAsync(test));
app.MapPut("/tests/{id}", async (ITestService testService, Test test) => await testService.UpdateTestAsync(test));
app.MapDelete("/tests/{id}", async (ITestService testService, string id) => await testService.DeleteTestAsync(id));

// Payment endpoints
app.MapGet("/payments", async (IPaymentService paymentService) => await paymentService.GetAllPaymentsAsync());
app.MapGet("/payments/{id}", async (IPaymentService paymentService, string id) => await paymentService.GetPaymentByIdAsync(id));
app.MapPost("/payments", async (IPaymentService paymentService, Payment payment) => await paymentService.CreatePaymentAsync(payment));
app.MapPut("/payments/{id}", async (IPaymentService paymentService, Payment payment) => await paymentService.UpdatePaymentAsync(payment));
app.MapDelete("/payments/{id}", async (IPaymentService paymentService, string id) => await paymentService.DeletePaymentAsync(id));

// Notification endpoints
app.MapGet("/notifications", async (INotificationService notificationService) => await notificationService.GetAllNotificationsAsync());
app.MapGet("/notifications/{id}", async (INotificationService notificationService, string id) => await notificationService.GetNotificationByIdAsync(id));
app.MapPost("/notifications", async (INotificationService notificationService, Notification notification) => await notificationService.CreateNotificationAsync(notification));
app.MapPut("/notifications/{id}", async (INotificationService notificationService, Notification notification) => await notificationService.UpdateNotificationAsync(notification));
app.MapDelete("/notifications/{id}", async (INotificationService notificationService, string id) => await notificationService.DeleteNotificationAsync(id));

// AuditLog endpoints
app.MapGet("/auditlogs", async (IAuditLogService auditLogService) => await auditLogService.GetAllAuditLogsAsync());
app.MapGet("/auditlogs/{id}", async (IAuditLogService auditLogService, string id) => await auditLogService.GetAuditLogByIdAsync(id));
app.MapPost("/auditlogs", async (IAuditLogService auditLogService, AuditLog auditLog) => await auditLogService.CreateAuditLogAsync(auditLog));
app.MapPut("/auditlogs/{id}", async (IAuditLogService auditLogService, AuditLog auditLog) => await auditLogService.UpdateAuditLogAsync(auditLog));
app.MapDelete("/auditlogs/{id}", async (IAuditLogService auditLogService, string id) => await auditLogService.DeleteAuditLogAsync(id));

// MockTest endpoints
app.MapGet("/mocktests", async (IMockTestService mockTestService) => await mockTestService.GetAllMockTestsAsync());
app.MapGet("/mocktests/{id}", async (IMockTestService mockTestService, string id) => await mockTestService.GetMockTestByIdAsync(id));
app.MapPost("/mocktests", async (IMockTestService mockTestService, MockTest mockTest) => await mockTestService.CreateMockTestAsync(mockTest));
app.MapPut("/mocktests/{id}", async (IMockTestService mockTestService, MockTest mockTest) => await mockTestService.UpdateMockTestAsync(mockTest));
app.MapDelete("/mocktests/{id}", async (IMockTestService mockTestService, string id) => await mockTestService.DeleteMockTestAsync(id));

// MockTestHistory endpoints
app.MapGet("/mocktesthistories", async (IMockTestHistoryService mockTestHistoryService) => await mockTestHistoryService.GetAllMockTestHistoriesAsync());
app.MapGet("/mocktesthistories/{id}", async (IMockTestHistoryService mockTestHistoryService, string id) => await mockTestHistoryService.GetMockTestHistoryByIdAsync(id));
app.MapPost("/mocktesthistories", async (IMockTestHistoryService mockTestHistoryService, MockTestHistory mockTestHistory) => await mockTestHistoryService.CreateMockTestHistoryAsync(mockTestHistory));
app.MapPut("/mocktesthistories/{id}", async (IMockTestHistoryService mockTestHistoryService, MockTestHistory mockTestHistory) => await mockTestHistoryService.UpdateMockTestHistoryAsync(mockTestHistory));
app.MapDelete("/mocktesthistories/{id}", async (IMockTestHistoryService mockTestHistoryService, string id) => await mockTestHistoryService.DeleteMockTestHistoryAsync(id));

app.MapGet("/", () => "Hello World!");

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
}

void SeedData(IServiceProvider services)
{
    var database = services.GetRequiredService<IMongoDatabase>();

    SeedCollection(database, "users", DummyData.GetUsers());
    SeedCollection(database, "questions", DummyData.GetQuestions());
    SeedCollection(database, "tests", DummyData.GetTests());
    SeedCollection(database, "payments", DummyData.GetPayments());
    SeedCollection(database, "notifications", DummyData.GetNotifications());
    SeedCollection(database, "audit_logs", DummyData.GetAuditLogs());
    SeedCollection(database, "mock_tests", DummyData.GetMockTests());
    SeedCollection(database, "mock_test_histories", DummyData.GetMockTestHistories());
}

void SeedCollection<T>(IMongoDatabase database, string collectionName, List<T> items) where T : IEntity
{
    var collection = database.GetCollection<T>(collectionName);
    var existingItems = collection.Find(_ => true).ToList();
    if (!existingItems.Any())
    {
        collection.InsertMany(items);
    }
}
