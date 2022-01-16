using System.Net;
using System.Threading.Tasks;
using LineTen.Integration.Tests.Framework.Attributes;
using LineTen.Integration.Tests.Framework.Fixtures;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Xunit;

namespace LineTen.Analytics.Api.Tests.Integration.Controllers.HealthCheck
{
    public class when_calling_health_check_endpoint :IClassFixture<LoopbackTestFixture<InMemoryDbStartup>> //https://xunit.net/docs/shared-context
    {
        private readonly LoopbackTestFixture<InMemoryDbStartup> _fixture;

        public when_calling_health_check_endpoint(LoopbackTestFixture<InMemoryDbStartup> fixture)
        {
            _fixture = fixture;
        }

        [Fact, Sequence(1)]
        public async Task it_should_return_a_healthy_result()
        {
            TestableHealthCheck.HealthStatus = HealthStatus.Healthy;
            var response = await _fixture.HttpClient.GetAsync($"health");
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Healthy", content);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact, Sequence(2)]
        public async Task it_should_return_an_unhealthy_result()
        {
            TestableHealthCheck.HealthStatus = HealthStatus.Unhealthy;
            var response = await _fixture.HttpClient.GetAsync($"health");
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Unhealthy", content);
            Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        }

        [Fact, Sequence(3)]
        public async Task it_should_return_a_degraded_result()
        {
            TestableHealthCheck.HealthStatus = HealthStatus.Degraded;
            var response = await _fixture.HttpClient.GetAsync($"health");
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Degraded", content);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
