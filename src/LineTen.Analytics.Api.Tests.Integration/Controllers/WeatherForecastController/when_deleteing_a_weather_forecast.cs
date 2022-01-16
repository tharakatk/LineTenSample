using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using LineTen.Integration.Tests.Framework.Attributes;
using LineTen.Integration.Tests.Framework.Fixtures;
using Newtonsoft.Json.Linq;
using Xunit;

namespace LineTen.Analytics.Api.Tests.Integration.Controllers.WeatherForecastController
{
    [TestCaseOrderer(SequenceTestCaseOrderer.OrdererTypeName, SequenceTestCaseOrderer.OrdererAssemblyName)]
    public class when_deleteing_a_weather_forecast :IClassFixture<LoopbackTestFixture<InMemoryDbStartup>> //https://xunit.net/docs/shared-context
    {
        private readonly LoopbackTestFixture<InMemoryDbStartup> _fixture;
        public when_deleteing_a_weather_forecast(LoopbackTestFixture<InMemoryDbStartup> fixture)
        {
            _fixture = fixture;
        }
        [Fact, Sequence(1)]
        public async Task it_should_return_unauthorized_if_user_isnt_authenticated()
        {
            var response = await _fixture.HttpClient.DeleteAsync($"weatherforecast/{Guid.NewGuid()}");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact, Sequence(2)]
        public async Task it_should_return_forbidden_if_user_isnt_authorized()
        {
            //user doesn't have the correct claim
            _fixture.ClaimsIdentity = new ClaimsIdentity(new List<Claim>{new Claim("sub", "id"), new Claim("scope", "template.microservice.read")}, "test");
            var response = await _fixture.HttpClient.DeleteAsync($"weatherforecast/{Guid.NewGuid()}");
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact, Sequence(3)]
        public async Task it_should_throw_an_error_due_to_not_implemented()
        {
            //user has the correct claim
            _fixture.ClaimsIdentity = new ClaimsIdentity(new List<Claim>{new Claim("sub", "id"), new Claim("scope", "lineten.analytics.write")}, "test");

            await Assert.ThrowsAsync<NotImplementedException>(async delegate
            {
                await _fixture.HttpClient.DeleteAsync($"weatherforecast/{Guid.NewGuid()}");
            });
            
        }
    }
}
