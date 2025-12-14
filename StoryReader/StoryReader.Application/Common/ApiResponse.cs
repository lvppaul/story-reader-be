using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryReader.Application.Common
{
    public class ApiResponse<T>
    {
        public bool Success => true;
        public T Data { get; init; } = default!;
        public string? Message { get; init; }

        public static ApiResponse<T> Ok(T data, string? message = null)
            => new() { Data = data, Message = message };
    }
}
