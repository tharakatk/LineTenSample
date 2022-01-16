using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace LineTen.Analytics.Api.Tests.Integration.Controllers.HealthCheck
{
    public class TestableHealthCheck : IHealthCheck
    {
        public static HealthStatus HealthStatus;
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.FromResult(new HealthCheckResult(HealthStatus));
        }
    }
}
