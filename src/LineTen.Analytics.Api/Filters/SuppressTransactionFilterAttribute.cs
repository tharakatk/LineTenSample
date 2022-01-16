using Microsoft.AspNetCore.Mvc.Filters;

namespace LineTen.Analytics.Api.Filters
{
    /// <summary>
     /// Marker attribute to exclude a specific controller or action participating in a transaction
    /// </summary>
    public class SuppressTransactionFilterAttribute : ActionFilterAttribute
    {
    }

}
