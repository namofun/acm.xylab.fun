#pragma warning disable CS0618

global using FunctionInvocationException = Microsoft.Azure.WebJobs.Host.FunctionInvocationException;

using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Interop = Microsoft.Azure.WebJobs.Host;

[assembly: FunctionsStartup(typeof(Microsoft.Azure.Functions.ContextAccessor.Startup))]

namespace Microsoft.Azure.Functions
{
    public abstract class FunctionFilterContext
    {
        private readonly Interop.FunctionFilterContext _context;

        public Guid FunctionInstanceId => _context.FunctionInstanceId;
        public string FunctionName => _context.FunctionName;
        public IDictionary<string, object> Properties => _context.Properties;
        public ILogger Logger => _context.Logger;

        protected FunctionFilterContext(Interop.FunctionFilterContext context)
        {
            _context = context;
        }
    }

    public class FunctionExceptionContext : FunctionFilterContext
    {
        private readonly Interop.FunctionExceptionContext _context;

        public Exception Exception => ExceptionDispatchInfo.SourceException;
        public ExceptionDispatchInfo ExceptionDispatchInfo => _context.ExceptionDispatchInfo;

        public FunctionExceptionContext(
            Interop.FunctionExceptionContext exceptionContext)
            : base(exceptionContext)
        {
            _context = exceptionContext;
        }
    }

    public abstract class FunctionInvocationContext : FunctionFilterContext
    {
        private readonly Interop.FunctionInvocationContext _context;

        public IReadOnlyDictionary<string, object> Arguments => _context.Arguments;

        protected FunctionInvocationContext(
            Interop.FunctionInvocationContext context)
            : base(context)
        {
            _context = context;
        }
    }

    public class FunctionExecutingContext : FunctionInvocationContext
    {
        public FunctionExecutingContext(
            Interop.FunctionExecutingContext context)
            : base(context)
        {
        }
    }

    public class FunctionExecutedContext : FunctionInvocationContext
    {
        private readonly Interop.FunctionExecutedContext _context;

        public FunctionResult FunctionResult => _context.FunctionResult;

        public FunctionExecutedContext(
            Interop.FunctionExecutedContext context)
            : base(context)
        {
            _context = context;
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public abstract class FunctionExceptionFilterAttribute : Interop.FunctionExceptionFilterAttribute
    {
        public override sealed Task OnExceptionAsync(
            Interop.FunctionExceptionContext exceptionContext,
            CancellationToken cancellationToken)
        {
            return this.OnExceptionAsync(
                new FunctionExceptionContext(exceptionContext),
                cancellationToken);
        }

        public abstract Task OnExceptionAsync(
            FunctionExceptionContext exceptionContext,
            CancellationToken cancellationToken);
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public abstract class FunctionInvocationFilterAttribute : Interop.FunctionInvocationFilterAttribute
    {
        public virtual Task OnExecutingAsync(
            FunctionExecutingContext executingContext,
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public virtual Task OnExecutedAsync(
            FunctionExecutedContext executedContext,
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public override sealed Task OnExecutedAsync(
            Interop.FunctionExecutedContext executedContext,
            CancellationToken cancellationToken)
        {
            return this.OnExecutedAsync(
                new FunctionExecutedContext(executedContext),
                cancellationToken);
        }

        public override sealed Task OnExecutingAsync(
            Interop.FunctionExecutingContext executingContext,
            CancellationToken cancellationToken)
        {
            return this.OnExecutingAsync(
                new FunctionExecutingContext(executingContext),
                cancellationToken);
        }
    }

    public interface IFunctionExceptionFilter : Interop.IFunctionExceptionFilter
    {
        Task OnExceptionAsync(
            FunctionExceptionContext exceptionContext,
            CancellationToken cancellationToken);

        Task Interop.IFunctionExceptionFilter.OnExceptionAsync(
            Interop.FunctionExceptionContext exceptionContext,
            CancellationToken cancellationToken)
        {
            return this.OnExceptionAsync(
                new FunctionExceptionContext(exceptionContext),
                cancellationToken);
        }
    }

    public interface IFunctionInvocationFilter : Interop.IFunctionInvocationFilter
    {
        Task OnExecutingAsync(
            FunctionExecutingContext executingContext,
            CancellationToken cancellationToken);

        Task OnExecutedAsync(
            FunctionExecutedContext executedContext,
            CancellationToken cancellationToken);

        Task Interop.IFunctionInvocationFilter.OnExecutingAsync(
            Interop.FunctionExecutingContext executingContext,
            CancellationToken cancellationToken)
        {
            return this.OnExecutingAsync(
                new FunctionExecutingContext(executingContext),
                cancellationToken);
        }

        Task Interop.IFunctionInvocationFilter.OnExecutedAsync(
            Interop.FunctionExecutedContext executedContext,
            CancellationToken cancellationToken)
        {
            return this.OnExecutedAsync(
                new FunctionExecutedContext(executedContext),
                cancellationToken);
        }
    }

    public class ContextAccessor
    {
        private static IHttpContextAccessor _httpContextAccessor;

        public static HttpContext Http => _httpContextAccessor.HttpContext;

        internal class Configurator : IConfigureOptions<WebJobs.JobHostOptions>
        {
            public Configurator(IHttpContextAccessor httpContextAccessor)
            {
                _httpContextAccessor = httpContextAccessor;
            }

            public void Configure(WebJobs.JobHostOptions options)
            {
            }
        }

        internal class Startup : FunctionsStartup
        {
            public override void Configure(IFunctionsHostBuilder builder)
            {
                builder.Services.ConfigureOptions<Configurator>();
            }
        }
    }
}
