using System;
using System.Net;

namespace Xylab.BricksService
{
    public sealed class BricksException : Exception
    {
        public HttpStatusCode StatusCode { get; }

        public BricksException(string message, HttpStatusCode statusCode)
            : base(message)
        {
            StatusCode = statusCode;
        }

        public BricksException(Exception innerException, string message, HttpStatusCode statusCode)
            : base(message, innerException)
        {
            StatusCode = statusCode;
        }
    }
}
