using Carter;
using MockTestApi.Services.Interfaces;

namespace MockTestApi.Endpoints
{
    public class TestAndQuestionModule : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            
            // Question endpoints
            app.MapGet("/questions", async (IQuestionService questionService) =>
            {
                return await RequestHandler.HandleRequestAsync(async () =>
                {
                    return Results.Ok(await questionService.GetAllQuestionsAsync());
                });
            });

            //app.MapGet("/questions/{id}", async (IQuestionService questionService, string id) => await questionService.GetQuestionByIdAsync(id));
            //app.MapPost("/questions", async (IQuestionService questionService, Question question) => await questionService.CreateQuestionAsync(question));
            //app.MapPut("/questions/{id}", async (IQuestionService questionService, Question question) => await questionService.UpdateQuestionAsync(question));
            //app.MapDelete("/questions/{id}", async (IQuestionService questionService, string id) => await questionService.DeleteQuestionAsync(id));

            // Test endpoints
            app.MapGet("/tests", async (ITestService testService) =>
            {
                return await RequestHandler.HandleRequestAsync(async () =>
                {
                    return Results.Ok(await testService.GetAllTestsAsync());
                });
            });
            //app.MapGet("/tests/{id}", async (ITestService testService, string id) => await testService.GetTestByIdAsync(id));
            //app.MapPost("/tests", async (ITestService testService, Test test) => await testService.CreateTestAsync(test));
            //app.MapPut("/tests/{id}", async (ITestService testService, Test test) => await testService.UpdateTestAsync(test));
            //app.MapDelete("/tests/{id}", async (ITestService testService, string id) => await testService.DeleteTestAsync(id));

            // ReferenceMaterial endpoints
            app.MapGet("/references/{id}", async (IReferenceMaterialService referenceMaterialService, string id) => {
                return await RequestHandler.HandleRequestAsync(async () =>
                { 
                    return Results.Ok(await referenceMaterialService.GetReferenceMaterialByIdAsync(id));
                });
            });

        }
    }
}
