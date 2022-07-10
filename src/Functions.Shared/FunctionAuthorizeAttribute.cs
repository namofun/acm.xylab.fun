using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.Functions
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class FunctionAuthorizeAttribute : Attribute, IFunctionInvocationFilter
    {
        public string Role { get; }

        public static object Key { get; } = typeof(FunctionAuthorizeAttribute);

        public FunctionAuthorizeAttribute(string role)
        {
            Role = role;
        }

        public static bool IsDevelopmentEnvironment()
        {
            return Environment.GetEnvironmentVariable("AzureWebJobsStorage") == "UseDevelopmentStorage=true";
        }

        public bool IsAuthorized()
        {
            if (IsDevelopmentEnvironment())
            {
                // In development environment we don't need to authenticate.
                return true;
            }

            return ContextAccessor.Http.User.HasClaim(c => c.Type == "roles" && c.Value == Role);
        }

        public Task OnExecutedAsync(FunctionExecutedContext executedContext, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task OnExecutingAsync(FunctionExecutingContext executingContext, CancellationToken cancellationToken)
        {
            if (ContextAccessor.Http != null && !IsDevelopmentEnvironment())
            {
                ContextAccessor.Http.Items[Key] = this;
            }

            return Task.CompletedTask;
        }
    }
}
