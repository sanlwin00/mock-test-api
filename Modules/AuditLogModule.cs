using Carter;
using MockTestApi.Services.Interfaces;

namespace MockTestApi.Modules
{
    public class AuditLogModule : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/audit-logs", async (IAuditLogService auditLogService, int pageNumber = 1, int pageSize = 10, string? searchKeyword= null) =>
            {
                return await RequestHandler.HandleRequestAsync(async () =>
                {
                    var result = await auditLogService.GetAuditLogsAsync(pageNumber, pageSize, searchKeyword);
                    return Results.Ok(result);
                });
            });
        }
    }
}
