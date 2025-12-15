using Microsoft.AspNetCore.Mvc;
using StoryReader.Application.Common;

namespace StoryReader.Api.Extensions
{
    public static class ControllerExtensions
    {
        public static IActionResult OkResponse<T>(
            this ControllerBase controller,
            T data,
            string? message = null)
        {
            return controller.Ok(ApiResponse<T>.Ok(data, message));
        }

        public static IActionResult OkMessage(
           this ControllerBase controller,
           string message)
        {
            return controller.Ok(ApiResponse<object>.Ok(null, message));
        }
    }
}
