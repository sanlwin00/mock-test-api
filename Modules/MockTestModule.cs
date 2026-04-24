using Carter;
using MockTestApi.Models;
using MockTestApi.Services.Interfaces;
using System.Security.Claims;

namespace MockTestApi.Modules
{
    public class MockTestModule : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/mock-tests/start", async (MockTest mockTestDto, IMockTestService mockTestService, IAuditLogService auditLogService) =>
            {
                return await RequestHandler.HandleRequestAsync(async () =>
                {
                    var mockTest = new MockTest
                    {
                        Id = mockTestDto.Id,
                        Title = mockTestDto.Title,
                        Description = mockTestDto.Description,
                        TestId = mockTestDto.TestId,
                        UserId = mockTestDto.UserId,
                        CreatedAt = mockTestDto.CreatedAt,
                        UpdatedAt = mockTestDto.UpdatedAt,
                        Questions = mockTestDto.Questions.Select(q => new MockTestQuestion
                        {
                            QuestionId = q.QuestionId,
                            SelectedOption = q.SelectedOption ?? -1,
                            UserAnswer = q.UserAnswer,
                            ReviewLater = q.ReviewLater,
                            Number = q.Number
                        }).ToList()
                    };

                    await mockTestService.CreateAsync(mockTestDto);
                    await auditLogService.CreateAuditLogAsync("Started", "MockTest", mockTest.Id);
                    return Results.Created($"/mock-tests/{mockTest.Id}", mockTest);
                });
            });

            app.MapPatch("/mock-tests/progress/{id}", async (string id, UpdateMockTestDto updateMockTestDto, IMockTestService mockTestService, IAuditLogService auditLogService) =>
            {
                return await RequestHandler.HandleRequestAsync(async () =>
                {
                    var success = await mockTestService.UpdateProgressAsync(id, updateMockTestDto);
                    if (!success) return Results.NotFound(); 
                    
                    return Results.NoContent();
                });
            });

            app.MapPut("/mock-tests/complete/{id}", async (string id, CompleteMockTestDto completeMockTestDto, IMockTestService mockTestService, IAuditLogService auditLogService) =>
            {
                return await RequestHandler.HandleRequestAsync(async () =>
                {
                    var success = await mockTestService.CompleteTestAsync(id, completeMockTestDto);
                    if (!success) return Results.NotFound();

                    await auditLogService.CreateAuditLogAsync("Completed", "MockTest", id, newValues: completeMockTestDto);
                    return Results.NoContent();
                });
            });

            app.MapGet("/mock-tests/user-count", async (IAuditLogService auditLogService) =>
            {
                return await RequestHandler.HandleRequestAsync(async () =>
                {
                    var result = await auditLogService.GetMockTestUserCountAsync();
                    return Results.Ok(result);
                });
            });

            app.MapGet("/mock-tests/my", async (HttpContext httpContext, IMockTestService mockTestService) =>
            {
                return await RequestHandler.HandleRequestAsync(async () =>
                {
                    var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (string.IsNullOrEmpty(userId))
                        return Results.Unauthorized();

                    var mockTests = await mockTestService.GetMockTestsByUserIdAsync(userId);
                    return Results.Ok(mockTests);
                });
            });

            app.MapGet("/mock-tests/{id}", async (string id, HttpContext httpContext, IMockTestService mockTestService) =>
            {
                return await RequestHandler.HandleRequestAsync(async () =>
                {
                    var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (string.IsNullOrEmpty(userId))
                        return Results.Unauthorized();

                    var mockTest = await mockTestService.GetMockTestByIdAsync(id);
                    if (mockTest == null)
                        return Results.NotFound();
                    if (mockTest.UserId != userId)
                        return Results.Forbid();

                    return Results.Ok(mockTest);
                });
            });
        }
    }

}