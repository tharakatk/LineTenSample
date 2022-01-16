using System.Linq;
using System.Net.Http;
using System.Transactions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace LineTen.Analytics.Api.Filters
{
    /// <inheritdoc />
    /// <summary>
    /// Ensures that API calls are transactional, commit only occurs when StatusCode is success and exception is null
    /// By default, any GET request does not participate in transactions to increase performance
    /// </summary>
    internal class GlobalTransactionScopeActionFilterAttribute : ActionFilterAttribute
    {
        private TransactionScope _transactionScope;
        private readonly ILogger<GlobalTransactionScopeActionFilterAttribute> _logger;

        public GlobalTransactionScopeActionFilterAttribute(ILogger<GlobalTransactionScopeActionFilterAttribute> logger)
        {
            _logger = logger;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.Filters.OfType<ForceTransactionFilterAttribute>().Any())
            {
                _transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            }
            else
            {
                if (context.HttpContext.Request.Method != HttpMethod.Get.Method)
                {
                    if (!context.Filters.OfType<SuppressTransactionFilterAttribute>().Any())
                    {
                        _transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
                    }
                }
            }
            
            base.OnActionExecuting(context);
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            if (_transactionScope == null)
            {
                base.OnActionExecuted(context);
            }
            else
            {
                using (_transactionScope)
                {
                    base.OnActionExecuted(context);
                    switch (context.Exception)
                    {
                        case null when context.Result is ObjectResult result && result.StatusCode >= 300:
                            _logger.LogWarning("Rolling back transaction due to status code {0}", result.StatusCode);
                            break;
                        case null:
                            _logger.LogDebug("Commiting transaction");
                            _transactionScope.Complete();
                            break;
                        default:
                            _logger.LogError("Rolling back transaction due to exception");
                            break;
                    }
                }
            }
        }
    }
}
