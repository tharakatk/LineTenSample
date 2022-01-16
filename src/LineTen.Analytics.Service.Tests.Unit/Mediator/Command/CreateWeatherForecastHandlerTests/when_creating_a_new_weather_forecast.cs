using System.Threading;
using LineTen.DataAccess.EntityFramework.Repository;
using LineTen.Analytics.Domain.Entities;
using LineTen.Analytics.Service.Mediator.Command;
using LineTen.Unit.Tests.Framework;
using Moq;
using Xunit;

namespace LineTen.Analytics.Service.Tests.Unit.Mediator.Command.CreateWeatherForecastHandlerTests
{
    public class when_creating_a_new_weather_forecast : WithSubject<CreateWeatherForecastHandler>
    {
        private readonly WeatherForecast _result;

        public when_creating_a_new_weather_forecast()
        {
            The<IRepository<WeatherForecast>>().Setup(x => x.Add(It.IsAny<WeatherForecast>())).Returns(new WeatherForecast());
            _result = Subject.Handle(new CreateWeatherForecastCommand(), CancellationToken.None).Result;
        }
        [Fact]
        public void it_should_add_the_object_to_the_repository()
        {
            The<IRepository<WeatherForecast>>().Verify(x => x.Add(It.IsAny<WeatherForecast>()),Times.Once);
        }

        [Fact]
        public void it_should_return_a_non_null_result()
        {
            Assert.NotNull(_result);
        }
    }
}
