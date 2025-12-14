using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryReader.Application.Common
{
    public class ErrorResponse
    {
        public bool Success => false;

        public ErrorDetail Error { get; init; } = default!;
    }

    public class ErrorDetail
    {
        public string Code { get; init; } = default!;
        public string Message { get; init; } = default!;
        public object? Details { get; init; }
    }
}
