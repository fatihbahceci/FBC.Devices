
internal class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }
}

public static class ExceptionHandlerExtensions
{
    public static IApplicationBuilder AddExceptionHandlerMiddleware(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                context.Response.ContentType = "application/json";
                var exceptionHandlerPathFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
                if (exceptionHandlerPathFeature?.Error is NotFoundException)
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                }
                else
                {
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                }
                var result = System.Text.Json.JsonSerializer.Serialize(new { error = exceptionHandlerPathFeature?.Error.Message });
                await context.Response.WriteAsync(result);
            });
        });
        return app;
    }
}
