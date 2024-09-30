using System.Net;
using System.Text.Json;

namespace BasculasPG.Middleware
{
    public class ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception exception)
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";

                logger.LogError(exception, "Fatal error");

                await context.Response.WriteAsync(new ExceptionHandlerMessage
                {
                    message = $"{exception.Message}"
                }.ToString());
            }
        }
    }

    public class ExceptionHandlerMessage
    {
        public string? message { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
