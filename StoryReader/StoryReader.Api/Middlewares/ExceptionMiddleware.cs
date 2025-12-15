using StoryReader.Application.Common;
using System.Net;
using System.Text.Json;

namespace StoryReader.Api.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IWebHostEnvironment _env;

        public ExceptionMiddleware(
            RequestDelegate next,
            ILogger<ExceptionMiddleware> logger,
            IWebHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (AppException ex)
            {
                await WriteError(
                    context,
                    ex.StatusCode,
                    ex.Code,
                    ex.Message,
                    "Bị lỗi rồi");
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception");

                await WriteError(
                    context,
                    StatusCodes.Status500InternalServerError,
                    "INTERNAL_SERVER_ERROR",
                    "Something went wrong",
                    _env.IsDevelopment() ? ex.Message : null
                );
            }
        }

        private async Task WriteError(
            HttpContext context,
            int statusCode,
            string code,
            string message,
            object? details)
        {
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            var response = new ErrorResponse
            {
                Error = new ErrorDetail
                {
                    Code = code,
                    Message = message,
                    Details = details
                }
            };

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(response));
        }
    }

}
