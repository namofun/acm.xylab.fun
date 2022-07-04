using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.Functions
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class FunctionAuthorizeAttribute : Attribute, IFunctionExceptionFilter, IFunctionInvocationFilter
    {
        public string Role { get; }

        public FunctionAuthorizeAttribute(string role)
        {
            Role = role;
        }

        public async Task OnExceptionAsync(
            FunctionExceptionContext exceptionContext,
            CancellationToken cancellationToken)
        {
            if (ContextAccessor.Http != null
                && exceptionContext.Exception is FunctionInvocationException
                && exceptionContext.Exception.InnerException is AuthorizationException)
            {
                if (exceptionContext.Exception.InnerException != null && exceptionContext.Exception.InnerException is AuthorizationException)
                {
                    ContextAccessor.Http.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await ContextAccessor.Http.Response.WriteAsync("", cancellationToken);
                }
            }
        }

        public Task OnExecutedAsync(FunctionExecutedContext executedContext, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task OnExecutingAsync(FunctionExecutingContext executingContext, CancellationToken cancellationToken)
        {
            if (ContextAccessor.Http != null
                && ContextAccessor.Http.User.FindFirst(c => c.Type == "roles" && c.Value == Role) == null
                && Environment.GetEnvironmentVariable("AzureWebJobsStorage") != "UseDevelopmentStorage=true")
            {
                throw new AuthorizationException();
            }

            return Task.CompletedTask;
        }

        private class AuthorizationException : Exception
        {
        }
    }
}
