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

            // Test endpoints
            app.MapGet("/tests", async (ITestService testService) =>
            {
                return await RequestHandler.HandleRequestAsync(async () =>
                {
                    return Results.Ok(await testService.GetAllTestsAsync());
                });
            });
          
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
