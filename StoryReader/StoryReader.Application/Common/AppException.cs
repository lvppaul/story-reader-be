using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryReader.Application.Common
{
    public sealed class AppException : Exception
    {
        public string Code { get; }
        public int StatusCode { get; }

        private AppException(string code, string message, int statusCode)
            : base(message)
        {
            Code = code;
            StatusCode = statusCode;
        }

        // ---------- FACTORY METHODS ----------

        public static AppException BadRequest(string code, string message) =>
            new(code, message, HttpStatus.BadRequest);

        public static AppException Unauthorized(string code, string message) =>
            new(code, message, HttpStatus.Unauthorized);

        public static AppException Forbidden(string code, string message) =>
            new(code, message, HttpStatus.Forbidden);

        public static AppException NotFound(string code, string message) =>
            new(code, message, HttpStatus.NotFound);

        public static AppException Conflict(string code, string message) =>
            new(code, message, HttpStatus.Conflict);
    }
}
