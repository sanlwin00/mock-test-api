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
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Antiforgery;
using Serilog;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add anti-forgery services
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-XSRF-TOKEN";
    options.Cookie.Name = "XSRF-TOKEN";
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.None;
});

// Bind Smtp settings from configuration
builder.Services.Configure<SmtpSetting>(builder.Configuration.GetSection("Smtp"));

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

builder.Services.AddHttpClient();
builder.Services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
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
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IEmailService>(provider =>
    new EmailService(
        Path.Combine(Directory.GetCurrentDirectory(), "Templates"),
        provider.GetRequiredService<IOptions<SmtpSetting>>()
        )
    );


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins",
        policy =>
        {
            var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>();
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

var app = builder.Build();

app.UseCors("AllowSpecificOrigins");

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


app.UseAuthentication();
app.UseAuthorization();

// This is for Stripe API
Stripe.StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

// Seed data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    SeedData(services);
}

app.MapGet("/chat/config", (IChatService chatService) =>
{
    return Results.Ok(chatService.LoadChatBotBaseConfiguration());
});

app.MapPost("/chat/send", async (ChatRequest chatRequest, IChatService chatService) =>
{
    try
    {
        var chatResponse = await chatService.GenerateChatResponseAsync(chatRequest.LastPrompt, chatRequest.ConversationHistory, chatRequest.Context);
        return Results.Ok(chatResponse);
    }
    catch (Exception ex)
    {
        Log.Error("Exception occured: {@ex}", ex);
        var response = new { message = ex.Message };
        return Results.Json(response, statusCode: 500);
    }
});

app.MapPost("/emails/send-contact-form", async ([FromForm] ContactFormRequest request, IEmailService emailService) =>
{
    try
    {
        await emailService.SendContactFormEmailAsync(
            request.ToEmail,
            request.FirstName,
            request.LastName,
            request.Phone,
            request.Message,
            request.Attachments
        );

        return Results.Ok("Email sent successfully");
    }
    catch (Exception ex)
    {
        Log.Error("Exception occured: {@ex}", ex);
        var response = new { message = ex.Message };
        return Results.Json(response, statusCode: 500);
    }
});

// User endpoints
//app.MapGet("/users", async (IUserService userService) => await userService.GetAllUsersAsync()).RequireAuthorization("AdminOnly");
//app.MapGet("/users/{id}", async (IUserService userService, string id) => await userService.GetUserByIdAsync(id)).RequireAuthorization();
//app.MapPost("/users", async (IUserService userService, User user) => await userService.CreateUserAsync(user));
//app.MapPut("/users/{id}", async (IUserService userService, User user) => await userService.UpdateUserAsync(user)).RequireAuthorization();
//app.MapDelete("/users/{id}", async (IUserService userService, string id) => await userService.DeleteUserAsync(id)).RequireAuthorization("AdminOnly");
app.MapPatch("/users/{id}", async (IUserService userService, UpdateUserDto updateUserDto, string id) =>
{
    try
    {
        var success = await userService.UpdateUserAsync(id, updateUserDto);
        if (success)
        {
            return Results.NoContent();
        }
        return Results.NotFound();
    }
    catch (Exception ex)
    {
        var response = new { message = ex.Message };
        return Results.Json(response, statusCode: 500);
    }
}).RequireAuthorization();

// Auth endpoints
app.MapPost("/auth/register", async (IUserService userService, RegisterRequest registerDto) =>
{
    try
    {
        var loginResponse = await userService.RegisterUserAsync(registerDto);

        if (loginResponse == null)
            return Results.Unauthorized();
        else
        {
            return Results.Ok(new { loginResponse.Token, loginResponse.User });
        }
    }
    catch (Exception ex)
    {
        var response = new { message = ex.Message };
        return Results.Json(response, statusCode: 500);
    }
});
app.MapPost("/auth/login", async (IUserService userService, LoginRequest loginRequest) =>
{
    try
    {
        var loginResponse = await userService.AuthenticateAsync(loginRequest);

        if (loginResponse == null)
            return Results.Unauthorized();
        else
        {
            return Results.Ok(new { loginResponse.Token, loginResponse.User });
        }
    }
    catch (UnauthorizedAccessException ex)
    {
        var response = new { message = ex.Message };
        return Results.Json(response, statusCode: 401);
    }
    catch (Exception ex)
    {
        var response = new { message = ex.Message };
        return Results.Json(response, statusCode: 500);
    }
});
app.MapPost("/auth/login/accesscode", async (IUserService userService, AccessCodeRequest accessCodeRequest) =>
{
    try
    {
        var loginResponse = await userService.AuthenticateWithAccessCodeAsync(accessCodeRequest.AccessCode);

        if (loginResponse == null)
            throw new UnauthorizedAccessException("Invalid Access Code.");
        else
        {
            return Results.Ok(new { loginResponse.Token, loginResponse.User });
        }
    }
    catch (UnauthorizedAccessException ex)
    {
        var response = new { message = ex.Message };
        return Results.Json(response, statusCode: 401);
    }
    catch (Exception ex)
    {
        var response = new { message = ex.Message };
        return Results.Json(response, statusCode: 500);
    }
});
app.MapGet("/auth/profile", async (HttpContext httpContext, IMapper mapper, IUserService userService) => {
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

    var userDto = mapper.Map<UserDto>(user);
    return Results.Ok(userDto);
}).RequireAuthorization();

