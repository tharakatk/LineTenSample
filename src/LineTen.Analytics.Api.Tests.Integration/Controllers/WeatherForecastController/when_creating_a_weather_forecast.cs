using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using LineTen.Integration.Tests.Framework.Attributes;
using LineTen.Integration.Tests.Framework.Fixtures;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace LineTen.Analytics.Api.Tests.Integration.Controllers.WeatherForecastController
{

    [TestCaseOrderer(SequenceTestCaseOrderer.OrdererTypeName, SequenceTestCaseOrderer.OrdererAssemblyName)]
    public class when_creating_a_weather_forecast:IClassFixture<LoopbackTestFixture<InMemoryDbStartup>> //https://xunit.net/docs/shared-context
    {
        private readonly LoopbackTestFixture<InMemoryDbStartup> _fixture;

        public when_creating_a_weather_forecast(LoopbackTestFixture<InMemoryDbStartup> fixture)
        {
            _fixture = fixture;
        }

        [Fact, Sequence(1)]
        public async Task it_should_initially_have_no_weather_forecasts()
        {
            var response = await _fixture.HttpClient.GetAsync($"weatherforecast");
            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var jToken = JToken.Parse(content);
            Assert.Equal(JTokenType.Array, jToken.Type);
            Assert.Empty(jToken.AsJEnumerable());
        }

        [Fact, Sequence(2)]
        public async Task it_should_return_unauthorized_for_anonymous_user()
        {
            var obj = JObject.Parse("{\r\n  \"date\": \"2020-10-04T12:03:52.172Z\",\r\n  \"temperatureC\": 0,\r\n  \"summary\": \"string\"\r\n}");
            var response = await _fixture.HttpClient.PostAsync($"weatherforecast", new StringContent(JsonConvert.SerializeObject(obj),Encoding.UTF8,MediaTypeNames.Application.Json));
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact, Sequence(3)]
        public async Task it_should_return_forbidden_for_a_user_without_the_relevant_claim()
        {
            _fixture.ClaimsIdentity = new ClaimsIdentity(new List<Claim>{new Claim("sub", "id"), new Claim("scope", "lineten.analytics.read")}, "test");
            var obj = JObject.Parse("{\r\n  \"date\": \"2020-10-04T12:03:52.172Z\",\r\n  \"temperatureC\": 0,\r\n  \"summary\": \"string\"\r\n}");
            var response = await _fixture.HttpClient.PostAsync($"weatherforecast", new StringContent(JsonConvert.SerializeObject(obj),Encoding.UTF8,MediaTypeNames.Application.Json));
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact, Sequence(4)]
        public async Task it_should_accept_a_new_weather_forecast()
        {
            _fixture.ClaimsIdentity = new ClaimsIdentity(new List<Claim>{new Claim("sub", "id"), new Claim("scope", "lineten.analytics.write")}, "test");
            var obj = JObject.Parse("{\r\n  \"date\": \"2020-10-04T12:03:52.172Z\",\r\n  \"temperatureC\": 0,\r\n  \"summary\": \"string\"\r\n}");
            var response = await _fixture.HttpClient.PostAsync($"weatherforecast", new StringContent(JsonConvert.SerializeObject(obj),Encoding.UTF8,MediaTypeNames.Application.Json));
            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            _fixture.PropertyBag.Location = response.Headers.Location;
        }

        [Fact, Sequence(5)]
        public async Task it_should_return_a_single_new_foecast()
        {
            var response = await _fixture.HttpClient.GetAsync(_fixture.PropertyBag.Location);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact, Sequence(6)]
        public async Task it_should_return_not_found_for_invalid_forecast()
        {
            var response = await _fixture.HttpClient.GetAsync($"weatherforecast/{Guid.NewGuid()}");
            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact, Sequence(7)]
        public async Task it_should_reject_an_invalid_forecast()
        {
            var obj = JObject.Parse("{}");
            var response = await _fixture.HttpClient.PostAsync($"weatherforecast", new StringContent(JsonConvert.SerializeObject(obj),Encoding.UTF8,MediaTypeNames.Application.Json));
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
