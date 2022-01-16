using System.Net;
using System.Threading.Tasks;
using LineTen.Integration.Tests.Framework.Attributes;
using LineTen.Integration.Tests.Framework.Fixtures;
using Newtonsoft.Json.Linq;
using Xunit;

namespace LineTen.Analytics.Api.Tests.Integration.Controllers.Swagger
{
    [TestCaseOrderer(SequenceTestCaseOrderer.OrdererTypeName, SequenceTestCaseOrderer.OrdererAssemblyName)]
    public class when_requesting_the_swagger_doc : IClassFixture<LoopbackTestFixture<InMemoryDbStartup>>
    {
        private readonly LoopbackTestFixture<InMemoryDbStartup> _fixture;
        public when_requesting_the_swagger_doc(LoopbackTestFixture<InMemoryDbStartup> fixture)
        {
            _fixture = fixture;
        }
        [Fact]
        public async Task it_should_return_ok_async()
        {
            var response = await _fixture.HttpClient.GetAsync($"/swagger/v1/swagger.json");
            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var jToken = JToken.Parse(content);
            Assert.Equal(JTokenType.Object, jToken.Type);
            Assert.NotEmpty(jToken.AsJEnumerable());
        }
    }
}
