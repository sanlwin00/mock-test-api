using Serilog;

namespace MockTestApi
{
    public static class RequestHandler
    {
        public static async Task<IResult> HandleRequestAsync(Func<Task<IResult>> action)
        {
            try
            {
                return await action();
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("Unauthorized", ex);
                return Results.Json(new { message = ex.Message }, statusCode: 401);
            }
            catch (Exception ex)
            {
                Log.Error("Exception occured", ex);
                return Results.Json(new { message = ex.Message }, statusCode: 500);
            }
        }
    }
}
