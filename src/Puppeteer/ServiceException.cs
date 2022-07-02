using System;
using System.Net;

namespace Xylab.BricksService.Puppeteer
{
    public sealed class ServiceException : Exception
    {
        public HttpStatusCode StatusCode { get; }

        public ServiceException(string message, HttpStatusCode statusCode)
            : base(message)
        {
            StatusCode = statusCode;
        }

        public ServiceException(Exception innerException, string message, HttpStatusCode statusCode)
            : base(message, innerException)
        {
            StatusCode = statusCode;
        }
    }
}
