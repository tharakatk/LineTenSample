using System.Threading;
using System.Threading.Tasks;
using LineTen.Analytics.Data.Database;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace LineTen.Analytics.Api.HealthChecks
{
    /// <summary>
    /// A very simple database connectivity check
    /// https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks
    /// </summary>
    public class DatabaseConnectivityHealthCheck : IHealthCheck
    {
        private readonly ApplicationDbContext _dbContext;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="dbContext"></param>
        public DatabaseConnectivityHealthCheck(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <inheritdoc />
        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var status = _dbContext.Database.CanConnect() ? HealthStatus.Healthy : HealthStatus.Unhealthy;
            return Task.FromResult(new HealthCheckResult(status, "Reports unhealthy status if database unavailable"));
        }
    }

   
}