app.MapPost("/auth/request-password-reset", async (PasswordResetRequest passwordResetRequest, IUserService userService) =>
{
    var success = await userService.RequestPasswordResetAsync(passwordResetRequest.Email, passwordResetRequest.PasswordResetUrl);
    if (success)
    {
        return Results.Ok("Password reset email sent");
    }
    return Results.NotFound("User not found");
});

app.MapPost("/auth/reset-password", async (PasswordResetDto passwordResetDto, IUserService userService) =>
{
    var success = await userService.ResetPasswordAsync(passwordResetDto.Token, passwordResetDto.Password);
    if (success)
    {
        return Results.Ok("Password has been reset");
    }
    return Results.BadRequest("Invalid or expired link. Please request for a new reset link.");
});

app.MapGet("/auth/csrf-token", (HttpContext context) =>
{
    var tokens = context.RequestServices.GetRequiredService<IAntiforgery>().GetAndStoreTokens(context);

    return Results.Ok(new { token = tokens.RequestToken });
});



// Question endpoints
app.MapGet("/questions", async (IQuestionService questionService) => await questionService.GetAllQuestionsAsync());
//app.MapGet("/questions/{id}", async (IQuestionService questionService, string id) => await questionService.GetQuestionByIdAsync(id));
//app.MapPost("/questions", async (IQuestionService questionService, Question question) => await questionService.CreateQuestionAsync(question));
//app.MapPut("/questions/{id}", async (IQuestionService questionService, Question question) => await questionService.UpdateQuestionAsync(question));
//app.MapDelete("/questions/{id}", async (IQuestionService questionService, string id) => await questionService.DeleteQuestionAsync(id));

// Test endpoints
app.MapGet("/tests", async (ITestService testService) => await testService.GetAllTestsAsync());
//app.MapGet("/tests/{id}", async (ITestService testService, string id) => await testService.GetTestByIdAsync(id));
//app.MapPost("/tests", async (ITestService testService, Test test) => await testService.CreateTestAsync(test));
//app.MapPut("/tests/{id}", async (ITestService testService, Test test) => await testService.UpdateTestAsync(test));
//app.MapDelete("/tests/{id}", async (ITestService testService, string id) => await testService.DeleteTestAsync(id));

// Payment endpoints
//app.MapGet("/payments", async (IPaymentService paymentService) => await paymentService.GetAllPaymentsAsync());
app.MapGet("/payments/validate_session/{sessionId}", async (HttpContext httpContext, IPaymentService paymentService, string sessionId) =>
{
    try
    {
        var userId = httpContext.User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
        if (userId == null)
        {
            return Results.Unauthorized();
        }

        var payment = await paymentService.ValidateSession(sessionId, userId);
        if (payment == null)
        {
            return Results.NotFound("Payment not found or validation failed.");
        }
        return Results.Ok(payment);
    }
    catch (Exception ex)
    {
        var response = new { message = ex.Message };
        return Results.Json(response, statusCode: 500);
    }
}).RequireAuthorization(); ;
app.MapPost("/payments/create_session", async (HttpContext httpContext, IPaymentService paymentService, StripeRequestDto stripeRequestDto) =>
{
    try
    {
        var userId = httpContext.User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
        if (userId == null)
        {
            return Results.Unauthorized();
        }

        var result = await paymentService.CreateSession(stripeRequestDto, userId);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        var response = new { message = ex.Message };
        return Results.Json(response, statusCode: 500);
    }
}).RequireAuthorization();

//app.MapPut("/payments/{id}", async (IPaymentService paymentService, Payment payment) => await paymentService.UpdatePaymentAsync(payment));
//app.MapDelete("/payments/{id}", async (IPaymentService paymentService, string id) => await paymentService.DeletePaymentAsync(id));

/* Notification endpoints
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
*/
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
    services.AddScoped<IRepository<PasswordResetToken>>(sp =>
    {
        var database = sp.GetRequiredService<IMongoDatabase>();
        return new MongoRepository<PasswordResetToken>(database, "password_reset_token");
    });
}

void SeedData(IServiceProvider services)
{
    var database = services.GetRequiredService<IMongoDatabase>();

    SeedCollection(database, "users", DummyData.GetUsers());
    SeedCollection(database, "questions", DummyData.GetQuestions());
    SeedCollection(database, "tests", DummyData.GetTests());
    //SeedCollection(database, "payments", DummyData.GetPayments());
    //SeedCollection(database, "notifications", DummyData.GetNotifications());
    //SeedCollection(database, "audit_logs", DummyData.GetAuditLogs());
    //SeedCollection(database, "mock_tests", DummyData.GetMockTests());
    //SeedCollection(database, "mock_test_histories", DummyData.GetMockTestHistories());
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
